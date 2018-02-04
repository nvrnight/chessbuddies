using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Commands;
using ChessBuddies.Services;
using Discord.Commands;
using Newtonsoft.Json;

namespace service.Chess.Commands
{
    public class GamesCommand : AdminCommand
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChessService _chessService;
        public GamesCommand(IAuthorizationService authorizationService, IChessService chessService) : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _chessService = chessService;
        }

        [Command("games")]
        public async Task SayAsync(string message = "")
        {
            await Authorize();

            await ReplyAsync(JsonConvert.SerializeObject(await _chessService.GetStatus()));
        }
    }
}