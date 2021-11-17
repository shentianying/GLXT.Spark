using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Hubs
{
    public class MsgHub : Hub
    {
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", new { data = "你好" });
        }
        //public override async Task OnConnectedAsync()
        //{
        //    var connectionId = Context.ConnectionId;
        //    await Groups.AddToGroupAsync(connectionId, "mygroup");
        //    await Groups.RemoveFromGroupAsync(connectionId, "mygroup");
        //    await Clients.Group("mygroup").SendAsync("someFunc", new { radom = "0.0" });
        //    await Clients.Client(connectionId).SendAsync("getconnectId", new { connectId = connectionId });
        //}
    }
}
