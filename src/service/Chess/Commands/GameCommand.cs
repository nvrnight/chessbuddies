using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Chess.Exceptions;
using ChessBuddies.Commands;
using ChessBuddies.Services;
using Discord.Commands;

namespace service.Chess.Commands
{
    public class GameCommand : AdminCommand
    {
        private readonly IChessService _chessService;
        protected GameCommand(IAuthorizationService authorizationService, IChessService chessService) : base(authorizationService)
        {
            _chessService = chessService;
        }

        [Command("game")]
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

                using(var stream = new MemoryStream())
                {
                    await _chessService.WriteBoard(match.Channel, match.Challenger, stream);
                    stream.Position = 0;
                    await Context.Channel.SendFileAsync(stream, "board.png");
                }
            });
        }
    }
}