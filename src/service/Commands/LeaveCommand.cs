
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace ChessBuddies.Commands
{
    public class LeaveCommand : AdminCommand
    {
        private readonly IAuthorizationService _authorizationService;
        public LeaveCommand(IAuthorizationService authorizationService) : base(authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Command("leave")]
        public async Task SayAsync(ulong channelId)
        {
            await Task.Run(async () => {
                await Authorize();

                var channel = Context.Client.GetChannel(channelId) as SocketGuildChannel;

                if(channel == null)
                    await ReplyAsync("Channel not found.");
                else
                    await channel.Guild.LeaveAsync();
            });
        }
    }
}