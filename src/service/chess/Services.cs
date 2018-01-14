using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChessDotNet;
using Discord;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace src
{
    public interface IChessService
    {
        Task WriteBoard(ulong channel, IUser player, Stream stream);
        Task<ChessMatchStatus> Move(Stream stream, ulong channel, IUser player, string rawMove);
        List<ChessChallenge> Challenges { get; }
        List<ChessMatch> Matches { get; }
        Task<ChessChallenge> Challenge(ulong channel, IUser player1, IUser player2);
        Task<ChessMatch> AcceptChallenge(ulong channel, IUser player);
        Task<ChessMatch> Resign(ulong channel, IUser player);
        Task<bool> PlayerIsInGame(ulong channel, IUser player);
        int ChallengeTimeout { get; }
    }
    public class ChessService : IChessService
    {
        private readonly int _challengeTimeout;
        private readonly IAssetService _assetService;
        public ChessService(int challengeTimeout, IAssetService assetService)
        {
            _challengeTimeout = challengeTimeout;
            _assetService = assetService;
        }
        public int ChallengeTimeout { get { return _challengeTimeout; } }
        private List<ChessMatch> _chessMatches = new List<ChessMatch>();
        private List<ChessChallenge> _challenges = new List<ChessChallenge>();
        public List<ChessChallenge> Challenges { get { return _challenges; } }
        public List<ChessMatch> Matches { get { return _chessMatches; } }

        private void DrawImage(IImageProcessingContext<Rgba32> processor, string name, int x, int y)
        {
            var pieceSquare = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath($"{name}.png"));
            processor.DrawImage(pieceSquare, new Size(50, 50), new Point(x * 50 + 117, y * 50 + 19), new GraphicsOptions());
        }
        public async Task WriteBoard(ulong channel, IUser player, Stream stream)
        {
            await Task.Run(() => {
                var match = _chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player));

                if(match == null)
                    throw new ChessException("You are not in a game.");

                var board = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("board.png"));
                
                var boardPieces = match.Game.GetBoard();

                var lastMove = match.History.OrderByDescending(x => x.MoveDate).FirstOrDefault();

                Dictionary<int, int> RankToRowMap = new Dictionary<int, int>
                {
                    { 1, 7 },
                    { 2, 6 },
                    { 3, 5 },
                    { 4, 4 },
                    { 5, 3 },
                    { 6, 2 },
                    { 7, 1 },
                    { 8, 0 }
                };

                board.Mutate(processor => {
                    for(var columnIndex = 0; columnIndex < boardPieces.Length; columnIndex++)
                    {
                        for(var rowIndex = 0; rowIndex < boardPieces[columnIndex].Length; rowIndex++)
                        {
                            if(
                                lastMove != null &&
                                (
                                    ((int)lastMove.Move.OriginalPosition.File == columnIndex && RankToRowMap[lastMove.Move.OriginalPosition.Rank] == rowIndex) ||
                                    ((int)lastMove.Move.NewPosition.File == columnIndex && RankToRowMap[lastMove.Move.NewPosition.Rank] == rowIndex)
                                )
                            )
                                DrawImage(processor, "yellow_square", columnIndex, rowIndex);

                            var piece = boardPieces[rowIndex][columnIndex];

                            if(piece != null)
                            {
                                var fenCharacter = piece.GetFenCharacter();

                                var prefix = "white";

                                if(new[] {'r', 'n', 'b', 'q', 'k', 'p'}.Contains(fenCharacter))
                                    prefix = "black";

                                DrawImage(processor, $"{prefix}_{fenCharacter}", columnIndex, rowIndex);
                            }
                        }
                    }
                });

                board.SaveAsPng(stream);
            });
        }
        public async Task<ChessChallenge> Challenge(ulong channel, IUser player1, IUser player2)
        {
            if(await PlayerIsInGame(channel, player1))
                throw new ChessException($"{player1.Mention} is currently in a game.");

            if(await PlayerIsInGame(channel, player2))
                throw new ChessException($"{player2.Mention} is currently in a game.");

            if(_challenges.Any(x => x.Channel == channel && x.Challenged == player1 && x.Challenger == player2))
                throw new ChessException($"{player1.Mention} has already challenged {player2.Mention}.");

            var challenge = new ChessChallenge { ChallengeDate = DateTime.UtcNow, Channel = channel, Challenger = player1, Challenged = player2 };
            
            _challenges.Add(challenge);
            
            RemoveChallenge(challenge);

            return challenge;
        }

        public async Task<ChessMatch> Resign(ulong channel, IUser player)
        {
            var match = _chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player));

            if(match == null)
                throw new Exception($"You are not currently in a game.");

            _chessMatches.Remove(match);

            return await Task.FromResult(match);
        }

        public async Task<ChessMatch> AcceptChallenge(ulong channel, IUser player)
        {
            if(await PlayerIsInGame(channel, player))
                throw new ChessException($"{player.Mention} is currently in a game.");

            var challenge = _challenges.Where(x => x.Channel == channel && x.Challenged == player).OrderBy(x => x.ChallengeDate).FirstOrDefault();

            if(challenge == null)
                throw new ChessException($"No challenge exists for you to accept.");

            if(await PlayerIsInGame(channel, challenge.Challenger))
                throw new ChessException($"{challenge.Challenger.Mention} is currently in a game.");

            var chessGame = new ChessGame();
            var chessMatch = new ChessMatch { Channel = channel, Game = chessGame, Challenger = challenge.Challenger, Challenged = challenge.Challenged };

            _challenges.Remove(challenge);
            _chessMatches.Add(chessMatch);

            return await Task.FromResult<ChessMatch>(chessMatch);
        }

        public async Task<ChessMatchStatus> Move(Stream stream, ulong channel, IUser player, string rawMove)
        {
            var moveInput = rawMove.Replace(" ", "").ToUpper();

            if(!Regex.IsMatch(moveInput, "[A-H][1-8][A-H][1-8]"))
                throw new ChessException("Error parsing move. Example move: a2a4");

            var match = _chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player));

            if(match == null)
                throw new ChessException("You are not currently in a game");

            var whoseTurn = match.Game.WhoseTurn;
            var otherPlayer = whoseTurn == Player.White ? Player.Black : Player.White;

            if((whoseTurn == Player.White && player != match.Challenger) || (whoseTurn == Player.Black && player != match.Challenged))
                throw new ChessException("It's not your turn.");

            var sourceX = moveInput[0].ToString();
            var sourceY = moveInput[1].ToString();
            var destX = moveInput[2].ToString();
            var destY = moveInput[3].ToString();
            var positionEnumValues = (IEnumerable<ChessDotNet.File>)Enum.GetValues(typeof(ChessDotNet.File));

            var sourcePositionX = positionEnumValues.Single(x => x.ToString("g") == sourceX);
            var destPositionX = positionEnumValues.Single(x => x.ToString("g") == destX);

            var originalPosition = new ChessDotNet.Position(sourcePositionX, int.Parse(sourceY));
            var destPosition = new ChessDotNet.Position(destPositionX, int.Parse(destY));

            var move = new Move(originalPosition, destPosition, whoseTurn);

            if(!match.Game.IsValidMove(move))
                throw new ChessException("Invalid move.");

            match.Game.ApplyMove(move, true);
            match.History.Add(new ChessMove { Move = move, MoveDate = DateTime.Now });

            var checkMated = match.Game.IsCheckmated(otherPlayer);
            var isOver = checkMated || match.Game.IsStalemated(otherPlayer);

            var status = new ChessMatchStatus
            {
                IsOver = isOver,
                Winner = isOver && checkMated ? player : null
            };

            await WriteBoard(channel, player, stream);

            if(isOver)
                _chessMatches.Remove(match);

            return await Task.FromResult(status);
        }

        public async Task<bool> PlayerIsInGame(ulong channel, IUser player)
        {
            return await Task.FromResult<bool>(_chessMatches.Any(x => x.Channel == channel && x.Players.Contains(player)));
        }

        private async void RemoveChallenge(ChessChallenge challenge)
        {
            await Task.Delay(_challengeTimeout);

            if(_challenges.Contains(challenge))
                _challenges.Remove(challenge);
        }
    }    
}