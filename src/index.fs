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


// OLD UI
//Some UI elements are declared outside the extension object so we can refer to them in functions
//This is a lightweight alternative to using class syntax on the extension

// let history_div = Browser.Dom.document.createElement("div")
// history_div.setAttribute("style","width: 100%; max-width: 100%;height:440px;overflow: auto;") 
// history_div.id <- "history_div"
// let user_input = Browser.Dom.document.createElement("textarea")
// user_input.setAttribute("style","width: 100%; max-width: 100%")
// user_input.id <- "user_input"
// let send_button : HTMLButtonElement = document.createElement ("button") :?> HTMLButtonElement
// send_button.setAttribute("style","width: 100%; max-width: 100%")
// send_button.id <- "send_button"
// send_button.innerText <- "Send"

//NEW UI: adapted with love from https://codepen.io/sajadhsm/pen/odaBdd
let container_div_starchat = Browser.Dom.document.createElement("div") 
container_div_starchat.className <- "container-div-starchat"
container_div_starchat.innerHTML <- """<section class="msger">
  <main id = "history_div" class="msger-chat">
    <div class="msg left-msg">
      <div class="msg-bubble">
        <div class="msg-info">
          <div class="msg-info-name">StarChat</div>
        </div>

        <div class="msg-text">
          Welcome! Go ahead and send me a message. 😄
        </div>
      </div>
    </div>
  </main>

  <div class="msger-inputarea">
    <textarea id = "user_input_starchat" rows="4" class="msger-input" placeholder="Enter your message..."></textarea>
    <button id = "send_button_starchat" type="submit" class="msger-send-btn" onclick="sendUserInput_StarChat()">Send</button>
  </div>
</section>"""

// let css = Browser.Css.CSSStyleSheet.Create()
// css.cssText <- 
let css = """<style>:root {
  --body-bg: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  --msger-bg: #fff;
  --border: 2px solid #ddd;
  --left-msg-bg: #ececec;
  --right-msg-bg: #579ffb;
}

html {
  box-sizing: border-box;
}

*,
*:before,
*:after {
  margin: 0;
  padding: 0;
  box-sizing: inherit;
}

body {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-image: var(--body-bg);
  font-family: Helvetica, sans-serif;
}

.container-div-starchat{
    height: 100%
}
.msger {
  display: flex;
  flex-flow: column wrap;
  justify-content: space-between;
  width: 100%;
  height: 100%;
  border: var(--border);
  border-radius: 5px;
  background: var(--msger-bg);
  box-shadow: 0 15px 15px -5px rgba(0, 0, 0, 0.2);
}

.msger-header {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  border-bottom: var(--border);
  background: #eee;
  color: #666;
}

.msger-chat {
  flex: 1;
  overflow-y: scroll;
  padding: 10px;
}
.msger-chat::-webkit-scrollbar {
  width: 10px;
}
.msger-chat::-webkit-scrollbar-track {
  background: #ddd;
}
.msger-chat::-webkit-scrollbar-thumb {
  background: #bdbdbd;
}
.msg {
  display: flex;
  align-items: flex-end;
  margin-bottom: 10px;
}
.msg:last-of-type {
  margin: 0;
}
.msg-img {
  width: 50px;
  height: 50px;
  margin-right: 10px;
  background: #ddd;
  background-repeat: no-repeat;
  background-position: center;
  background-size: cover;
  border-radius: 50%;
}
.msg-bubble {
  max-width: 450px;
  padding: 15px;
  border-radius: 15px;
  background: var(--left-msg-bg);
}
.msg-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}
.msg-info-name {
  margin-right: 10px;
  font-weight: bold;
}
.msg-info-time {
  font-size: 0.85em;
}

.left-msg .msg-bubble {
  border-bottom-left-radius: 0;
}

.right-msg {
  flex-direction: row-reverse;
}
.right-msg .msg-bubble {
  background: var(--right-msg-bg);
  color: #fff;
  border-bottom-right-radius: 0;
}
.right-msg .msg-img {
  margin: 0 0 0 10px;
}

.msger-inputarea {
  display: flex;
  padding: 10px;
  border-top: var(--border);
  background: #eee;
}
.msger-inputarea * {
  padding: 10px;
  border: none;
  border-radius: 3px;
  font-size: 1em;
}
.msger-input {
  flex: 1;
  background: #ddd;
}
.msger-send-btn {
  margin-left: 10px;
  background: rgb(0, 196, 65);
  color: #fff;
  font-weight: bold;
  cursor: pointer;
  transition: background 0.23s;
}
.msger-wait-btn {
  margin-left: 10px;
  background: rgb(196, 49, 0);
  color: #fff;
  font-weight: bold;
  cursor: pointer;
  transition: background 0.23s;
}
.msger-send-btn:hover {
  background: rgb(0, 180, 50);
}

.msger-chat {
  background-color: #fcfcfe;  
}</style>"""

