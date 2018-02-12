using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessBuddies
{
    public class IsAdmin : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            var adminUsernamesCsv = config["admins"];
            var adminUsernames = adminUsernamesCsv?.Split(',') ?? new string[] { };
            if (adminUsernames.Contains($"{context.Message.Author.Username}#{context.Message.Author.DiscriminatorValue}"))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("User is not an Admin"));
        }
    }
}