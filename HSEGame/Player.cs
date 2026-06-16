
namespace HSEGame
{
    public class Player
    {
        public int BestScore
        {
            get; set;
        }
        public int TotalPoints
        {
            get; set;
        }

        public int Lifes
        {
            get; set;
        }

        public Player()
        {
            Lifes = 3;
        }
    }
}
