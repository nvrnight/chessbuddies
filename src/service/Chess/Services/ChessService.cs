
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChessDotNet;
using Discord;
using ChessBuddies.Chess.Exceptions;
using ChessBuddies.Chess.Models;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using ChessDotNet.Pieces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace ChessBuddies.Services
{
    public interface IChessService
    {
        Task<ChessServiceStatus> GetStatus();
        Task<ulong> WhoseTurn(ulong channel, ulong player);
        Task<ChessMatch> GetMatch(ulong channel, ulong player);
        Task WriteBoard(ulong channel, ulong player, Stream stream);
        Task WriteBoard(ChessMove lastMove, ChessGame chessGame, Stream stream);
        Task<ChessMatchStatus> Move(Stream stream, ulong channel, ulong player, string rawMove);
        List<ChessChallenge> Challenges { get; }
        List<ChessMatch> Matches { get; set; }
        Task<ChessChallenge> Challenge(ulong channel, ulong player1, ulong player2, Action<ChessChallenge> onTimeout = null);
        Task<ChessMatch> AcceptChallenge(ulong channel, ulong player);
        Task<bool> HasChallenge(ulong channel, ulong player);
        Task<ChessMatch> Resign(ulong channel, ulong player);
        Task<bool> PlayerIsInGame(ulong channel, ulong player);
        int ConfirmationsTimeout { get; }

        Task Undo(ulong channel, ulong player);
        Task<UndoRequest> UndoRequest(ulong channel, ulong player, int amount, Stream stream, Action<ChessMatch> onTimeout = null);
        Task<bool> HasUndoRequest(ulong channel, ulong player);
        Task LoadState(List<ChessMatch> matches, DiscordSocketClient client);
    }
    public class ChessService : IChessService
    {
        private readonly int _confirmationsTimeout;
        private readonly IAssetService _assetService;
        public ChessService(int confirmationsTimeout, IAssetService assetService)
        {
            _confirmationsTimeout = confirmationsTimeout;
            _assetService = assetService;
        }
        public int ConfirmationsTimeout { get { return _confirmationsTimeout; } }
        private List<ChessMatch> _chessMatches = new List<ChessMatch>();
        private List<ChessChallenge> _challenges = new List<ChessChallenge>();
        public List<ChessChallenge> Challenges { get { return _challenges; } }
        public List<ChessMatch> Matches
        {
            get{ return _chessMatches; }
            set
            {
                _chessMatches = value;
            }
        }
        public async Task<ChessServiceStatus> GetStatus()
        {
            return await Task.FromResult(new ChessServiceStatus {
                ChallengeRequestsInProgress = _challenges.Count(),
                MatchesInProgress = _chessMatches.Select(x =>
                    new MatchStatus
                    {
                        Id = x.Id,
                        LastMoveDate = x.History.OrderByDescending(h => h.MoveDate).FirstOrDefault()?.MoveDate 
                    }).ToArray()
            });
        }
        public async Task<ulong> WhoseTurn(ulong channel, ulong player)
        {
            var match = await GetMatch(channel, player);

            return match.Game.WhoseTurn == Player.White ? match.Challenger : match.Challenged;
        }
        public async Task<ChessMatch> GetMatch(ulong channel, ulong player)
        {
            return await Task.FromResult(_chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player)));
        }
        private void DrawImage(IImageProcessingContext<Rgba32> processor, string name, int x, int y)
        {
            var pieceSquare = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath($"{name}.png"));
            processor.DrawImage(pieceSquare, new Size(50, 50), new Point(x * 50 + 117, y * 50 + 19), new GraphicsOptions());
        }
        public async Task WriteBoard(ChessMove lastMove, ChessGame chessGame, Stream stream)
        {
            await Task.Run(() => {
                var board = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("board.png"));
                
                var boardPieces = chessGame.GetBoard();

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
                                    ((int)lastMove.OriginalFile == columnIndex && RankToRowMap[lastMove.OriginalRank] == rowIndex) ||
                                    ((int)lastMove.NewFile == columnIndex && RankToRowMap[lastMove.NewRank] == rowIndex)
                                )
                            )
                                DrawImage(processor, "yellow_square", columnIndex, rowIndex);

                            var piece = boardPieces[rowIndex][columnIndex];

                            if(piece != null)
                            {
                                var fenCharacter = piece.GetFenCharacter();

                                if(fenCharacter.ToString().ToUpper() == "K" && chessGame.IsInCheck(piece.Owner))
                                    DrawImage(processor, "red_square", columnIndex, rowIndex);

                                var prefix = "white";

                                if(new[] {'r', 'n', 'b', 'q', 'k', 'p'}.Contains(fenCharacter))
                                    prefix = "black";

                                DrawImage(processor, $"{prefix}_{fenCharacter.ToString().ToLower()}", columnIndex, rowIndex);
                            }
                        }
                    }
                });

                board.SaveAsPng(stream);
            });
        }
        public async Task WriteBoard(ulong channel, ulong player, Stream stream)
        {
            await Task.Run(async () => {
                var match = _chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player));

                if(match == null)
                    throw new ChessException("You are not in a game.");

                var lastMove = match.History.OrderByDescending(x => x.MoveDate).FirstOrDefault();

                await WriteBoard(lastMove, match.Game, stream);
            });
        }
        public async Task<ChessChallenge> Challenge(ulong channel, ulong player1, ulong player2, Action<ChessChallenge> onTimeout = null)
        {
            return await Task.Run(async () => {
                if(await PlayerIsInGame(channel, player1))
                    throw new ChessException($"{player1.Mention()} is currently in a game.");

                if(await PlayerIsInGame(channel, player2))
                    throw new ChessException($"{player2.Mention()} is currently in a game.");

                if(_challenges.Any(x => x.Channel == channel && x.Challenged == player1 && x.Challenger == player2))
                    throw new ChessException($"{player1.Mention()} has already challenged {player2.Mention()}.");

                var challenge = new ChessChallenge { ChallengeDate = DateTime.UtcNow, Channel = channel, Challenger = player1, Challenged = player2 };
                
                _challenges.Add(challenge);
                
                RemoveChallenge(challenge, onTimeout);

                return await Task.FromResult(challenge);
            });
        }

        public async Task<ChessMatch> Resign(ulong channel, ulong player)
        {
            return await Task.Run(async () => {
                var match = _chessMatches.SingleOrDefault(x => x.Channel == channel && x.Players.Contains(player));

                if(match == null)
                    throw new ChessException($"You are not currently in a game.");

                _chessMatches.Remove(match);

                return await Task.FromResult(match);
            });
            
        }

        public async Task<ChessMatch> AcceptChallenge(ulong channel, ulong player)
        {
            return await Task.Run(async () => {
                if(await PlayerIsInGame(channel, player))
                    throw new ChessException($"{player.Mention()} is currently in a game.");

                var challenge = _challenges.Where(x => x.Channel == channel && x.Challenged == player).OrderBy(x => x.ChallengeDate).FirstOrDefault();

                if(challenge == null)
                    throw new ChessException($"No challenge exists for you to accept.");

                if(await PlayerIsInGame(channel, challenge.Challenger))
                    throw new ChessException($"{challenge.Challenger.Mention()} is currently in a game.");

                var chessGame = new ChessGame();
                var chessMatch = new ChessMatch { Channel = channel, Game = chessGame, Challenger = challenge.Challenger, Challenged = challenge.Challenged };

                _challenges.Remove(challenge);
                _chessMatches.Add(chessMatch);

                return await Task.FromResult<ChessMatch>(chessMatch);
            });
        }

        public async Task<ChessMatchStatus> Move(Stream stream, ulong channel, ulong player, string rawMove)
        {
            return await Task.Run(async () => {
                var moveInput = rawMove.Replace(" ", "").ToUpper();

                if(!Regex.IsMatch(moveInput, "^[A-H][1-8][A-H][1-8][Q|N|B|R]?$"))
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

                char? promotionChar = moveInput.Length > 4 ? moveInput[4].ToString().ToLower()[0] : 'q';

                var positionEnumValues = (IEnumerable<ChessDotNet.File>)Enum.GetValues(typeof(ChessDotNet.File));

                var sourcePositionX = positionEnumValues.Single(x => x.ToString("g") == sourceX);
                var destPositionX = positionEnumValues.Single(x => x.ToString("g") == destX);

                var originalPosition = new ChessDotNet.Position(sourcePositionX, int.Parse(sourceY));
                var destPosition = new ChessDotNet.Position(destPositionX, int.Parse(destY));

                var piece = match.Game.GetPieceAt(originalPosition);

                if(piece != null && !(piece is Pawn))
                    promotionChar = null;

                var move = new Move(originalPosition, destPosition, whoseTurn, promotionChar);

                if(!match.Game.IsValidMove(move))
                    throw new ChessException("Invalid move.");

                var chessMove = new ChessMove
                {
                    NewRank = move.NewPosition.Rank,
                    NewFile = move.NewPosition.File,
                    OriginalFile = move.OriginalPosition.File,
                    OriginalRank = move.OriginalPosition.Rank,
                    MoveDate = DateTime.Now,
                    PreviousWhoseTurn = match.Game.WhoseTurn
                };

                var board = match.Game.GetBoard();
                for(var column = 0; column < board.Length; column++)
                {
                    for(var row = 0; row < board[column].Length; row++)
                    {
                        chessMove.PreviousBoardState[column][row] = board[column][row];
                    }
                }

                match.Game.ApplyMove(move, true);
                match.History.Add(chessMove);
                match.UndoRequest = null;

                var checkMated = match.Game.IsCheckmated(otherPlayer);
                var isOver = checkMated || match.Game.IsStalemated(otherPlayer);

                var status = new ChessMatchStatus
                {
                    IsOver = isOver,
                    Winner = isOver && checkMated ? player : (ulong?)null,
                    IsCheck = match.Game.IsInCheck(otherPlayer)
                };

                await WriteBoard(channel, player, stream);

                if(isOver)
                    _chessMatches.Remove(match);

                return await Task.FromResult(status);
            });
        }

        public async Task<bool> PlayerIsInGame(ulong channel, ulong player)
        {
            return await Task.FromResult<bool>(_chessMatches.Any(x => x.Channel == channel && x.Players.Contains(player)));
        }

        private async void RemoveUndoRequest(ChessMatch match, Action<ChessMatch> onTimeout)
        {
            await Task.Run(async () => {
                await Task.Delay(_confirmationsTimeout);
                if(match.UndoRequest != null)
                {
                    match.UndoRequest = null;
                    onTimeout(match);
                }
            });
        }

        private async void RemoveChallenge(ChessChallenge challenge, Action<ChessChallenge> onTimeout)
        {
            await Task.Run(async () => {
                await Task.Delay(_confirmationsTimeout);

                if(_challenges.Contains(challenge))
                {
                    _challenges.Remove(challenge);
                    if(onTimeout != null)
                        onTimeout(challenge);
                }
            });
        }

        public async Task<UndoRequest> UndoRequest(ulong channel, ulong player, int amount, Stream stream, Action<ChessMatch> onTimeout = null)
        {
            return await Task.Run(async () => {
                var match = await GetMatch(channel, player);

                if(match == null)
                    throw new ChessException("You are not in a game.");
                
                var historyCount = match.History.Count();
                if(historyCount < amount)
                    amount = historyCount;

                var moveToRevert = match.History.OrderByDescending(x => x.MoveDate).Skip(amount - 1).FirstOrDefault();
                if(moveToRevert == null)
                    throw new ChessException("Nothing to undo.");

                var userWhoseTurn = match.Game.WhoseTurn == Player.White ? match.Challenged : match.Challenger;

                if(userWhoseTurn != player)
                    throw new ChessException("You can't undo another players turn.");
                
                if(match.UndoRequest != null)
                    throw new ChessException("Undo request is already in process.");
                
                var undoRequest = new UndoRequest { CreatedDate = DateTime.UtcNow, CreatedBy = player, Amount = amount };

                match.UndoRequest = undoRequest;

                RemoveUndoRequest(match, onTimeout);

                var previousMove = match.History.OrderByDescending(x => x.MoveDate).Skip(amount).FirstOrDefault();

                await WriteBoard(previousMove, new ChessGame(new GameCreationData { Board = moveToRevert.PreviousBoardState, WhoseTurn = moveToRevert.PreviousWhoseTurn}), stream);

                return await Task.FromResult(undoRequest);
            });
        }

        public async Task Undo(ulong channel, ulong player)
        {
            await Task.Run(async () => {
                var match = await GetMatch(channel, player);

                if(match == null)
                    throw new ChessException("You are not in a game.");

                var amount = match.UndoRequest.Amount;
                var historyCount = match.History.Count();
                if(historyCount < amount)
                    amount = historyCount;

                match.History = match.History.OrderByDescending(x => x.MoveDate).Skip(amount - 1).ToList();

                var move = match.History.OrderByDescending(x => x.MoveDate).FirstOrDefault();

                if(move == null)
                    throw new ChessException("Nothing to undo.");

                var board = match.Game.GetBoard();

                for(var column = 0; column < move.PreviousBoardState.Length; column++)
                {
                    for(var row = 0; row < move.PreviousBoardState[column].Length; row++)
                    {
                        board[column][row] = move.PreviousBoardState[column][row];
                    }
                }
                var whoseTurn = match.Game.WhoseTurn == Player.White ? Player.Black : Player.White;

                match.Game = new ChessGame(new GameCreationData { Board = board, WhoseTurn = whoseTurn });

                match.History.Remove(move);
                match.UndoRequest = null;
            });
        }

        public async Task<bool> HasChallenge(ulong channel, ulong player)
        {
            return await Task.FromResult(_challenges.Any(x => x.Channel == channel && x.Challenged == player));
        }

        public async Task<bool> HasUndoRequest(ulong channel, ulong player)
        {
            return await Task.Run(async () => {
                var match = await GetMatch(channel, player);

                if(match?.UndoRequest != null)
                {
                    var playerWhoCanAccept = match.UndoRequest.CreatedBy == match.Challenger ? match.Challenged : match.Challenger;

                    return await Task.FromResult(playerWhoCanAccept == player);
                }

                return await Task.FromResult(false);
            });
        }

        public async Task LoadState(List<ChessMatch> matches, DiscordSocketClient client)
        {
            await Task.Run(() => {
                foreach(var match in matches)
                {
                    try
                    {
                        match.Game = new ChessGame();

                        foreach(var move in match.History.OrderBy(x => x.MoveDate))
                        {
                            move.PreviousBoardState = match.Game.GetBoard();
                            match.Game.ApplyMove(new Move(new Position(move.OriginalFile, move.OriginalRank), new Position(move.NewFile, move.NewRank), match.Game.WhoseTurn), true);
                        }
                    
                        Matches.Add(match);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed to load match.");
                        Console.WriteLine(JsonConvert.SerializeObject(match));
                        Console.WriteLine(ex.ToString());
                    }                
                }
            });
        }
    }    
}