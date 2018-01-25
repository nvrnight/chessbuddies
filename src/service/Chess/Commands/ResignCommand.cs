using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;

namespace ChessBuddies.Chess.Commands
{
    public class ResignCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public ResignCommand(IChessService chessService)
        {
            _chessService = chessService;
        }
        [Command("resign")]
        public async Task SayAsync()
        {
            await Task.Run(async () => {
                try
                {
                    var match = await _chessService.Resign(Context.Channel.Id, this.Context.Message.Author.Id);

                    var winner = match.Challenger == this.Context.Message.Author.Id ? match.Challenged : match.Challenger;

                    await this.ReplyAsync($"{this.Context.Message.Author.Mention} has resigned the match. {winner.Mention()} has won the game.");
                }
                catch(ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
    }
}