using System;

namespace ChessBuddies.Chess.Models
{
    public class MatchStatus
    {
        public Guid Id {get; set;}
        public DateTime? LastMoveDate {get; set;}
    }
    public class ChessServiceStatus
    {
        public MatchStatus[] MatchesInProgress {get; set;}
        public int ChallengeRequestsInProgress {get; set;}
    }
}