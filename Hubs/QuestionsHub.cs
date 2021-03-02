using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace QandA.Hubs
{
    public class QuestionsHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
           // when a client connects to this hub, this OnConnectedAsync method will be called
            await Clients.Caller.SendAsync("Message",
            "Successfully connected");//push a message to the client to inform it that a connection has been successfully made:
           // override a method in the base class that gets invoked when a client connects:
        }
        public override async Task OnDisconnectedAsync(Exception exception)//that is invoked when a client disconnects
        {
            await Clients.Caller.SendAsync("Message",
            "Successfully disconnected");//will be called in our React client when it disconnects from the SignalR API.
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SubscribeQuestion(int questionId)//method that the client can call to subscribe to updates for a particular question:
            //the question ID of the question to subscribe to.
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Question-{questionId}");//groups feature that we use to store all the subscribers to them question in.
            await Clients.Caller.SendAsync("Message","Successfully subscribed");
        }
        public async Task UnsubscribeQuestion(int questionId)//method to unsubscribe from getting updates about a question:
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,
            $"Question-{questionId}");
            await Clients.Caller.SendAsync("Message",
            "Successfully unsubscribed");
        }
    }
}
