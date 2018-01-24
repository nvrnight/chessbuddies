using System;
using Discord;
using Newtonsoft.Json;

namespace ChessBuddies.Chess.Models
{
    public class ChessChallenge
    {
        public ulong Challenger {get; set;}
        public ulong Challenged {get; set;}
        public ulong Channel {get; set;}
        public DateTime ChallengeDate {get; set;}
    }
}