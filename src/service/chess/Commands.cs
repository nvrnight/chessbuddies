using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;

namespace src
{
    public class RevertCommand : ModuleBase<SocketCommandContext>
    {
        [Command("revert")]
        public async Task SayAsync()
        {
            await this.ReplyAsync($"Revert command called.");
        }
    }
    public class EndCommand : ModuleBase<SocketCommandContext>
    {
        [Command("end")]
        public async Task SayAsync()
        {
            await this.ReplyAsync($"End command called.");
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
                var match = await _chessService.AcceptChallenge(Context.Channel.Id, this.Context.Message.Author);

                await this.ReplyAsync($"Match has started between {match.Challenger.Mention} and {match.Challenged.Mention}.");
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
    }
    public class ListChallengesCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public ListChallengesCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("listchallenges")]
        public async Task SayAsync(string message = "")
        {
            await this.ReplyAsync(JsonConvert.SerializeObject(_chessService.Challenges));
        }
    }
    public class ListMatchesCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IChessService _chessService;

        public ListMatchesCommand(IChessService chessService)
        {
            _chessService = chessService;
        }

        [Command("listmatches")]
        public async Task SayAsync(string message = "")
        {
            await this.ReplyAsync(JsonConvert.SerializeObject(_chessService.Matches));
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

                var challenge = await _chessService.Challenge(Context.Channel.Id, this.Context.Message.Author, user);

                ChallengeTimeout(challenge);

                await this.ReplyAsync(this.Context.Message.Author.Mention + $" is challenging {user.Mention}.");
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
        private async void ChallengeTimeout(ChessChallenge challenge)
        {
            await Task.Delay(ChessService.ChallengeTimeout);

            if(!_chessService.Matches.Any(x => x.Channel == challenge.Channel && x.Players.Contains(challenge.Challenged) && x.Players.Contains(challenge.Challenger)))
                await this.ReplyAsync($"Challenge timed out for {challenge.Challenger.Mention} challenging {challenge.Challenged.Mention}");
        }
    }
    public class ResignCommand : ModuleBase<SocketCommandContext>
    {
        [Command("resign")]
        public async Task SayAsync()
        {
            await this.ReplyAsync("Resign command called.");
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
                await this.ReplyAsync(JsonConvert.SerializeObject(_chessService.Move(message)));
            }
            catch(ChessException ex)
            {
                await this.ReplyAsync(ex.Message);
            }
        }
    }
}