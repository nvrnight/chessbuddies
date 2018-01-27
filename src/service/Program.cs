using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ChessBuddies.Services;
using ChessDotNet;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChessBuddies.Chess.Exceptions;
using System.Threading;
using Newtonsoft.Json;
using ChessBuddies.Chess.Models;
using System.Collections.Generic;

namespace ChessBuddies
{
    class Program
    {
        public static AutoResetEvent ShutdownEvent  { get; set; } = new AutoResetEvent(false);
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

            var adminUsernamesCsv = config["admins"];
            var adminUsernames = adminUsernamesCsv?.Split(',') ?? new string[] {};
            
            int timeout;
            if(!int.TryParse(config["confirmationsTimeout"], out timeout))
                timeout = 30000;

            var discordBotsApiKey = config["discordBotsApiKey"];
            var discordBotsBotId = config["discordBotsBotId"];

            _services = new ServiceCollection()
                .AddSingleton<IAssetService, AssetService>()
                .AddSingleton<IDiscordBotsService, DiscordBotsService>(s => new DiscordBotsService(discordBotsApiKey, discordBotsBotId))
                .AddSingleton<IChessService, ChessService>(s => new ChessService(timeout, s.GetService<IAssetService>()))
                .AddSingleton<IAuthorizationService, AuthorizationService>(s => new AuthorizationService(adminUsernames))
                .AddSingleton<ChessGame, ChessGame>()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _chessService = _services.GetService<IChessService>();
            
            await InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            var stateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "state.json");

            if(!string.IsNullOrEmpty(discordBotsApiKey))
            {
                var discordBotsService = _services.GetService<IDiscordBotsService>();

                async Task postStats() {
                    await discordBotsService.UpdateStats(_client.Guilds.Count);
                };

                _client.Ready += postStats;
                _client.JoinedGuild += async (c) => { await postStats(); };
                _client.LeftGuild += async (c) => { await postStats(); };
            }

            _client.Ready += async () => {
                await Task.Run(async () => {
                    if(System.IO.File.Exists(stateFilePath))
                    {
                        var deserializedMatches = JsonConvert.DeserializeObject<List<ChessMatch>>(System.IO.File.ReadAllText(stateFilePath));
                        await _chessService.LoadState(deserializedMatches, _client);
                    }
                });
            };

            ShutdownEvent.WaitOne();

            System.IO.File.WriteAllText(stateFilePath, JsonConvert.SerializeObject(_chessService.Matches));

            await _client.SetGameAsync(null);
            await _client.StopAsync();
            await _client.LogoutAsync();
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

        private async Task HandleMoveLogic(SocketCommandContext context, SocketMessage message)
        {
            try
            {
                using(var stream = new MemoryStream())
                {
                    var moveResult = await _chessService.Move(stream, context.Channel.Id, context.Message.Author.Id, message.Content.Substring(1, message.Content.Length - 1));

                    if(moveResult.IsOver)
                    {
                        var overMessage = moveResult.Winner != null ? $"Checkmate! {moveResult.Winner.Value.Mention()} has won the match." : "Stalemate!";

                        await context.Channel.SendMessageAsync(overMessage);
                    }
                    else
                    {
                        var nextPlayer = await _chessService.WhoseTurn(context.Channel.Id, context.Message.Author.Id);

                        var yourMoveMessage = $"Your move {nextPlayer.Mention()}.";

                        if(moveResult.IsCheck)
                            yourMoveMessage += " Check!";

                        await context.Channel.SendMessageAsync(yourMoveMessage);
                    }

                    stream.Position = 0;
                    await context.Channel.SendFileAsync(stream, "board.png");
                }
            }
            catch(ChessException ex)
            {
                await context.Channel.SendMessageAsync(ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await context.Channel.SendMessageAsync("An Unexpected error occurred");
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            await Task.Run(async () => {
                // Don't process the command if it was a System Message
                var message = messageParam as SocketUserMessage;
                if (message == null || message.Author.IsBot) return;            
                // Create a number to track where the prefix ends and the command begins
                int argPos = 0;
                // Determine if the message is a command, based on if it starts with '!' or a mention prefix
                var mentionsBot = message.Content.Trim() == _client.CurrentUser.Mention.Replace("<@!", "<@");

                if (!(message.HasCharPrefix('!', ref argPos)) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos) && !mentionsBot) return;

                // Create a Command Context
                var context = new SocketCommandContext(_client, message);

                if(mentionsBot)
                {
                    await context.Channel.SendMessageAsync("Use !help for a list of commands.");
                    return;
                }
                
                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully)

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    async Task sendError(string e)
                    {
                        await context.Channel.SendMessageAsync(e);
                    }

                    if(result.ErrorReason == "Unknown command.")
                    {
                        if(await _chessService.PlayerIsInGame(context.Channel.Id, context.Message.Author.Id))
                        {
                            await HandleMoveLogic(context, message);
                        }
                    }
                    else
                    {
                        Console.WriteLine(result.Error.Value.ToString());
                        await sendError("An Unexpected error occurred");
                    }
                }
            });
        }
    }
}
