# Chess Buddies
A .NET Core based Discord chess bot

## Instructions

### Discord Commands(!name, Example, Description):
* !? or !help
* **!challenge**, **!challenge @SomeDiscordUser**, challenge another player to a match
* **!accept**, accept a match challenge or undo request
* **!move**, **!move a2a4** or **!a2a4**, move your chess piece
* **!resign**, resign the match
* **!show**, display the board
* **!undo**, request the last move be undone

### Installing and running a release.
Pre-requisites: Install .NET Core 2.0.5 Runtime [32-bit](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x86-installer) or [64-bit](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x64-installer).

1. Download the current release from the Releases tab
2. Unzip it to a directory.
3. Open powershell in the directory(CTRL+SHIFT+Right Click -> Open Powershell Window Here).
4. Edit appsettings.json and put your bot's token in the token field. If you don't know how to create a Discord bot or get your bot's token, follow the instructions here: https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token
5. Run **dotnet ChessBuddies.dll**
