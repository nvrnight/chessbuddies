using Discord;

namespace ChessBuddies.Chess.Models
{
    public class ChessMatchStatus
    {
        public bool IsOver {get; set;}
        public bool IsCheck {get; set;}
        public ulong? Winner {get; set;}
    }
}