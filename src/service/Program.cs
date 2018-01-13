using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ChessDotNet;

namespace src
{
    class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private IChessService _chessService;
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _client.Log += Log;

            string token = config["token"];

            _services = new ServiceCollection()
                .AddSingleton<IAssetService, AssetService>()
                .AddSingleton<IChessService, ChessService>()
                .AddSingleton<ChessGame, ChessGame>()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _chessService = _services.GetService<IChessService>();
            
            await InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
		}

        private Task Log(LogMessage message)
		{
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
		}

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos)) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess) {
                Func<string, Task> sendError = async (e) => { await context.Channel.SendMessageAsync(e); };

                if(result.ErrorReason == "Unknown command.")
                {
                    try
                    {
                        var move = _chessService.Move(message.Content.Substring(1, message.Content.Length - 1));
                        await context.Channel.SendMessageAsync(JsonConvert.SerializeObject(move));
                    }
                    catch(ChessException ex)
                    {
                        await sendError(ex.Message);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        await sendError("An Unexpected error occurred");
                    }
                }
                else
                {
                    await sendError(result.ErrorReason);
                }
            }
        }
    }
}