// let appendToHistory( response ) ( message_type : Message)=
//     let newDiv = Browser.Dom.document.createElement("div")
//     newDiv.innerHTML <- response
//     match message_type with
//     | BotMessage -> newDiv.setAttribute("style","background-color:LemonChiffon;") //"width: 100%; max-width: 100%;height:440px;overflow: auto;")
//     | UserMessage -> newDiv.setAttribute("style","background-color:LightGrey;")
//     | ErrorMessage -> newDiv.setAttribute("style","background-color:Tomato;")

//     history_div.appendChild(newDiv) |> ignore

//     //scroll into view
//     history_div.scrollTop <- history_div.scrollHeight;

// let sendUserInput() =
//     // get  user message from textarea
//     let user_message = user_input?value

//     //add user message to history
//     appendToHistory user_message UserMessage

//     //disable send button while we wait
//     send_button.disabled <- true
//     send_button.innerText <- "Wait"
    
//     // send to API, get bot response
//     promise {
//         let! coding_assistance_response = StarChatAPI.SendMessage user_message
//         match coding_assistance_response with
//         | Ok( ok ) ->
//             //log
//             Logging.LogToServer( Logging.StarChatLogEntry060623.Create user_message ok.bot_response )

//             //format markdown
//             let html = markdownIt.render(ok.bot_response.Replace("<|end|>",""))

//             //update UI
//             appendToHistory html BotMessage
            
//         | Error( e  ) ->
//             //update UI with the error
//             appendToHistory ("An error occurred when contacting StarChat. See your browser console for more information.") ErrorMessage
//             console.log(e)

//         //enable button
//         send_button.disabled <- false
//         send_button.innerText <- "Send"
//     }
//     // ()

let appendToHistoryNew( text ) ( message_type : Message)=
    let name,side = 
        match message_type with
        | BotMessage -> "Starchat","left"
        | UserMessage -> "You","right"
        | ErrorMessage -> "ERROR","left"
    let msgHTML = 
        """<div class="msg #SIDE-msg">
            <div class="msg-bubble">
            <div class="msg-info">
                <div class="msg-info-name">#NAME</div>
            </div>
            <div class="msg-text">#TEXT</div>
            </div>
        </div>""".Replace("#NAME",name).Replace("#SIDE",side).Replace("#TEXT",text)

    let msgerChat = Browser.Dom.document.getElementsByClassName("msger-chat").[0] :?> HTMLElement
    msgerChat.insertAdjacentHTML("beforeend", msgHTML);
    msgerChat.scrollTop <- msgerChat.scrollTop + 500.0;

    //scroll into view
    // history_div.scrollTop <- history_div.scrollHeight;

let sendUserInputNew() =
    // get  user message from textarea
    let user_message = Browser.Dom.document.getElementById("user_input_starchat")?value

    //add user message to history
    appendToHistoryNew user_message UserMessage

    //disable send button while we wait
    let send_button = Browser.Dom.document.getElementById("send_button_starchat") :?> HTMLButtonElement
    
    send_button.disabled <- true
    send_button.className <- "msger-wait-btn" 
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
            appendToHistoryNew html BotMessage
            
        | Error( e  ) ->
            //update UI with the error
            appendToHistoryNew ("An error occurred when contacting StarChat. See your browser console for more information.") ErrorMessage
            console.log(e)

        //enable button
        send_button.disabled <- false
        send_button.className <- "msger-send-btn" 
        send_button.innerText <- "Send"
    }
    // ()

/// Simplest way to connect javascript injected into code cell output to F#: make a global function in node
let [<Global>] ``global`` : obj = jsNative
``global``?sendUserInput_StarChat <- sendUserInputNew

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

            // // Add chat history to widget
            // content.node.appendChild(history_div) |> ignore

            // // Add user input to widget
            // content.node.appendChild(user_input) |> ignore

            // // Add send button to widget
            // send_button.addEventListener ("click", (fun _ -> sendUserInput() |> ignore ))
            // content.node.appendChild(send_button) |> ignore

            //new ui            
            content.node.appendChild(container_div_starchat) |> ignore
            
            //put the style in the <head> of the entire page
            let head = Browser.Dom.document.head
            head.insertAdjacentHTML("beforeend", css)


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