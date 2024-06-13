export interface LogEntry {
  username: string;
  json: string;
}

export interface StarChatLogEntry060623 {
  schema: string;
  user_message: string;
  bot_response: string;
}


export function createUserMessageBotResponse(user_message:string, bot_response:string): StarChatLogEntry060623 {
  return {
    schema: "scle060623",
    user_message: user_message,
    bot_response: bot_response
  };
}


let logUrl: string | undefined = undefined;
export function set_log_url( url: string) {
  logUrl = url;
}
let idOption: string | undefined = undefined;
export function set_id( id: string) {
  idOption = id;
}

export function LogToServer(logObject: any): void {
  if(logUrl) {
    let id =  window.location.href;
    if( idOption ) { id = idOption; }
    window.fetch(logUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        username:id,
        //base64 encode the payload because it can have all kinds of craziness inside it
        json:btoa(logObject)
        
      })
    }).then(response => {
      if (!response.ok) {
        console.log(response.statusText);
      }
    })
  }
}
