using System.Threading.Tasks;
using Discord.Commands;

namespace src
{
    public class HelpQuestionMarkCommand : ModuleBase<SocketCommandContext>
    {
        //
        [Command("?")]
        public async Task SayAsync()
        {

            await this.ReplyAsync(Help.CommandsHelpText);
        }
    }

    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        //
        [Command("help")]
        public async Task SayAsync()
        {

            await this.ReplyAsync(Help.CommandsHelpText);
        }
    }
}

