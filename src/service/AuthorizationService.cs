using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies
{
    public interface IAuthorizationService
    {
        Task<bool> IsAdmin(SocketCommandContext context);
    }
    public class AuthorizationService : IAuthorizationService
    {
        private string[] _adminUsernames;
        public AuthorizationService(string[] adminUsernames)
        {
            _adminUsernames = adminUsernames;
        }

        public async Task<bool> IsAdmin(SocketCommandContext context)
        {
            return await Task.FromResult(_adminUsernames.Contains($"{context.Message.Author.Username}#{context.Message.Author.DiscriminatorValue}"));
        }
    }
}