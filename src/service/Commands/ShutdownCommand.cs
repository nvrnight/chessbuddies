using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ChessBuddies;
using ChessBuddies.Services;
using Discord;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public class ShutdownCommand : ModuleBase<SocketCommandContext>
    {
        public ShutdownCommand()
        { }
        [IsAdmin]
        [Command("shutdown")]
        [Summary("This will save on-going games and shutdown the bot.")]
        public Task Shutdown()
        {
            Program.ShutdownEvent.Set();
            return Task.CompletedTask;
        }
    }
}