using System.Collections.Generic;

namespace TaskReminderApp.Services
{
    public static class KeyboardLayoutConverter
    {
        private static readonly Dictionary<char, char> RusToEng = new Dictionary<char, char>
        {
            {'й', 'q'}, {'ц', 'w'}, {'у', 'e'}, {'к', 'r'}, {'е', 't'},
            {'н', 'y'}, {'г', 'u'}, {'ш', 'i'}, {'щ', 'o'}, {'з', 'p'},
            {'х', '['}, {'ъ', ']'}, {'ф', 'a'}, {'ы', 's'}, {'в', 'd'},
            {'а', 'f'}, {'п', 'g'}, {'р', 'h'}, {'о', 'j'}, {'л', 'k'},
            {'д', 'l'}, {'ж', ';'}, {'э', '\''}, {'я', 'z'}, {'ч', 'x'},
            {'с', 'c'}, {'м', 'v'}, {'и', 'b'}, {'т', 'n'}, {'ь', 'm'},
            {'б', ','}, {'ю', '.'}, {'.', '/'},

            {'Й', 'Q'}, {'Ц', 'W'}, {'У', 'E'}, {'К', 'R'}, {'Е', 'T'},
            {'Н', 'Y'}, {'Г', 'U'}, {'Ш', 'I'}, {'Щ', 'O'}, {'З', 'P'},
            {'Х', '{'}, {'Ъ', '}'}, {'Ф', 'A'}, {'Ы', 'S'}, {'В', 'D'},
            {'А', 'F'}, {'П', 'G'}, {'Р', 'H'}, {'О', 'J'}, {'Л', 'K'},
            {'Д', 'L'}, {'Ж', ':'}, {'Э', '"'}, {'Я', 'Z'}, {'Ч', 'X'},
            {'С', 'C'}, {'М', 'V'}, {'И', 'B'}, {'Т', 'N'}, {'Ь', 'M'},
            {'Б', '<'}, {'Ю', '>'}, {',', '?'},

            {'№', '#'}, {'"', '@'}, {';', '$'}, {'?', '&'}
        };

        private static readonly Dictionary<char, char> EngToRus = new Dictionary<char, char>
        {
            {'q', 'й'}, {'w', 'ц'}, {'e', 'у'}, {'r', 'к'}, {'t', 'е'},
            {'y', 'н'}, {'u', 'г'}, {'i', 'ш'}, {'o', 'щ'}, {'p', 'з'},
            {'[', 'х'}, {']', 'ъ'}, {'a', 'ф'}, {'s', 'ы'}, {'d', 'в'},
            {'f', 'а'}, {'g', 'п'}, {'h', 'р'}, {'j', 'о'}, {'k', 'л'},
            {'l', 'д'}, {';', 'ж'}, {'\'', 'э'}, {'z', 'я'}, {'x', 'ч'},
            {'c', 'с'}, {'v', 'м'}, {'b', 'и'}, {'n', 'т'}, {'m', 'ь'},
            {',', 'б'}, {'.', 'ю'}, {'/', '.'},

            {'Q', 'Й'}, {'W', 'Ц'}, {'E', 'У'}, {'R', 'К'}, {'T', 'Е'},
            {'Y', 'Н'}, {'U', 'Г'}, {'I', 'Ш'}, {'O', 'Щ'}, {'P', 'З'},
            {'{', 'Х'}, {'}', 'Ъ'}, {'A', 'Ф'}, {'S', 'Ы'}, {'D', 'В'},
            {'F', 'А'}, {'G', 'П'}, {'H', 'Р'}, {'J', 'О'}, {'K', 'Л'},
            {'L', 'Д'}, {':', 'Ж'}, {'"', 'Э'}, {'Z', 'Я'}, {'X', 'Ч'},
            {'C', 'С'}, {'V', 'М'}, {'B', 'И'}, {'N', 'Т'}, {'M', 'Ь'},
            {'<', 'Б'}, {'>', 'Ю'}, {'?', ','},

            {'#', '№'}, {'@', '"'}, {'$', ';'}, {'&', '?'}
        };

        public static string Convert(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            bool hasRussian = ContainsRussian(text);
            bool hasEnglish = ContainsEnglish(text);

            if (hasRussian && !hasEnglish)
            {
                return ConvertRusToEng(text);
            }
            else if (hasEnglish && !hasRussian)
            {
                return ConvertEngToRus(text);
            }
            else if (hasRussian && hasEnglish)
            {
                var rusCount = CountRussian(text);
                var engCount = CountEnglish(text);

                if (rusCount > engCount)
                    return ConvertEngToRus(text);
                else
                    return ConvertRusToEng(text);
            }

            return text;
        }

        private static bool ContainsRussian(string text)
        {
            foreach (char c in text)
            {
                if (c >= 'а' && c <= 'я' || c >= 'А' && c <= 'Я' || c == 'ё' || c == 'Ё')
                    return true;
            }
            return false;
        }

        private static bool ContainsEnglish(string text)
        {
            foreach (char c in text)
            {
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                    return true;
            }
            return false;
        }

        private static int CountRussian(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c >= 'а' && c <= 'я' || c >= 'А' && c <= 'Я' || c == 'ё' || c == 'Ё')
                    count++;
            }
            return count;
        }

        private static int CountEnglish(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                    count++;
            }
            return count;
        }

        private static string ConvertRusToEng(string text)
        {
            char[] result = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                if (RusToEng.TryGetValue(text[i], out char converted))
                    result[i] = converted;
                else
                    result[i] = text[i];
            }
            return new string(result);
        }

        private static string ConvertEngToRus(string text)
        {
            char[] result = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                if (EngToRus.TryGetValue(text[i], out char converted))
                    result[i] = converted;
                else
                    result[i] = text[i];
            }
            return new string(result);
        }
    }
}
