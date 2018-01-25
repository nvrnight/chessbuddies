using System.IO;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;

namespace ChessBuddies.Chess.Commands
{
    public class ShowCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public ShowCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("show")]
        public async Task SayAsync(string message = "")
        {
            await Task.Run(async () => {
                using(var stream = new MemoryStream())
                {
                    try
                    {
                        await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author.Id, stream);

                        stream.Position = 0;
                        await this.Context.Channel.SendFileAsync(stream, "board.png");
                    }
                    catch(ChessException ex)
                    {
                        await ReplyAsync(ex.Message);
                    }
                }
            });
        }
    }
}