using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBuddies.Commands
{
    [Group("help"), Name("Help"), Alias("?")]
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        public HelpCommand(IServiceProvider provider)
        {
            _commands = provider.GetService<CommandService>();
            _provider = provider;
        }
        [Command]
        public async Task HelpAsync()
        {
            string prefix = "!" ?? $"@{Context.Client.CurrentUser.Username} ";
            var commands = _commands.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary))
                     .GroupBy(x => x.Name)
                     .Select(x => x.First());
            var embed = new EmbedBuilder()
                .WithFooter(x => x.Text = $"Type `{prefix}help <command>` for more information");

            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                    embed.AddField(prefix + command.Aliases.First(), command.Summary);
            }


            await ReplyAsync("", embed: embed.Build());
        }

        [Command]
        public async Task HelpAsync(string commandName)
        {
            string alias = $"{commandName}".ToLower();
            string prefix = "!" ?? $"@{Context.Client.CurrentUser.Username} ";

            var commands = _commands.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary));


            var command = commands.Where(x => x.Aliases.Contains(alias));
            var embed = new EmbedBuilder();

            var aliases = new List<string>();
            foreach (var overload in command)
            {
                var result = await overload.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                {
                    var sbuilder = new StringBuilder()
                        .Append(prefix + overload.Aliases.First());

                    foreach (var parameter in overload.Parameters)
                    {
                        string p = parameter.Name;
                        p = p.First().ToString().ToUpper() + p.Substring(1);

                        if (parameter.IsRemainder)
                            p += "...";
                        if (parameter.IsOptional)
                            p = $"[{p}]";
                        else
                            p = $"<{p}>";

                        sbuilder.Append(" " + p);
                    }

                    embed.AddField(sbuilder.ToString(), overload.Remarks ?? overload.Summary);
                }
                aliases.AddRange(overload.Aliases);
            }

            embed.WithFooter(x => x.Text = $"Aliases: {string.Join(", ", aliases)}");

            await ReplyAsync("", embed: embed.Build());
        }
    }
}