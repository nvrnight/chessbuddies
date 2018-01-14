using System;
using System.Collections.Generic;
using ChessDotNet;
using Discord;
using Newtonsoft.Json;

namespace src
{    
    public class ChessMatchStatus
    {
        public bool IsOver {get; set;}
        public IUser Winner {get; set;}
    }
    public class ChessMove
    {
        public Move Move {get; set;}
        public DateTime MoveDate {get; set;}
    }
    public class ChessMatch
    {
        public List<ChessMove> History {get; set;}
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
    public class ChessChallenge
    {
        [JsonIgnore]
        public IUser Challenger {get; set;}
        [JsonIgnore]
        public IUser Challenged {get; set;}
        public ulong ChallengerId { get { return Challenger.Id; } }
        public ulong ChallengedId { get { return Challenger.Id; } }
        public ulong Channel {get; set;}
        public DateTime ChallengeDate {get; set;}
    }
    public class Position
    {
        public char X {get; set;}
        public char Y {get; set;}
    }
}