using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public class InfoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task SayAsync()
        {
            await this.ReplyAsync("Created by Tanker(#6157) aka Nvrnight. Built using .NET Core 2x with Chess.NET/Discord.NET.");
        }
    }
}