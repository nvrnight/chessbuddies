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
        public async Task SayAsync(int minutes = 0)
        {
            await Authorize();

            if(minutes == 0)
                await ReplyAsync("Must include minutes until shutdown. Value must be greater than 0.");
            else
            {
                Program.ShutdownTime = DateTime.UtcNow.AddMinutes(minutes);
                
                ScheduleStatusUpdates(minutes);
            }
        }
        private async void ScheduleStatusUpdates(int minutes)
        {
            var timer = new Timer(60000);
            await UpdateStatus();
            timer.Elapsed += async (obj, e) => {
                await UpdateStatus();
            };
            timer.Enabled = true; 
            await Task.Delay((int)Math.Abs((DateTime.UtcNow - Program.ShutdownTime).Value.TotalMilliseconds) - 5000); 
            timer.Stop();
            timer.Dispose();
        }

        private async Task UpdateStatus()
        {
            var minutesLeft = (int)Math.Abs(Math.Floor((DateTime.UtcNow - Program.ShutdownTime).Value.TotalMinutes));
            await Context.Client.SetGameAsync($"Shutdown < {minutesLeft} min(s)");
        }
    }
}