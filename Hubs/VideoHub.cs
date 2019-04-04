#region using
using Hit.Auth.Filters;
using Hit.LoggerLibrary;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Reflection;
using System.Threading.Tasks;
#endregion

namespace Hit.GutService.Hubs
{
    [JwtAuthentication("")]
    public class VideoHub : Hub
    {
        public override Task OnConnected()
        {
            Logger.INFO(MethodBase.GetCurrentMethod(), "StreamHub connected");
            //TODO revizijska
            return base.OnConnected();
        }

        [HubMethodName("Stream")]
        public void Send(byte[] frame)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<VideoHub>();
            context.Clients.All.NotifyUser(frame as object);
        }
    }
}
