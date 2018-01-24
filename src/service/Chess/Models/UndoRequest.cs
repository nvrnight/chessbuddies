using System;
using Discord;

namespace ChessBuddies.Chess.Models
{
    public class UndoRequest
    {
        public DateTime CreatedDate {get; set;}
        public ulong CreatedBy {get; set;}
    }
}