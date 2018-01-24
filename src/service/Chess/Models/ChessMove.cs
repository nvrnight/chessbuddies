using System;
using ChessDotNet;
using Newtonsoft.Json;

namespace ChessBuddies.Chess.Models
{
    public class ChessMove
    {
        [JsonIgnore]
        public Piece[][] PreviousBoardState {get; set;} = new Piece[8][] { new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8], new Piece[8]};
        public File OriginalFile {get; set;}
        public int OriginalRank {get; set;}
        public File NewFile {get; set;}
        public int NewRank {get; set;}
        public DateTime MoveDate {get; set;}
    }
}