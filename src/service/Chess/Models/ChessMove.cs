using System;
using ChessDotNet;

namespace ChessBuddies.Chess.Models
{
    public class ChessMove
    {
        public Piece[][] PreviousBoardState {get; set;} = new Piece[8][] { new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8]};
        public Move Move {get; set;}
        public DateTime MoveDate {get; set;}
    }
}