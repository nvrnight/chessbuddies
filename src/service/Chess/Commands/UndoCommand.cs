using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;
using System.IO;

namespace ChessBuddies.Chess.Commands
{
    public class UndoCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public UndoCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("undo")]
        public async Task SayAsync(string amountString = "")
        {
            await Task.Run(async () => {
                int amount;
                if(!int.TryParse(amountString, out amount))
                    amount = 1;

                try
                {
                    using(var stream = new MemoryStream())
                    {
                        var undoRequest = await _chessService.UndoRequest(Context.Channel.Id, Context.Message.Author.Id, amount, stream, async x => {
                            using(var timeoutStream = new MemoryStream())
                            {
                                await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author.Id, timeoutStream);

                                timeoutStream.Position = 0;
                                await Context.Channel.SendFileAsync(timeoutStream, "board.png", "The undo request timed out. Here is the current board.");
                            }
                        });

                        stream.Position = 0;
                        await Context.Channel.SendFileAsync(stream, "board.png", $"{Context.Message.Author.Mention} is wanting to undo {undoRequest.Amount} move(s). Do !accept to accept. Here is a preview of what the board will look like.");
                    }
                    
                }
                catch(ChessException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            });
        }
    }
}