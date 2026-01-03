using System;
using System.Collections.Generic;

namespace MediciGet
{
    public static class Extensions
    {
        private static Random rng = new Random();

        // Метод расширения для перемешивания списка (Фишера-Йейтса)
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Метод расширения для множественного перемешивания списка
        public static void ShuffleMultiple<T>(this IList<T> list, int times)
        {
            for (int i = 0; i < times; i++)
            {
                list.Shuffle();
            }
        }
    }
}
