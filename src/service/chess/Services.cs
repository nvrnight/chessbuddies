using System.Text.RegularExpressions;

namespace src
{
    public interface IChessService
    {
        ChessMove Move(string rawMove);
    }
    public class ChessService : IChessService
    {
        public ChessMove Move(string rawMove)
        {
            var move = rawMove.Replace(" ", "");

            if(!Regex.IsMatch(move, "[a-h][1-8][a-h][1-8]"))
                throw new ChessMoveParseException();

            var sourceX = move[0];
            var sourceY = move[1];
            var destX = move[2];
            var destY = move[3];

            return new ChessMove { Source = new Position { X = sourceX, Y = sourceY }, Destination = new Position { X = destX, Y = destY } };
        }
    }    
}