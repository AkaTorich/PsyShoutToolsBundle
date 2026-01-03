using System.Collections.Generic;

namespace MediciGet
{
    public class CardComparer : IEqualityComparer<Card>
    {
        public bool Equals(Card x, Card y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.Rank == y.Rank && x.Suit == y.Suit;
        }

        public int GetHashCode(Card obj)
        {
            if (obj is null)
                return 0;

            return obj.Rank.GetHashCode() ^ obj.Suit.GetHashCode();
        }
    }
}
