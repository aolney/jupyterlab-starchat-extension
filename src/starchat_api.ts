let endpointOption: string | undefined = undefined;
export function set_endpoint_url( url: string) {
    endpointOption = url;
}

export interface  CodingAssistanceRequest 
{
    user_message : string
}

export interface  CodingAssistanceResponse
{
    bot_response : string
}


export function SendMessage(input: string) : Promise<CodingAssistanceResponse> {
    //autoset endpointOption for saturn
    if( !endpointOption && window.location.hostname == "saturn.olney.ai"){
        endpointOption = "https://starchat.olney.ai/api/getBotResponse";
    } else if (!endpointOption ) {
        endpointOption = "UNDEFINED"
    }

    let p = window.fetch(endpointOption, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
        user_message: input
        })
    }).then( response => {
        if( response.ok ) {
            return response.json();
        } else {
            throw new Error("Failed to use StarChat service. Specific error is " + response.status);
        }
    }).catch( error => console.log("Error in call to StarChat service. Specific error is " + error));

    //p is now the json payload
    return p;
    // //autoset endpointOption for saturn
    // if( !endpointOption && window.location.hostname == "saturn.olney.ai"){
    //     endpointOption = "saturn.olney.ai";
    // } else if (!endpointOption ) {
    //     endpointOption = "UNDEFINED"
    // }
    // let p = window.fetch(endpointOption, {
    //     method: "POST",
    //     headers: {
    //         "Content-Type": "application/json",
    //     },
    //     body: JSON.stringify({
    //     user_message: input
    //     })
    // });

    // return p;
}
