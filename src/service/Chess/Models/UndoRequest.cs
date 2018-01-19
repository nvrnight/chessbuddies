using System;
using Discord;

namespace ChessBuddies.Chess.Models
{
    public class UndoRequest
    {
        public DateTime CreatedDate {get; set;}
        public IUser CreatedBy {get; set;}
    }
}