namespace src
{    
    public class Position
    {
        public char X {get; set;}
        public char Y {get; set;}
    }
    public class ChessMove
    {
        public Position Source {get; set;}
        public Position Destination {get; set;}
    }
}