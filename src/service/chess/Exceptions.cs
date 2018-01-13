using System;

namespace src
{
    public class ChessException : Exception
    {
        public ChessException(string message) : base(message){}
    }
}