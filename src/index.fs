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

[<ImportMember("@jupyterlab/apputils")>]
let ICommandPalette : obj = jsNative

//Some UI elements are declared outside the extension object so we can refer to them in functions
//This is a lightweight alternative to using class syntax on the extension

let history_div = Browser.Dom.document.createElement("div")
history_div.setAttribute("style","width: 100%; max-width: 100%;height:440px;overflow: auto;")
history_div.id <- "history_div"
let user_input = Browser.Dom.document.createElement("textarea")
user_input.setAttribute("style","width: 100%; max-width: 100%")
user_input.id <- "user_input"

let appendResponseToHistory( response ) =
    let newDiv = Browser.Dom.document.createElement("div")
    newDiv.innerHTML <- response
    newDiv.setAttribute("style","color:LemonChiffon;") //"width: 100%; max-width: 100%;height:440px;overflow: auto;")
    // newDiv.id <- "history_div"
    history_div.appendChild(newDiv) |> ignore

let appendErrorToHistory( ) =
    let newDiv = Browser.Dom.document.createElement("div")
    newDiv.innerHTML <- "An error occurred when contacting StarChat. See your browser console for more information."
    newDiv.setAttribute("style","color:Tomato;") //"width: 100%; max-width: 100%;height:440px;overflow: auto;")
    // newDiv.id <- "history_div"
    history_div.appendChild(newDiv) |> ignore

let sendUserInput() =
    // get  user message from textarea
    let user_message = user_input.innerText

    // send to API, get bot response
    promise {
        let! coding_assistance_response = StarChatAPI.SendMessage user_message
        match coding_assistance_response with
        | Ok( ok ) ->
            //log
            Logging.LogToServer( Logging.StarChatLogEntry060623.Create user_message ok.bot_response )

            //update UI
            appendResponseToHistory(ok.bot_response)

        | Error( e ) ->
            //update UI with the error
            appendErrorToHistory()
            console.log(e)
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
            let send_button = document.createElement ("button")
            send_button.setAttribute("style","width: 100%; max-width: 100%")
            send_button.id <- "send_button"
            send_button.innerText <- "Send"
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