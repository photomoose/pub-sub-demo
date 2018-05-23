using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Web
{
	public class PubSubHub : Hub
	{
		public async Task SendMessage(string user, string message)
		{
			await Clients.All.SendAsync("ReceiveMessage", user, message);
		}
	}
}