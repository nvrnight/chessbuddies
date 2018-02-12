using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public class InfoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        [Summary("Dispays some info about the bot")]
        public async Task SayAsync()
        {
            await this.ReplyAsync("Created by Tanker(#6157) aka Nvrnight. Built using .NET Core 2x with Chess.NET/Discord.NET.");
        }
    }
}