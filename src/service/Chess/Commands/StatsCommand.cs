using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChessBuddies.Database;
using Discord.Commands;
using service.Extensions;

namespace ChessBuddies.Chess.Commands
{
    public class StatsCommand : ModuleBase<SocketCommandContext>
    {
        private readonly Db _db;
        public StatsCommand(Db db)
        {
            _db = db;
        }

        private void AppendStats(StringBuilder statsBuilder, string title, int totalGames, int whiteWins, int blackWins, int staleMates, int? totalWins = null)
        {
            statsBuilder.AppendLine(title);
            statsBuilder.AppendLine($"Total Games: {totalGames}");
            if(totalWins != null)
                statsBuilder.AppendLine($"Total Wins: {totalWins}");
            statsBuilder.AppendLine($"White Wins: {whiteWins}({whiteWins.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine($"Black Wins: {blackWins}({blackWins.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine($"Stalemates: {staleMates}({staleMates.GetPercentage(totalGames)}%)");
            statsBuilder.AppendLine();
        }

        [Command("stats")]
        public async Task SayAsync(string message = "")
        {
            await Task.Run(async () =>
            {
                message = message.Trim();

                var authorId = (long)Context.Message.Author.Id;

                var statsBuilder = new StringBuilder();
                statsBuilder.AppendLine("```");

                var gameStats = _db.GameStats.Where(x =>
                    x.challenged == authorId || x.challenger == authorId
                );
                var title = "**Your Stats**";
                var authorWonGames = gameStats.Where(x => x.winner == authorId);
                AppendStats(statsBuilder,
                    title,
                    gameStats.Count(),
                    authorWonGames.Count(x => x.winner == x.challenger),
                    authorWonGames.Count(x => x.winner == x.challenged),
                    gameStats.Count(x => x.winner == null),
                    authorWonGames.Count()
                );

                if (!string.IsNullOrEmpty(message))
                {
                    var matches = Regex.Matches(message, @"<@(\d+)>");
                    
                    if(matches.Any())
                    {
                        var user = await Context.Channel.GetUserAsync(ulong.Parse(matches[0].Groups[1].Value));
                        var userId = (long)user.Id;

                        var authorGamesAgainstUser = gameStats.Where(x => x.challenged == userId || x.challenger == userId);

                        var authorWonGamesAgainstUser = authorGamesAgainstUser.Where(x => x.winner == authorId);
                        AppendStats(statsBuilder,
                            $"**Your Stats vs {user.Username}**",
                            authorGamesAgainstUser.Count(),
                            authorWonGamesAgainstUser.Count(x => x.winner == x.challenger),
                            authorWonGamesAgainstUser.Count(x => x.winner == x.challenged),
                            authorGamesAgainstUser.Count(x => x.winner == null),
                            authorWonGamesAgainstUser.Count()
                        );   
                    }
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
                }

                statsBuilder.Append("```");

                await ReplyAsync(statsBuilder.ToString());
            });
        }
    }
}