namespace MediciGet
{
    public class Card
    {
        public int Rank { get; set; } // 6..14 (6-10, В - 11, Д - 12, К - 13, Т - 14)
        public char Suit { get; set; } // 'ч','б','п','к'

        // Конструктор с параметрами
        public Card(int rank, char suit)
        {
            Rank = rank;
            Suit = suit;
        }

        // Конструктор без параметров
        public Card()
        {
            Rank = 0;
            Suit = '?';
        }

        public override string ToString()
        {
            string rankStr = "";
            switch (Rank)
            {
                case 6: rankStr = "6"; break;
                case 7: rankStr = "7"; break;
                case 8: rankStr = "8"; break;
                case 9: rankStr = "9"; break;
                case 10: rankStr = "10"; break;
                case 11: rankStr = "В"; break; // Валет
                case 12: rankStr = "Д"; break; // Дама
                case 13: rankStr = "К"; break; // Король
                case 14: rankStr = "Т"; break; // Туз
                default: rankStr = "?"; break;
            }
            return $"{rankStr}{Suit}";
        }
    }
}
