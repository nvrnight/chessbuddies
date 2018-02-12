using System.IO;
using System.Threading.Tasks;
using ChessBuddies;
using ChessBuddies.Services;
using Discord.Commands;
using ChessBuddies.Chess.Exceptions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ChessBuddies.Database;
using System;
using Discord;
using Newtonsoft.Json;
using System.Text;
using service.Extensions;

namespace ChessBuddies.Chess.Commands
{
    [Name("Chess")]
    [Summary("All the commands related to chess")]
    public class ChessCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;
        private readonly IServiceProvider _services;
        private readonly Db _db;

        public ChessCommands(IChessService chessService, IServiceProvider services, Db db)
        {
            _services = services;
            _chessService = chessService;
            _db = db;
        }

        [Command("accept")]
        [Summary("Accept a match challenge or undo request")]
        public async Task AcceptAsync()
        {
            await Task.Run(async () => {
                try
                {
                    var writeBoard = false;
                    if(await _chessService.HasChallenge(Context.Channel.Id, Context.Message.Author.Id))
                    {
                        var match = await _chessService.AcceptChallenge(Context.Channel.Id, this.Context.Message.Author.Id);

                        await this.ReplyAsync($"Match has started between {match.Challenger.Mention()} and {match.Challenged.Mention()}.");

                        writeBoard = true;
                    }
                    else if(await _chessService.HasUndoRequest(Context.Channel.Id, Context.Message.Author.Id))
                    {
                        await _chessService.Undo(Context.Channel.Id, Context.Message.Author.Id);

                        writeBoard = true;
                    }
                    else
                        throw new ChessException("Nothing to accept.");

                    if(writeBoard)
                    {
                        using(var stream = new MemoryStream())
                        {
                            await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author.Id, stream);

                            stream.Position = 0;
                            await this.Context.Channel.SendFileAsync(stream, "board.png");
                        }
                    }
                }
                catch(ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
        [Command("challenge")]
        [Summary("Challenge another player to a match")]
        public async Task ChallengeAsync(IUser user)
        {
            await Task.Run(async () => {
                try
                {
                    await _chessService.Challenge(Context.Channel.Id, this.Context.Message.Author.Id, user.Id, async x => {
                        await this.ReplyAsync($"Challenge timed out for {x.Challenger.Mention()} challenging {x.Challenged.Mention()}");
                    });

                    await this.ReplyAsync(this.Context.Message.Author.Mention + $" is challenging {user.Mention}.");
                }
                catch (ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
        [IsAdmin]
        [Command("endgame")]
        [Summary("End a game, id can be found from the output of **!games**")]
        public async Task EndGameAsync(string id = "")
        {

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
        [IsAdmin]
        [Command("game")]
        [Summary("View a game")]
        public async Task SayAsync(string id = "")
        {
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

                using (var stream = new MemoryStream())
                {
                    await _chessService.WriteBoard(match.Channel, match.Challenger, stream);
                    stream.Position = 0;
                    await Context.Channel.SendFileAsync(stream, "board.png");
                }
            });
        }
        [IsAdmin]
        [Command("games")]
        [Summary("Views information about ongoing games")]
        public async Task ShowGamesAsync()
        {
            await ReplyAsync(JsonConvert.SerializeObject(await _chessService.GetStatus()));
        }
        [Command("move")]
        [Summary("Move your piece, if your pawn reaches the other side of the board it will be promoted to queen by default.\nYou can promote your pawn to other pieces if you like, r = Rook, b = Bishop, q = Queen, n = Knight.\nAn example move promoting a white pawn to a Knight would be **!a7a8n**")]
        public async Task MoveAsync(string message)
        {
            await Task.Run(async () =>
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        var result = await _chessService.Move(stream, Context.Channel.Id, Context.Message.Author.Id, message);

                        await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author.Id, stream);

                        stream.Position = 0;
                        await this.Context.Channel.SendFileAsync(stream, "board.png");

                        if (result.IsOver)
                        {
                            var overMessage = result.Winner != null ? $"Checkmate! {result.Winner.Value.Mention()} has won the match." : "Stalemate!";

                            await this.ReplyAsync(overMessage);
                        }
                        else
                        {
                            var nextPlayer = await _chessService.WhoseTurn(Context.Channel.Id, Context.Message.Author.Id);

                            var yourMoveMessage = $"Your move {nextPlayer.Mention()}.";

                            if (result.IsCheck)
                                yourMoveMessage += " Check!";

                            await Context.Channel.SendMessageAsync(yourMoveMessage);
                        }
                    }
                }
                catch (ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
        [Command("resign")]
        [Summary("Resign your current match")]
        public async Task ResignAsync()
        {
            await Task.Run(async () => {
                try
                {
                    var match = await _chessService.Resign(Context.Channel.Id, this.Context.Message.Author.Id);

                    var winner = match.Challenger == this.Context.Message.Author.Id ? match.Challenged : match.Challenger;

                    await this.ReplyAsync($"{this.Context.Message.Author.Mention} has resigned the match. {winner.Mention()} has won the game.");
                }
                catch (ChessException ex)
                {
                    await this.ReplyAsync(ex.Message);
                }
            });
        }
        [Command("show")]
        [Summary("Display the current board")]
        public async Task ShowAsync()
        {
            await Task.Run(async () => {
                using (var stream = new MemoryStream())
                {
                    try
                    {
                        await _chessService.WriteBoard(Context.Channel.Id, Context.Message.Author.Id, stream);

                        stream.Position = 0;
                        await this.Context.Channel.SendFileAsync(stream, "board.png");
                    }
                    catch (ChessException ex)
                    {
                        await ReplyAsync(ex.Message);
                    }
                }
            });
        }
        private void AppendStats(StringBuilder statsBuilder, string title, int totalGames, int whiteWins, int blackWins, int staleMates, int? totalWins = null)
        {
            statsBuilder.AppendLine(title);
            statsBuilder.AppendLine("```");
            statsBuilder.AppendLine($"Total Games: {totalGames}");
            if (totalWins != null)
                statsBuilder.AppendLine($"Total Wins: {totalWins}");
            statsBuilder.AppendLine($"White Wins: {whiteWins}({whiteWins.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine($"Black Wins: {blackWins}({blackWins.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine($"Stalemates: {staleMates}({staleMates.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine("```");
            statsBuilder.AppendLine();
        }

        [Command("stats")]
        [Summary("Displays yours and the bot's stats or the referenced user's stats and their stats against you.")]
        public async Task ShowStatsAsync(IUser user = null)
        {
            await Task.Run(async () =>
            {

                var authorId = (long)Context.Message.Author.Id;

                var statsBuilder = new StringBuilder();

                var authorGameStats = _db.GameStats.Where(x =>
                    x.challenged == authorId || x.challenger == authorId
                );
                var authorWonGames = authorGameStats.Where(x => x.winner == authorId);


                if (user != null)
                {
                        
                        var userId = (long)user.Id;

                        var userGames = _db.GameStats.Where(x =>
                            x.challenged == userId || x.challenger == userId
                        );

                        var userWonGames = userGames.Where(x => x.winner == authorId);

                        var authorGamesAgainstUser = authorGameStats.Where(x => x.challenged == userId || x.challenger == userId);

                        var authorWonGamesAgainstUser = authorGamesAgainstUser.Where(x => x.winner == authorId);

                        AppendStats(statsBuilder,
                            $"**{user.Username}'s Stats**",
                            userGames.Count(),
                            userWonGames.Count(x => x.winner == x.challenger),
                            userWonGames.Count(x => x.winner == x.challenged),
                            userGames.Count(x => x.winner == null),
                            userWonGames.Count()
                        );

                        AppendStats(statsBuilder,
                            $"**{Context.Message.Author.Username}'s Stats vs {user.Username}**",
                            authorGamesAgainstUser.Count(),
                            authorWonGamesAgainstUser.Count(x => x.winner == x.challenger),
                            authorWonGamesAgainstUser.Count(x => x.winner == x.challenged),
                            authorGamesAgainstUser.Count(x => x.winner == null),
                            authorWonGamesAgainstUser.Count()
                        );
                }
                else
                {
                    AppendStats(statsBuilder,
                        "**Bot Stats**",
                        _db.GameStats.Count(),
                        _db.GameStats.Count(x => x.winner == x.challenger),
                        _db.GameStats.Count(x => x.winner == x.challenged),
                        _db.GameStats.Count(x => x.winner == null)
                    );

                    AppendStats(statsBuilder,
                        $"**{Context.Message.Author.Username}'s Stats**",
                        authorGameStats.Count(),
                        authorWonGames.Count(x => x.winner == x.challenger),
                        authorWonGames.Count(x => x.winner == x.challenged),
                        authorGameStats.Count(x => x.winner == null),
                        authorWonGames.Count()
                    );
                }

                await ReplyAsync(statsBuilder.ToString());
            });
        }
        [Command("undo")]
        [Summary("Request the last x moves to be undone.\nDefaults to 1")]
        public async Task UndoAsync(int amountOfTurns = 1)
        {
            await Task.Run(async () => {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        var undoRequest = await _chessService.UndoRequest(Context.Channel.Id, Context.Message.Author.Id, amountOfTurns, stream, async x => {
                            using (var timeoutStream = new MemoryStream())
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
                catch (ChessException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            });
        }
    }   
}