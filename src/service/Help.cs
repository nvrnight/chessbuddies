using System.Linq;
using System.Reflection;
using Discord.Commands;

namespace ChessBuddies
{
    public class Help
    {
        public static string CommandsHelpText
        {
            get
            {
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ModuleBase<SocketCommandContext>)));

                var methods = types.Select(x => x.GetMethods().Where(m => m.Name == "SayAsync").Single());

                var attributes = methods.Select(m => (CommandAttribute)m.GetCustomAttributes(typeof(CommandAttribute), true).Single());

                return "List of known commands: " + string.Join(", ", attributes.Select(x => x.Text));
            }
        }
    }
}