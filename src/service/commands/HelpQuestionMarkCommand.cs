using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public class HelpQuestionMarkCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IAuthorizationService _authorizationService;
        public HelpQuestionMarkCommand(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }
        [Command("?")]
        public async Task SayAsync()
        {
            await this.ReplyAsync(Help.GetCommandsHelpText(await _authorizationService.IsAdmin(Context)));
        }
    }

    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IAuthorizationService _authorizationService;
        public HelpCommand(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Command("help")]
        public async Task SayAsync()
        {
            await this.ReplyAsync(Help.GetCommandsHelpText(await _authorizationService.IsAdmin(Context)));
        }
    }
}

