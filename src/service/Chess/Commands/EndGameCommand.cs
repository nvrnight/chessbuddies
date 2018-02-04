using System;
using System.Linq;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Commands;
using ChessBuddies.Services;
using Discord.Commands;

namespace service.Chess.Commands
{
    public class EndGameCommand : AdminCommand
    {
        private readonly IChessService _chessService;
        protected EndGameCommand(IAuthorizationService authorizationService, IChessService chessService) : base(authorizationService)
        {
            _chessService = chessService;
        }

        [Command("endgame")]
        public async Task SayAsync(string id = "")
        {
            await Authorize();

            await Task.Run(async () =>
            {
                id = id.Trim();

                if (string.IsNullOrEmpty(id))
                {
                    await ReplyAsync("Id not provided.");
                    return;
                }

                Guid idGuid;
                if(!Guid.TryParse(id, out idGuid))
                {
                    await ReplyAsync("Id is not formatted correctly.");
                    return;
                }

                var match = _chessService.Matches.SingleOrDefault(x => x.Id == idGuid);

                if(match == null)
                {
                    await ReplyAsync("Match not found.");
                    return;
                }

                _chessService.Matches.Remove(match);

                await ReplyAsync("Match removed.");
            });
        }
    }
}