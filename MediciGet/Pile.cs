using System.Collections.Generic;
using System.Linq;

namespace MediciGet
{
    public class Pile
    {
        public List<Card> Stack { get; set; }

        public Pile()
        {
            Stack = new List<Card>();
        }

        public Card Top()
        {
            if (Stack != null && Stack.Count > 0)
                return Stack.Last();
            return new Card(); // Возвращаем карту с параметрами по умолчанию
        }
    }
}
