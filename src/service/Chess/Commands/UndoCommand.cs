using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;

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
        public async Task SayAsync()
        {
            try
            {
                var undoRequest = await _chessService.UndoRequest(Context.Channel.Id, Context.Message.Author.Id, async x => {
                    await ReplyAsync($"Undo request timed out.");
                });

                await ReplyAsync($"{Context.Message.Author.Mention} is wanting to undo the previous move. Do !accept to accept.");
            }
            catch(ChessException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}