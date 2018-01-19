using System.Threading.Tasks;
using ChessBuddies.Services;
using Discord.Commands;
using Newtonsoft.Json;

namespace ChessBuddies.Commands
{
    public class StatusCommand : AdminCommand
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChessService _chessService;
        public StatusCommand(IAuthorizationService authorizationService, IChessService chessService) : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _chessService = chessService;
        }

        [Command("status")]
        public async Task SayAsync(string message = "")
        {
            await Authorize();

            await ReplyAsync(JsonConvert.SerializeObject(await _chessService.GetStatus()));
        }
    }
}