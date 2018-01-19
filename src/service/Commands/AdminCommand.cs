using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public abstract class AdminCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IAuthorizationService _authorizationService;
        protected AdminCommand(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        protected async Task Authorize()
        {
            if(!(await _authorizationService.IsAdmin(Context)))
                throw new Exception("Unknown command.");
        }
    }
}