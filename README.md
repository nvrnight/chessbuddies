# Chess Buddies
A .NET Core based Discord chess bot

## Instructions

### Discord Commands(!name, Example, Description):
* !? or !help
* **!challenge**, **!challenge @SomeDiscordUser**, challenge another player to a match
* **!accept**, accept a match challenge or undo request
* **!move**, **!move a2a4** or **!a2a4**, move your piece, if your pawn reaches the other side of the board it will be promoted to queen by default. You can promote your pawn to other pieces if you like, r = Rook, b = Bishop, q = Queen, n = Knight. An example move promoting a white pawn to a Knight would be **!a7a8n**
* **!resign**, resign the match
* **!show**, display the board
* **!undo**, request the last move be undone

### Administration Commands
* **!shutdown**, **!shutdown 5**, this will shutdown the bot in 5 minutes.
* **!status**, retrieves the bot's status

### Add [Chess Buddies](https://discordapp.com/oauth2/authorize?&client_id=400489160441462787&scope=bot&permissions=0) to your Discord server.

### Installing and Hosting your own Chess Buddies bot.
Pre-requisites: Install .NET Core 2.0.5 Runtime [32-bit](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x86-installer) or [64-bit](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x64-installer).

1. Download the [current release](https://github.com/nvrnight/chessbuddies/releases/tag/1.0.8) from the [Releases tab](https://github.com/nvrnight/chessbuddies/releases)
2. Unzip it to a directory.
3. Open powershell in the directory(CTRL+SHIFT+Right Click -> Open Powershell Window Here).
4. Edit appsettings.json and put your Discord account in the admins field and put your bot's token in the token field. If you don't know how to create a Discord bot or get your bot's token, follow the [Discord bot guide](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token).
5. Run **dotnet ChessBuddies.dll**
