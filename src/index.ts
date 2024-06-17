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


const MessageMap: { [id: string] : [string,string]; } = 
  {
    "UserMessage": ["You","right"], 
    "BotMessage": ["StarChat","left"],
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
 * Enable user input button
 */
export function enableUserInput(send_button:HTMLButtonElement): void {
    send_button.disabled = false
    send_button.className = "msger-send-btn" 
    send_button.innerText = "Send"
}
/**
 * Send user input to StarChat
 */
export function sendUserInput(): void {
  // get user message from textarea
  let user_message : string = (<HTMLTextAreaElement>document.getElementById("user_input_starchat"))?.value ?? ""

  //add user message to history
  appendToHistory(user_message,"UserMessage");

  //disable send button while we wait
  let send_button = <HTMLButtonElement>document.getElementById("send_button_starchat");
  
  send_button.disabled = true
  send_button.className = "msger-wait-btn" 
  send_button.innerText = "Wait"
  
  // send to API, get bot response
  StarChatAPI.SendMessage(user_message)
    .then( (data) => 
      {
        let bot_response = data.bot_response;
        //log
        Logging.LogToServer( Logging.createUserMessageBotResponse(user_message,bot_response ) );

        //format markdown
        let html = markdown_it.render(bot_response.replace("<|end|>",""))

        //update UI
        appendToHistory(html,"BotMessage");

        //enable input
        enableUserInput(send_button);
      }).catch( (e) =>
        {
          //log
          Logging.LogToServer( Logging.createUserMessageBotResponse(user_message, "SERVER_ERROR: " + e ) );

          //update UI with the error
          appendToHistory("An error occurred when contacting StarChat. See your browser console for more information.", "ErrorMessage");
          console.log(e);

          //enable input
          enableUserInput(send_button);
        });

}

/// Simplest way to connect javascript in injected HTML to our function: make a global function here
const _global = (window /* browser */ || global /* node */) as any;
_global.sendUserInput_StarChat = sendUserInput;


/**
 * Function to create our widget
 */
export function createWidget(): MainAreaWidget {
  const content = new Widget();
  content.addClass('starchat');
  const widget: MainAreaWidget = new MainAreaWidget({ content });
  widget.id = "jupyterlab_starchat_extension";
  widget.title.label = "StarChat Coding Assistant";
  widget.title.closable = true;

  // ui  
  content.node.appendChild(container_div_starchat)
            
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
