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
    public class ChallengeCommand : ModuleBase<SocketCommandContext>
    {
        [Command("challenge")]
        public async Task SayAsync(string message = "")
        {
            message = message.Trim();
            if(!Regex.IsMatch(message, @"<@\d+>"))
            {
                await this.ReplyAsync("Invalid challenge, example: challenge @PersonName");
                return;
            }

            await this.ReplyAsync($"Challenging {message}.");
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
            catch(ChessMoveParseException)
            {
                await this.ReplyAsync("Error parsing move. Example move: a2a4");
            }
        }
    }
}