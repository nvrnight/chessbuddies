using System;
using System.Collections.Generic;
using ChessDotNet;
using Discord;
using Newtonsoft.Json;

namespace ChessBuddies.Chess.Models
{
    public class ChessMatch
    {
        public DateTime CreatedDate {get;set;} = DateTime.UtcNow;
        public Guid Id {get; set;} = Guid.NewGuid();
        public List<ChessMove> History {get; set;} = new List<ChessMove>();
        public UndoRequest UndoRequest {get; set;}
        [JsonIgnore]
        public ChessGame Game {get; set;}
        public ulong Challenger {get; set;}
        public ulong Challenged {get; set;}
        [JsonIgnore]
        public ulong[] Players { get { return new[] { Challenger, Challenged }; } }
        public ulong Channel {get; set;}
    }
}