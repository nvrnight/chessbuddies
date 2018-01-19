using System.Collections.Generic;
using ChessDotNet;
using Discord;
using Newtonsoft.Json;

namespace ChessBuddies.Chess.Models
{
    public class ChessMatch
    {
        public List<ChessMove> History {get; set;} = new List<ChessMove>();
        public UndoRequest UndoRequest {get; set;}
        public ChessGame Game {get; set;}
        [JsonIgnore]
        public IUser Challenger {get; set;}
        [JsonIgnore]
        public IUser Challenged {get; set;}
        [JsonIgnore]
        public IUser[] Players { get { return new[] { Challenger, Challenged }; } }
        public ulong ChallengerId {get { return Challenger.Id; } }
        public ulong ChallengedId {get { return Challenger.Id; } }
        public ulong Channel {get; set;}
    }
}