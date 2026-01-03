using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediciGet
{
    public partial class Form1 : Form
    {
        private readonly List<Card> allCards = new List<Card>();
        private readonly Dictionary<string, CardDescription> cardDescriptions = new Dictionary<string, CardDescription>();
        private CancellationTokenSource cts;
        private int attemptCount = 0; // Счётчик попыток

        public Form1()
        {
            InitializeComponent();
            InitializeDeck();
            InitializeCardDescriptions();
            InitializeCardButtons();
        }

        // Инициализация полной колоды
        private void InitializeDeck()
        {
            char[] suits = { 'ч', 'б', 'п', 'к' };
            for (int rank = 6; rank <= 14; rank++)
            {
                foreach (var suit in suits)
                {
                    allCards.Add(new Card(rank, suit));
                }
            }
        }

        // Инициализация описаний карт
        private void InitializeCardDescriptions()
        {
            cardDescriptions.Add("6ч", new CardDescription("6 черви — перемещение. Идешь утром на работу."));
            cardDescriptions.Add("6б", new CardDescription("6 бубны — перемещение. Идешь в магазин."));
            cardDescriptions.Add("6п", new CardDescription("6 пики — перемещение. Идешь на свидание."));
            cardDescriptions.Add("6к", new CardDescription("6 крести — перемещение. Идешь выяснять отношения."));

            cardDescriptions.Add("7ч", new CardDescription("7 черви — поглощение. Сел завтракать."));
            cardDescriptions.Add("7б", new CardDescription("7 бубны — поглощение. Пересчитал деньги в кошельке."));
            cardDescriptions.Add("7п", new CardDescription("7 пики — поглощение. Узнал что-то неприятное."));
            cardDescriptions.Add("7к", new CardDescription("7 крести — поглощение. Взял книгу с полки."));

            cardDescriptions.Add("8ч", new CardDescription("8 черви — общение. Болтаешь с приятным человеком."));
            cardDescriptions.Add("8б", new CardDescription("8 бубны — общение. Общаешься с продавцом в магазине."));
            cardDescriptions.Add("8п", new CardDescription("8 пики — общение. Причесал волосы."));
            cardDescriptions.Add("8к", new CardDescription("8 крести — общение. Обсуждаешь деловые проблемы."));

            cardDescriptions.Add("9ч", new CardDescription("9 черви — действие. Дал кому-то в глаз."));
            cardDescriptions.Add("9б", new CardDescription("9 бубны — действие. Купил что-то."));
            cardDescriptions.Add("9п", new CardDescription("9 пики — действие. Поправил воротник."));
            cardDescriptions.Add("9к", new CardDescription("9 крести — действие. Причесал волосы."));

            cardDescriptions.Add("10ч", new CardDescription("10 черви — результат. Рад, что все завершилось."));
            cardDescriptions.Add("10б", new CardDescription("10 бубны — результат. Результат связан с деньгами."));
            cardDescriptions.Add("10п", new CardDescription("10 пики — результат. Завершение работы."));
            cardDescriptions.Add("10к", new CardDescription("10 крести — результат. Завершение работы."));

            cardDescriptions.Add("Вч", new CardDescription("Валет черви — курьер силы. Хочешь заработать деньги."));
            cardDescriptions.Add("Вб", new CardDescription("Валет бубны — курьер силы. Хочешь познакомиться."));
            cardDescriptions.Add("Вп", new CardDescription("Валет пики — курьер силы. Хочешь выполнить работу."));
            cardDescriptions.Add("Вк", new CardDescription("Валет крести — курьер силы. Хочешь улучшить навыки."));

            cardDescriptions.Add("Дч", new CardDescription("Дама черви — персона. Встретили друга."));
            cardDescriptions.Add("Дб", new CardDescription("Дама бубны — персона. Облаяла собака."));
            cardDescriptions.Add("Дп", new CardDescription("Дама пики — персона. Кассирша."));
            cardDescriptions.Add("Дк", new CardDescription("Дама крести — персона. Водитель автобуса."));

            cardDescriptions.Add("Кч", new CardDescription("Король черви — закон. Остановился у перехода."));
            cardDescriptions.Add("Кб", new CardDescription("Король бубны — закон. Нужно заплатить."));
            cardDescriptions.Add("Кп", new CardDescription("Король пики — закон. Подарить подарок."));
            cardDescriptions.Add("Кк", new CardDescription("Король крести — закон. Необходимо пройти."));

            cardDescriptions.Add("Тч", new CardDescription("Туз черви — сила. Любое внешнее событие."));
            cardDescriptions.Add("Тб", new CardDescription("Туз бубны — сила. Вызов начальника."));
            cardDescriptions.Add("Тп", new CardDescription("Туз пики — сила. Выигрыш в лотерею."));
            cardDescriptions.Add("Тк", new CardDescription("Туз крести — сила. Подметающая машина."));
        }

        // Создание кнопок для выбора карт
        private void InitializeCardButtons()
        {
            int x = 10, y = 10;
            int bw = 60, bh = 30;
            int spacing = 5;
            int maxWidth = panelCards.Width - 20;

            foreach (var card in allCards)
            {
                Button btn = new Button
                {
                    Text = card.ToString(),
                    Width = bw,
                    Height = bh,
                    Left = x,
                    Top = y,
                    Tag = card
                };
                btn.Click += CardButton_Click;
                panelCards.Controls.Add(btn);

                x += bw + spacing;
                if (x + bw > maxWidth)
                {
                    x = 10;
                    y += bh + spacing;
                }
            }
        }

        private void CardButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Card selectedCard)
            {
                if (listBoxFinalStack.Items.Contains(selectedCard))
                {
                    MessageBox.Show("Эта карта уже добавлена в конечную стопку!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (listBoxFinalStack.Items.Count >= 36)
                {
                    MessageBox.Show("Нельзя добавить более 36 карт в конечную стопку!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                listBoxFinalStack.Items.Add(selectedCard);
            }
        }

        private void ButtonFindChain_Click(object sender, EventArgs e)
        {
            List<Card> finalStack = new List<Card>();
            foreach (var item in listBoxFinalStack.Items)
            {
                if (item is Card c)
                {
                    finalStack.Add(c);
                }
            }

            if (finalStack.Count == 0)
            {
                MessageBox.Show("Не выбраны карты для финальной стопки!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Task.Run(() =>
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());

                while (!token.IsCancellationRequested)
                {
                    // Перемешиваем колоду
                    List<Card> shuffledDeck = allCards.OrderBy(x => random.Next()).ToList();

                    // Убираем выбранные карты из перемешанной колоды
                    List<Card> remainingDeck = shuffledDeck.Except(finalStack).ToList();

                    // Строим цепочку в обратном порядке
                    List<Card> chain = BuildChain(remainingDeck, finalStack);

                    attemptCount++;
                    UpdateAttemptCounter();

                    // Проверяем, что цепочка завершена
                    if (chain != null && chain.Count == 36)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show("Цепочка найдена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DisplayChain(chain);
                        }));
                        return;
                    }
                }
            }, token);
        }

        private List<Card> BuildChain(List<Card> remainingDeck, List<Card> finalStack)
        {
            List<Card> chain = new List<Card>(finalStack);

            while (remainingDeck.Count > 0)
            {
                Card nextCard = remainingDeck.FirstOrDefault(card => CanMerge(card, chain.First()));
                if (nextCard != null)
                {
                    chain.Insert(0, nextCard);
                    remainingDeck.Remove(nextCard);
                }
                else
                {
                    return null;
                }
            }

            return chain;
        }

        private void DisplayChain(List<Card> chain)
        {
            string chainDescription = "Найденная цепочка:\r\n";
            foreach (var card in chain)
            {
                string key = card.ToString();
                if (cardDescriptions.ContainsKey(key))
                {
                    chainDescription += $"{key}: {cardDescriptions[key].Description}\r\n";
                }
                else
                {
                    chainDescription += $"{key}: Описание не найдено.\r\n";
                }
            }

            // Добавляем найденную цепочку в лог
            AppendLog(chainDescription);
            MessageBox.Show(chainDescription, "Цепочка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool CanMerge(Card card1, Card card2)
        {
            return card1.Suit == card2.Suit || card1.Rank == card2.Rank;
        }

        private void UpdateAttemptCounter()
        {
            if (labelAttemptCounter.InvokeRequired)
            {
                labelAttemptCounter.Invoke(new Action(UpdateAttemptCounter));
            }
            else
            {
                labelAttemptCounter.Text = $"Количество попыток: {attemptCount}";
            }
        }

        // Метод для добавления текста в лог
        private void AppendLog(string message)
        {
            if (textBoxLog.InvokeRequired)
            {
                textBoxLog.Invoke(new Action(() => textBoxLog.AppendText(message + "\r\n")));
            }
            else
            {
                textBoxLog.AppendText(message + "\r\n");
            }
        }
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            listBoxFinalStack.Items.Clear();
            attemptCount = 0;
            UpdateAttemptCounter();
            cts?.Cancel();
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
