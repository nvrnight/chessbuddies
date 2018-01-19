using System.Threading.Tasks;
using Discord.Commands;

namespace ChessBuddies.Commands
{
    public class HelpQuestionMarkCommand : ModuleBase<SocketCommandContext>
    {
        //
        [Command("?")]
        public async Task SayAsync()
        {

            await this.ReplyAsync(Help.GetCommandsHelpText());
        }
    }

    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        //
        [Command("help")]
        public async Task SayAsync()
        {

            await this.ReplyAsync(Help.GetCommandsHelpText());
        }
    }
}

