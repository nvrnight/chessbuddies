using System.IO;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Chess.Exceptions;
using ChessBuddies.Services;
using Discord.Commands;

namespace ChessBuddies.Chess.Commands
{
    public class MoveCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;
        public MoveCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("move")]
        public async Task SayAsync(string message)
        {
            try
            {
                using(var stream = new MemoryStream())
                {
                    var result = await _chessService.Move(stream, Context.Channel.Id, Context.Message.Author, message);

                    await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author, stream);

                    stream.Position = 0;
                    await this.Context.Channel.SendFileAsync(stream, "board.png");

                    if(result.IsOver) {
                        var overMessage = "The match is over.";
                        if(result.Winner != null)
                            overMessage += $" {result.Winner.Mention} has won the match.";

                        await this.ReplyAsync(overMessage);
                    } else {
                        var nextPlayer = await _chessService.WhoseTurn(Context.Channel.Id, Context.Message.Author);

                        var yourMoveMessage = $"Your move {nextPlayer.Mention}.";

                        if(result.IsCheck)
                            yourMoveMessage += " Check!";

                        await Context.Channel.SendMessageAsync(yourMoveMessage);
                    }
                }
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
    }
}