using System;

namespace ChessBuddies.Chess.Exceptions
{
    public class ChessException : Exception
    {
        public ChessException(string message) : base(message){}
    }
}