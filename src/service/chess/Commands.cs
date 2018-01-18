using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace src
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
            using(var stream = new MemoryStream())
            {
                try
                {
                    await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author, stream);

                    stream.Position = 0;
                    await this.Context.Channel.SendFileAsync(stream, "board.png");
                }
                catch(ChessException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }
        }
    }
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
                var undoRequest = await _chessService.UndoRequest(Context.Channel.Id, Context.Message.Author, async x => {
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

                await _chessService.Challenge(Context.Channel.Id, this.Context.Message.Author, user, async x => {
                    await this.ReplyAsync($"Challenge timed out for {x.Challenger.Mention} challenging {x.Challenged.Mention}");
                });

                await this.ReplyAsync(this.Context.Message.Author.Mention + $" is challenging {user.Mention}.");
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
    }
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
            try
            {
                var match = await _chessService.Resign(Context.Channel.Id, this.Context.Message.Author);

                var winner = match.Challenger == this.Context.Message.Author ? match.Challenged : match.Challenger;

                await this.ReplyAsync($"{this.Context.Message.Author.Mention} has resigned the match. {winner.Mention} has won the game.");
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
    }
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