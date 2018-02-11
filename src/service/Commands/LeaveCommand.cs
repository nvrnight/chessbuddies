
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace ChessBuddies.Commands
{
    public class LeaveCommand : ModuleBase<SocketCommandContext>
    {
        public LeaveCommand()
        {
        }
        [IsAdmin]
        [Command("leave")]
        [Summary("Leaves the current guild")]
        public async Task SayAsync(ulong channelId)
        {
            await Task.Run(async () => {

                var channel = Context.Client.GetChannel(channelId) as SocketGuildChannel;

                if(channel == null)
                    await ReplyAsync("Channel not found.");
                else
                    await channel.Guild.LeaveAsync();
            });
        }
    }
}