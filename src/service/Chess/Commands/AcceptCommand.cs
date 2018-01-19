using System.IO;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;

namespace ChessBuddies.Chess.Commands
{
    public class AcceptCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public AcceptCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("accept")]
        public async Task SayAsync()
        {
            try
            {
                var writeBoard = false;
                if(await _chessService.HasChallenge(Context.Channel.Id, Context.Message.Author))
                {
                    var match = await _chessService.AcceptChallenge(Context.Channel.Id, this.Context.Message.Author);

                    await this.ReplyAsync($"Match has started between {match.Challenger.Mention} and {match.Challenged.Mention}.");

                    writeBoard = true;
                }
                else if(await _chessService.HasUndoRequest(Context.Channel.Id, Context.Message.Author))
                {
                    await _chessService.Undo(Context.Channel.Id, Context.Message.Author);

                    writeBoard = true;
                }
                else
                    throw new ChessException("Nothing to accept.");

                if(writeBoard)
                {
                    using(var stream = new MemoryStream())
                    {
                        await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author, stream);

                        stream.Position = 0;
                        await this.Context.Channel.SendFileAsync(stream, "board.png");
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