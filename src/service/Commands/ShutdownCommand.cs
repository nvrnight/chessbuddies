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
        public Task Shutdown()
        {
            Program.ShutdownEvent.Set();
            return Task.CompletedTask;
        }
    }
}