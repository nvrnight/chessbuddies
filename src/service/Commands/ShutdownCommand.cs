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
    public class ShutdownCommand : AdminCommand
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChessService _chessService;
        public ShutdownCommand(IAuthorizationService authorizationService, IChessService chessService) : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _chessService = chessService;
        }

        [Command("shutdown")]
        public async Task SayAsync(string message = "")
        {
            await Task.Run(async () => {
                await Authorize();

                Program.ShutdownEvent.Set();
            });
        }
    }
}