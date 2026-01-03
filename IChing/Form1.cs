using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IChingWindowsForms
{
    public partial class Form1 : Form
    {
        private Random _random;

        // Таблица соответствия между бинарным индексом (0..63) и номером гексаграммы по порядку Кинг Вэнь.
        // Например, если все линии инь (индекс 0), то KingWenOrder[0] = 2 (гексаграмма 坤),
        // а если все линии ян (индекс 63), то KingWenOrder[63] = 1 (гексаграмма 乾).
        private static readonly int[] KingWenOrder = new int[64]
        {
            2, 24, 7, 19, 15, 36, 46, 11,
            16, 51, 40, 54, 62, 55, 32, 34,
            8, 3, 29, 60, 39, 63, 48, 5,
            45, 17, 47, 58, 31, 49, 28, 43,
            23, 27, 4, 41, 52, 22, 18, 26,
            35, 21, 64, 38, 56, 30, 50, 14,
            20, 59, 6, 61, 53, 37, 57, 9,
            12, 25, 44, 13, 10, 42, 33, 1
        };

        // Массив описаний гексаграмм.
        private static readonly HexInfo[] AllHexagrams = new HexInfo[64]
        {
            new HexInfo { Number = 1, Title = "1. 乾 (Qián), Творчество", Description = "Символ творящей силы. Успех достигается настойчивостью и самосовершенствованием." },
            new HexInfo { Number = 2, Title = "2. 坤 (Kūn), Исполнение", Description = "Символ земли и восприимчивости. Мягкость и поддержка ведут к изобилию и гармонии." },
            new HexInfo { Number = 3, Title = "3. 屯 (Zhūn), Начальная трудность", Description = "Начальные усилия в сложных условиях. Требуют терпения и внешней поддержки." },
            new HexInfo { Number = 4, Title = "4. 蒙 (Méng), Недоразвитость", Description = "Юность и незрелость. Нужны наставления и учёба, чтобы избежать ошибок." },
            new HexInfo { Number = 5, Title = "5. 需 (Xū), Ожидание", Description = "Терпеливое ожидание подходящего момента. Спокойная подготовка к действию." },
            new HexInfo { Number = 6, Title = "6. 訟 (Sòng), Тяжба", Description = "Спор и разногласия. Следует найти справедливое решение и избегать пустой конфронтации." },
            new HexInfo { Number = 7, Title = "7. 師 (Shī), Войско", Description = "Организованная коллективная сила. Дисциплина и ясная цель ведут к успеху." },
            new HexInfo { Number = 8, Title = "8. 比 (Bǐ), Приближение", Description = "Единство и доверие. Способствует сплочению и общим целям." },
            new HexInfo { Number = 9, Title = "9. 小畜 (Xiǎo Chù), Малая сила", Description = "Мягкое, постепенное влияние. Требует скромности и упорства." },
            new HexInfo { Number = 10, Title = "10. 履 (Lǚ), Наступление", Description = "Осторожный шаг вперёд по тонкой грани. Нужно удерживать равновесие." },
            new HexInfo { Number = 11, Title = "11. 泰 (Tài), Расцвет", Description = "Гармония и процветание. Силы неба и земли соединяются." },
            new HexInfo { Number = 12, Title = "12. 否 (Pǐ), Упадок", Description = "Противоположность расцвету: застой. Следует укрепить внутренний стержень." },
            new HexInfo { Number = 13, Title = "13. 同人 (Tóng Rén), Единомышленники", Description = "Объединение вокруг общей цели, приводит к успеху через взаимопонимание." },
            new HexInfo { Number = 14, Title = "14. 大有 (Dà Yǒu), Обладание великим", Description = "Большие ресурсы и влияние. Нужно использовать их мудро и скромно." },
            new HexInfo { Number = 15, Title = "15. 謙 (Qiān), Смирение", Description = "Сила скромности и сдержанности. Признак истинной внутренней силы." },
            new HexInfo { Number = 16, Title = "16. 豫 (Yù), Вольность", Description = "Воодушевление и радостный подъём. Стоит использовать энергию на благое дело." },
            new HexInfo { Number = 17, Title = "17. 隨 (Suí), Следование", Description = "Приспособление к переменам, не теряя собственных корней." },
            new HexInfo { Number = 18, Title = "18. 蠱 (Gǔ), Исправление порчи", Description = "Устранение старых ошибок ради обновления и роста." },
            new HexInfo { Number = 19, Title = "19. 臨 (Lín), Посещение", Description = "Возрастание влияния, покровительство. Будьте справедливы и снисходительны." },
            new HexInfo { Number = 20, Title = "20. 観 (Guān), Созерцание", Description = "Вглядеться в суть, понять причины. Спокойное наблюдение рождает мудрость." },
            new HexInfo { Number = 21, Title = "21. 噬嗑 (Shì Kè), Стиснутые зубы", Description = "Решительное устранение препятствий и несправедливости." },
            new HexInfo { Number = 22, Title = "22. 賁 (Bì), Убранство", Description = "Внешняя красота. Однако внутренняя суть важнее поверхностного блеска." },
            new HexInfo { Number = 23, Title = "23. 剥 (Bō), Бо", Description = "Постепенное «слущивание» старого. В этой гексаграмме верхняя линия сплошная, а остальные – разрывные." },
            new HexInfo { Number = 24, Title = "24. 复 (Fù), Возврат", Description = "Возвращение к истокам. Новый цикл, время пересмотреть пройденный путь." },
            new HexInfo { Number = 25, Title = "25. 无妄 (Wú Wàng), Непорочность", Description = "Чистые мотивы без корысти. Искренность и прямота приносят удачу." },
            new HexInfo { Number = 26, Title = "26. 大畜 (Dà Chù), Воспитание великим", Description = "Накопление силы и мудрости. Самодисциплина перед рывком." },
            new HexInfo { Number = 27, Title = "27. 頤 (Yí), Питание", Description = "Забота о пище тела и духа. Следите за тем, что потребляете и чем делитесь." },
            new HexInfo { Number = 28, Title = "28. 大過 (Dà Guò), Переразвитие великого", Description = "Чрезмерная нагрузка грозит поломкой. Укрепите слабые места и ослабьте избыточное." },
            new HexInfo { Number = 29, Title = "29. 坎 (Kǎn), Бездна", Description = "Повторная опасность. Сохраняйте стойкость и веру в правильность пути." },
            new HexInfo { Number = 30, Title = "30. 離 (Lí), Сияние", Description = "Яркость и пламя. Огонь требует постоянного питания, разум — осознанности." },
            new HexInfo { Number = 31, Title = "31. 咸 (Xián), Взаимодействие", Description = "Притяжение сил, создающее союз. Важна искренность и взаимное уважение." },
            new HexInfo { Number = 32, Title = "32. 恒 (Héng), Постоянство", Description = "Длительная стабильность. Верность принципам приводит к твёрдому результату." },
            new HexInfo { Number = 33, Title = "33. 遯 (Dùn), Бегство", Description = "Временное отступление, чтобы сохранить силы и дождаться лучшего часа." },
            new HexInfo { Number = 34, Title = "34. 大壯 (Dà Zhuàng), Великая мощь", Description = "Проявление силы и решимости. Действуйте этично и осмотрительно." },
            new HexInfo { Number = 35, Title = "35. 晉 (Jìn), Восход", Description = "Поступательное продвижение, подобно восходу солнца. Используйте момент роста." },
            new HexInfo { Number = 36, Title = "36. 明夷 (Míng Yí), Поражение света", Description = "Свет в тени внешних обстоятельств. Иногда лучше скрывать достоинства." },
            new HexInfo { Number = 37, Title = "37. 家人 (Jiā Rén), Домашние", Description = "Порядок и взаимная поддержка в семье (или коллективе)." },
            new HexInfo { Number = 38, Title = "38. 睽 (Kuí), Разлад", Description = "Расхождение взглядов. Может привести к конфликту или стимулировать поиск нового." },
            new HexInfo { Number = 39, Title = "39. 蹇 (Jiǎn), Препятствие", Description = "Трудности требуют упорства и помощи друзей. Преодолев преграду, обретаете силу." },
            new HexInfo { Number = 40, Title = "40. 解 (Xiè), Разрешение", Description = "Освобождение от уз и проблем. Необходимо решительное действие." },
            new HexInfo { Number = 41, Title = "41. 損 (Sǔn), Убыль", Description = "Добровольное сокращение внешнего ради укрепления внутреннего." },
            new HexInfo { Number = 42, Title = "42. 益 (Yì), Приумножение", Description = "Увеличение потенциала. Делитесь благом — оно приумножается." },
            new HexInfo { Number = 43, Title = "43. 夬 (Guài), Выход", Description = "Решительный прорыв. Чёткое заявление позиции для устранения препятствий." },
            new HexInfo { Number = 44, Title = "44. 姤 (Gòu), Столкновение", Description = "Неожиданная встреча или влияние. Будьте бдительны, чтобы не допустить хаоса." },
            new HexInfo { Number = 45, Title = "45. 萃 (Cuì), Воссоединение", Description = "Сбор людей и ресурсов для общей цели. Искреннее объединение рождает успех." },
            new HexInfo { Number = 46, Title = "46. 升 (Shēng), Подъём", Description = "Постепенное восхождение. Шаг за шагом продвигайтесь к вершине." },
            new HexInfo { Number = 47, Title = "47. 困 (Kùn), Истощение", Description = "Ситуация ограничений и трудностей. Проверьте внутренние ресурсы и не теряйте дух." },
            new HexInfo { Number = 48, Title = "48. 井 (Jǐng), Колодец", Description = "Источник жизни, требующий обновления. Общая опора для многих." },
            new HexInfo { Number = 49, Title = "49. 革 (Gé), Смена", Description = "Революционные перемены. Старое изжило себя, наступает время обновления." },
            new HexInfo { Number = 50, Title = "50. 鼎 (Dǐng), Жертвенник", Description = "Символ котла и преобразования. Высшая трансформация и духовная пища." },
            new HexInfo { Number = 51, Title = "51. 震 (Zhèn), Возбуждение", Description = "Гром, внезапный толчок. Пробуждает и заставляет действовать." },
            new HexInfo { Number = 52, Title = "52. 艮 (Gèn), Сосредоточенность", Description = "Спокойствие горы. Умение остановиться и вглядеться в себя." },
            new HexInfo { Number = 53, Title = "53. 漸 (Jiàn), Постепенность", Description = "Медленный, но верный рост, подобно созреванию плода. Требует времени." },
            new HexInfo { Number = 54, Title = "54. 歸妹 (Guī Mèi), Невеста", Description = "Ситуация, где нужно принимать условия. Помните об этике и гармонии." },
            new HexInfo { Number = 55, Title = "55. 豐 (Fēng), Изобилие", Description = "Пик процветания и яркости. Важно не забывать о мере и сути." },
            new HexInfo { Number = 56, Title = "56. 旅 (Lǚ), Странствие", Description = "Путешествие и жизнь вдали от дома. Сохраняйте осторожность и достоинство." },
            new HexInfo { Number = 57, Title = "57. 巽 (Xùn), Проникновение", Description = "Мягкая сила ветра. Деликатное воздействие способно проникать глубоко." },
            new HexInfo { Number = 58, Title = "58. 兑 (Duì), Радость", Description = "Искренняя открытость и весёлое общение. Главное — не стать поверхностным." },
            new HexInfo { Number = 59, Title = "59. 涣 (Huàn), Рассеяние", Description = "Разуплотнение старых форм. Освобождение от оков и объединение новых сил." },
            new HexInfo { Number = 60, Title = "60. 節 (Jié), Ограничение", Description = "Разумные границы и правила. Помогают сохранить энергию и фокус." },
            new HexInfo { Number = 61, Title = "61. 中孚 (Zhōng Fú), Внутренняя правда", Description = "Искренность сердца создает доверие и гармонию в отношениях." },
            new HexInfo { Number = 62, Title = "62. 小過 (Xiǎo Guò), Переразвитие малого", Description = "Чрезмерное внимание к деталям. Нужна осторожность, чтобы не упустить главное." },
            new HexInfo { Number = 63, Title = "63. 既濟 (Jì Jì), Уже конец", Description = "Все элементы на своих местах. Но требуется поддерживать порядок, чтобы не рухнул." },
            new HexInfo { Number = 64, Title = "64. 未濟 (Wèi Jì), Еще не конец", Description = "Последний шаг до полного завершения. Нужна аккуратность и осознанность." }
        };

        public Form1()
        {
            InitializeComponent();
            _random = new Random();
        }

        // Обработчик нажатия кнопки генерации гексаграмм.
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Генерируем исходную гексаграмму.
            Line[] originalLines = GenerateHexagram();
            int originalNumber = GetKingWenNumber(originalLines);
            HexInfo originalHex = AllHexagrams[originalNumber - 1];

            // Преобразуем старые (изменяющиеся) линии во вторую устойчивую гексаграмму.
            Line[] finalLines = MakeStable(originalLines);
            int finalNumber = GetKingWenNumber(finalLines);
            HexInfo finalHex = AllHexagrams[finalNumber - 1];

            // Формируем текстовое описание гексаграмм.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ИСХОДНАЯ ГЕКСАГРАММА:");
            sb.AppendLine($"King Wen = {originalNumber}");
            sb.AppendLine($"{originalHex.Title}");
            sb.AppendLine();
            sb.AppendLine($"{originalHex.Description}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine("РЕЗУЛЬТИРУЮЩАЯ ГЕКСАГРАММА:");
            sb.AppendLine($"King Wen = {finalNumber}");
            sb.AppendLine($"{finalHex.Title}");
            sb.AppendLine();
            sb.AppendLine($"{finalHex.Description}");

            textBoxResult.Text = sb.ToString();

            // Отрисовка гексаграмм с подписями.
            DrawHexagrams(originalLines, finalLines, originalHex, finalHex);
        }

        // Генерация исходной гексаграммы.
        // Для каждой линии проводится 3 броска (метод монет), сумма может быть 6, 7, 8 или 9.
        // Массив lines: индекс 0 – нижняя линия, индекс 5 – верхняя.
        private Line[] GenerateHexagram()
        {
            Line[] lines = new Line[6];
            for (int i = 0; i < 6; i++)
            {
                int sum = 0;
                for (int j = 0; j < 3; j++)
                {
                    // С вероятностью 50% добавляем 3, иначе 2.
                    sum += _random.Next(2) == 1 ? 3 : 2;
                }

                // Сумма определяет тип линии:
                // 6 – Старая Инь, 7 – Молодая Ян, 8 – Молодая Инь, 9 – Старая Ян.
                switch (sum)
                {
                    case 6:
                        lines[i] = new Line { IsYang = false, IsOld = true, WasOld = true };
                        break;
                    case 7:
                        lines[i] = new Line { IsYang = true, IsOld = false, WasOld = false };
                        break;
                    case 8:
                        lines[i] = new Line { IsYang = false, IsOld = false, WasOld = false };
                        break;
                    case 9:
                        lines[i] = new Line { IsYang = true, IsOld = true, WasOld = true };
                        break;
                }
            }
            return lines;
        }

        // Вычисление номера гексаграммы по бинарному индексу.
        // Массив lines хранит линии так, что lines[0] – нижняя, lines[5] – верхняя.
        // Нижняя линия соответствует младшему биту, а верхняя – старшему.
        private int GetKingWenNumber(Line[] lines)
        {
            int index = 0;
            for (int i = 0; i < 6; i++)
            {
                if (lines[5 - i].IsYang)
                {
                    index |= (1 << i);
                }
            }
            return KingWenOrder[index];
        }

        // Преобразование исходной гексаграммы во вторую устойчивую гексаграмму.
        // Если линия была изменяющейся (IsOld == true), она инвертируется, а значение WasOld сохраняется.
        private Line[] MakeStable(Line[] original)
        {
            Line[] res = new Line[6];
            for (int i = 0; i < 6; i++)
            {
                if (original[i].IsOld)
                {
                    res[i] = new Line
                    {
                        IsYang = !original[i].IsYang,
                        IsOld = false, // После преобразования линия становится устойчивой.
                        WasOld = original[i].WasOld
                    };
                }
                else
                {
                    res[i] = original[i];
                }
            }
            return res;
        }

        // Отрисовка обеих гексаграмм с подписями.
        // Здесь нижние линии (индекс 0) отображаются сверху, а верхние (индекс 5) – снизу.
        // Линии будут отрисованы на 15 пикселей ниже заголовка.
        private void DrawHexagrams(Line[] originalLines, Line[] finalLines, HexInfo originalHex, HexInfo finalHex)
        {
            Bitmap bmp = new Bitmap(pictureBoxHexagrams.Width, pictureBoxHexagrams.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                int startX = 50;
                int titleY = 25;           // Вертикальная координата для заголовка.
                int lineStartY = titleY + 30; // Линии начинаются на 15 пикселей ниже заголовка.
                int spacing = 30;           // Интервал между линиями.
                int offsetX = 300;

                // Отрисовка исходной гексаграммы (от нижней линии к верхней).
                // Здесь lines[0] (нижняя) рисуется первой (оказывается сверху).
                g.DrawString("ИСХОДНАЯ: " + originalHex.Title, new Font("Arial", 10, FontStyle.Bold), Brushes.Black, startX, titleY - 20);
                for (int i = 0; i < 6; i++)
                {
                    DrawLine(g, startX, lineStartY + spacing * i, originalLines[i]);
                }

                // Отрисовка результирующей гексаграммы.
                g.DrawString("РЕЗУЛЬТИРУЮЩАЯ: " + finalHex.Title, new Font("Arial", 10, FontStyle.Bold), Brushes.Black, startX + offsetX, titleY - 20);
                for (int i = 0; i < 6; i++)
                {
                    DrawLine(g, startX + offsetX, lineStartY + spacing * i, finalLines[i]);
                }
            }
            pictureBoxHexagrams.Image = bmp;
        }

        // Отрисовка отдельной линии с символом и подробным описанием.
        // Для второй гексаграммы используется сохранённое свойство WasOld для отражения исходного статуса линии.
        private void DrawLine(Graphics g, int x, int y, Line line)
        {
            Pen pen = new Pen(Color.Black, 4);
            if (line.IsYang)
            {
                // Для Ян – сплошная линия.
                g.DrawLine(pen, x, y, x + 100, y);
            }
            else
            {
                // Для Инь – линия с разрывом.
                g.DrawLine(pen, x, y, x + 40, y);
                g.DrawLine(pen, x + 60, y, x + 100, y);
            }

            // Определяем символ линии в зависимости от её типа и изменяемости.
            string symbol = line.IsYang ? (line.WasOld ? "⚊→⚋" : "⚊") : (line.WasOld ? "⚋→⚊" : "⚋");
            string description = line.IsYang ? (line.WasOld ? "Старая Ян" : "Молодая Ян") : (line.WasOld ? "Старая Инь" : "Молодая Инь");

            string displayText = $"{symbol} ({description})";
            g.DrawString(displayText, new Font("Arial", 12), Brushes.Blue, x + 105, y - 10);
        }
    }

    // Структура для хранения информации о линии гексаграммы.
    public struct Line
    {
        public bool IsYang;  // Тип линии после преобразования (устойчивая).
        public bool IsOld;   // Текущий флаг изменяемости (false для устойчивых линий).
        public bool WasOld;  // Сохраняет информацию: была ли линия изменяющейся изначально.
    }

    // Класс для описания гексаграммы.
    public class HexInfo
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
