using System;
using System.Linq;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Commands;
using ChessBuddies.Database;
using ChessBuddies.Services;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace service.Chess.Commands
{
    public class EndGameCommand : AdminCommand
    {
        private readonly IChessService _chessService;
        private readonly IServiceProvider _services;
        protected EndGameCommand(IAuthorizationService authorizationService, IChessService chessService, IServiceProvider services) : base(authorizationService)
        {
            _services = services;
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
                if (!Guid.TryParse(id, out idGuid))
                {
                    await ReplyAsync("Id is not formatted correctly.");
                    return;
                }

                var match = _chessService.Matches.SingleOrDefault(x => x.Id == idGuid);

                if (match == null)
                {
                    await ReplyAsync("Match not found.");
                    return;
                }

                #pragma warning disable 4014
                Task.Run(async () =>
                {
                    using (Db db = _services.GetService<Db>())
                    {
                        await db.EndMatch(match, null);
                        db.SaveChanges();
                    }
                });
                #pragma warning restore 4014
                _chessService.Matches.Remove(match);

                await ReplyAsync("Match removed.");
            });
        }
    }
}