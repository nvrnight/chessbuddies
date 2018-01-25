using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;

namespace ChessBuddies.Chess.Commands
{
    public class ChallengeCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public ChallengeCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("challenge")]
        public async Task SayAsync(string message = "")
        {
            await Task.Run(async () => {
                message = message.Trim();

                var matches = Regex.Matches(message, @"<@(\d+)>");

                if(!matches.Any())
                {
                    await this.ReplyAsync("Invalid challenge, example: challenge @PersonName");
                    return;
                }

                try
                {
                    var user = await Context.Channel.GetUserAsync(ulong.Parse(matches[0].Groups[1].Value));

                    await _chessService.Challenge(Context.Channel.Id, this.Context.Message.Author.Id, user.Id, async x => {
                        await this.ReplyAsync($"Challenge timed out for {x.Challenger.Mention()} challenging {x.Challenged.Mention()}");
                    });

                    await this.ReplyAsync(this.Context.Message.Author.Mention + $" is challenging {user.Mention}.");
                }
                catch(ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
    }
}