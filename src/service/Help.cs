using System.Linq;
using System.Reflection;
using ChessBuddies.Commands;
using Discord.Commands;

namespace ChessBuddies
{
    public class Help
    {
        public static string GetCommandsHelpText(bool isAdmin)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x =>
                !x.IsAbstract &&
                (
                    (isAdmin || !x.IsSubclassOf(typeof(AdminCommand))) &&
                    x.IsSubclassOf(typeof(ModuleBase<SocketCommandContext>))
                )
            );

            var attributes = types.Select(x => x.GetMethod("SayAsync")).Select(m => m.GetCustomAttribute<CommandAttribute>(true));

            return "List of known commands: " + string.Join(", ", attributes.Select(x => x.Text));
        }
    }
}