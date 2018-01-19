using System;
using Discord;
using Newtonsoft.Json;

namespace ChessBuddies.Chess.Models
{
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
}