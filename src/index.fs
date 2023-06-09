module App

open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Browser.Dom
open Browser.Types
open Import.Jupyter

//sugar for creating js objects
let inline (!!) x = createObj x
let inline (=>) x y = x ==> y

[<StringEnum>]
type Message =
    | UserMessage
    | BotMessage
    | ErrorMessage

[<ImportMember("@jupyterlab/apputils")>]
let ICommandPalette : obj = jsNative

type [<AllowNullLiteral>] MarkdownIt =
    abstract render: md: string * ?env: obj option -> string

type [<AllowNullLiteral>] MarkdownItConstructor =
    [<Emit "$0($1...)">] abstract Invoke: unit -> MarkdownIt
    
// type [<AllowNullLiteral>] MarkdownItConstructorStatic =
//     [<Emit "new $0($1...)">] abstract Create: unit -> MarkdownItConstructor

let [<Import("default", from="markdown-it")>] Exports: MarkdownItConstructor = jsNative

let markdownIt = Exports.Invoke()

// type IMarkdownIt =
//     abstract render : text:string -> string

// [<ImportAll("markdown-it")>]
// let MarkdownIt: IMarkdownIt = jsNative

// [<ImportAll("markdown-it")>]
// let MarkdownIt: obj = jsNative

// [<ImportMember("markdown-it/render")>]
// let render( markdown : string) : string = jsNative


//Some UI elements are declared outside the extension object so we can refer to them in functions
//This is a lightweight alternative to using class syntax on the extension

let history_div = Browser.Dom.document.createElement("div")
history_div.setAttribute("style","width: 100%; max-width: 100%;height:440px;overflow: auto;") 
history_div.id <- "history_div"
let user_input = Browser.Dom.document.createElement("textarea")
user_input.setAttribute("style","width: 100%; max-width: 100%")
user_input.id <- "user_input"
let send_button : HTMLButtonElement = document.createElement ("button") :?> HTMLButtonElement
send_button.setAttribute("style","width: 100%; max-width: 100%")
send_button.id <- "send_button"
send_button.innerText <- "Send"

let appendToHistory( response ) ( message_type : Message)=
    let newDiv = Browser.Dom.document.createElement("div")
    newDiv.innerHTML <- response
    match message_type with
    | BotMessage -> newDiv.setAttribute("style","background-color:LemonChiffon;") //"width: 100%; max-width: 100%;height:440px;overflow: auto;")
    | UserMessage -> newDiv.setAttribute("style","background-color:LightGrey;")
    | ErrorMessage -> newDiv.setAttribute("style","background-color:Tomato;")

    history_div.appendChild(newDiv) |> ignore

    //scroll into view
    history_div.scrollTop <- history_div.scrollHeight;

let sendUserInput() =
    // get  user message from textarea
    let user_message = user_input?value

    //add user message to history
    appendToHistory user_message UserMessage

    //disable send button while we wait
    send_button.disabled <- true
    send_button.innerText <- "Wait"
    
    // send to API, get bot response
    promise {
        let! coding_assistance_response = StarChatAPI.SendMessage user_message
        match coding_assistance_response with
        | Ok( ok ) ->
            //log
            Logging.LogToServer( Logging.StarChatLogEntry060623.Create user_message ok.bot_response )

            //format markdown
            let html = markdownIt.render(ok.bot_response.Replace("<|end|>",""))

            //update UI
            appendToHistory html BotMessage
            
        | Error( e  ) ->
            //update UI with the error
            appendToHistory ("An error occurred when contacting StarChat. See your browser console for more information.") ErrorMessage
            console.log(e)

        //enable button
        send_button.disabled <- false
        send_button.innerText <- "Send"
    }
    // ()

let extension =
    !! [
        "id" => "jupyterlab_starchat_extension"
        "autoStart" => true
        "requires" => [| ICommandPalette |] 
        //------------------------------------------------------------------------------------------------------------
        //NOTE: this **must** be wrapped in a Func, otherwise the arguments are tupled and Jupyter doesn't expect that
        //------------------------------------------------------------------------------------------------------------
        "activate" =>  System.Func<JupyterFrontEnd,ICommandPalette,unit>( fun app palette ->
            console.log("JupyterLab extension jupyterlab_starchat_extension is activated!");
            // Create a blank content widget inside of a MainAreaWidget
            let content = PhosphorWidgets.Widget.Create();
            let widget = JupyterlabApputils.MainAreaWidget.Create( !![ "content" => content ]);
            widget.id <- "jupyterlab_starchat_extension";
            widget.title.label <- "StarChat Coding Assistant";
            widget.title.closable <- Some(true);

            // Add chat history to widget
            //history_div.innerText <- "Hello from Fable"
            content.node.appendChild(history_div) |> ignore

            // Add user input to widget
            content.node.appendChild(user_input) |> ignore

            // Add send button to widget
            send_button.addEventListener ("click", (fun _ -> sendUserInput() |> ignore ))
            content.node.appendChild(send_button) |> ignore


            // Add application command
            let command = "jupyterlab_starchat_extension:open"
            //TODO: using dynamic (?) b/c the imports aren't fully implemented
            app.commands?addCommand( command, 
                !![
                    "label" => "StarChat"
                    "execute" => fun () -> 
                        if not <| widget.isAttached then
                            app.shell?add(widget, "main")
                        app.shell?activateById(widget.id)
                ])
            //Add command to palette
            palette?addItem(
                !![
                    "command" => command
                    "category" => "Coding Assistants"
                ]
            )

            let searchParams = Browser.Url.URLSearchParams.Create(  Browser.Dom.window.location.search )
            // Browser.
                            
            //If query string has id=xxx, store this identifier as a participant id
            match searchParams.get("id") with
            | Some(id) -> Logging.idOption <- Some(id)
            | _ -> ()

            //If query string has log=xxx, use this at the logging endpoint
            //must include http/https in url
            match searchParams.get("log") with
            | Some(logUrl) -> Logging.logUrl <- Some(logUrl)
            | _ -> ()

            //get the api endpoint from the query string
            match searchParams.get("endpoint") with
            | Some(endpoint: string) -> StarChatAPI.endpointOption <- Some(endpoint)
            | _ -> ()
        )
 
    ]

exportDefault extension