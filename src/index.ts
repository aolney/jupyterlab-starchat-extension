import {
  JupyterFrontEnd,
  JupyterFrontEndPlugin
} from '@jupyterlab/application';
import { INotebookTracker } from "@jupyterlab/notebook";
import { ICommandPalette, MainAreaWidget } from '@jupyterlab/apputils';
import { Widget } from '@lumino/widgets';
import * as Logging from './logging';
import * as StarChatAPI from './starchat_api';
import markdownit from 'markdown-it';

const markdown_it = markdownit()

/**
 * HTML for interface
 */
const container_div_starchat = document.createElement("div");
container_div_starchat.className = "container-div-starchat";
container_div_starchat.innerHTML = `<section class="msger">
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
</section>`;

//TODO: move this elsewhere
const css = `<style>:root {
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
}</style>`;

const MessageMap: { [id: string] : [string,string]; } = 
  {
    "UserMessage": ["StarChat","left"],
    "BotMessage": ["You","right"],
    "ErrorMessage": ["ERROR","left"]
  }

/**
 * Append new turn to history
 */
export function appendToHistory( text:string, message_type: string): void {
  let [name,side] = MessageMap[message_type];
  let msgHTML = 
        `<div class="msg #SIDE-msg">
            <div class="msg-bubble">
            <div class="msg-info">
                <div class="msg-info-name">#NAME</div>
            </div>
            <div class="msg-text">#TEXT</div>
            </div>
        </div>`.replace("#NAME",name).replace("#SIDE",side).replace("#TEXT",text)
  let msgerChat = document.getElementsByClassName("msger-chat")[0] 
  msgerChat.insertAdjacentHTML("beforeend", msgHTML);
  msgerChat.scrollTop = msgerChat.scrollTop + 500.0;

}

/**
 * Send user input to StarChat
 */
export function sendUserInput(): void {
  // get user message from textarea
  let user_message : string = document.getElementById("user_input_starchat")?.nodeValue ?? ""
  // if( !user_message ) { user_message = ""; }

  //add user message to history
  appendToHistory(user_message,"UserMessage");

  //disable send button while we wait
  let send_button = <HTMLButtonElement>document.getElementById("send_button_starchat");
  
  send_button.disabled = true
  send_button.className = "msger-wait-btn" 
  send_button.innerText = "Wait"
  
  // send to API, get bot response
  // TODO rework according to https://stackoverflow.com/questions/62859190/handle-promise-catches-in-typescript
  StarChatAPI.SendMessage(user_message)
    // .then( (response) => response.json())
    .then( (data) => 
      {
        let bot_response = data.bot_response;
        //log
        Logging.LogToServer( Logging.createUserMessageBotResponse(user_message,bot_response ) );

        //format markdown
        let html = markdown_it.render(bot_response.replace("<|end|>",""))

        //update UI
        appendToHistory(html,"BotMessage");
      }).catch( (e) =>
        {
          //log
          Logging.LogToServer( Logging.createUserMessageBotResponse(user_message, "SERVER_ERROR: " + e ) );

          //update UI with the error
          appendToHistory("An error occurred when contacting StarChat. See your browser console for more information.", "ErrorMessage");
          console.log(e);
        });

      //enable button
      send_button.disabled = false
      send_button.className = "msger-send-btn" 
      send_button.innerText = "Send"
}

/// Simplest way to connect javascript in injected HTML to our function: make a global function here
const _global = (window /* browser */ || global /* node */) as any;
_global.sendUserInput_StarChat = sendUserInput;


/**
 * Function to create our widget
 */
export function createWidget(): MainAreaWidget {
  const content = new Widget();
  const widget: MainAreaWidget = new MainAreaWidget({ content });
  widget.id = "jupyterlab_starchat_extension";
  widget.title.label = "StarChat Coding Assistant";
  widget.title.closable = true;

  // ui  
  content.node.appendChild(container_div_starchat)
            
  //put the style in the <head> of the entire page
  let head = document.head
  head.insertAdjacentHTML("beforeend", css)

  return widget;
};

/**
 * Initialization data for the @aolney/jupyterlab-starchat-extension extension.
 */
const plugin: JupyterFrontEndPlugin<void> = {
  id: '@aolney/jupyterlab-starchat-extension:plugin',
  description: 'A JupyterLab extension providing an interface to StarChat.',
  autoStart: true,
  requires: [ICommandPalette, INotebookTracker],
  activate: (app: JupyterFrontEnd, palette: ICommandPalette) => {
    console.log('JupyterLab extension @aolney/jupyterlab-starchat-extension is activated!');
    let widget = createWidget();

     // Add application command
    let command = "jupyterlab_starchat_extension:open"
    app.commands.addCommand( command, 
      {
        label: "StarChat",
        execute: (): void => {
          if( !widget.isAttached ) {
            app.shell.add(widget,"main");
          }
          app.shell.activateById(widget.id);
        }
      });

    //Add command to palette
    palette.addItem(
    {
      command: command,
      category: "Coding Assistants"
    });

    // process query string parameters
    const searchParams: any = new URLSearchParams(window.location.search);

    let id = searchParams.get("id");
    if( id ) {
      Logging.set_id( id );
      console.log( "jupyterlab_starchat_extension: using id=" + id + " for logging" ) ;
    }

    let log_url = searchParams.get("log") ;
    if( log_url ) {
      Logging.set_log_url( log_url );
      console.log( "jupyterlab_starchat_extension: using log=" + log_url + " for logging" ) ;
    }

    let endpoint_url = searchParams.get("endpoint") ;
    if( endpoint_url ) {
      StarChatAPI.set_endpoint_url( endpoint_url );
      console.log( "jupyterlab_starchat_extension: using endpoint=" + endpoint_url + " for StarChat service" ) ;
    }
  }
};

export default plugin;
