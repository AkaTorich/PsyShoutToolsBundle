using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BASSGenerator
{
    [DesignerCategory("Code")]
    public class NoteDisplayControl : Control
    {
        // Структура для хранения информации о ноте с ее позицией и длительностью
        private class NoteInfo
        {
            public int MidiNote { get; set; }     // MIDI номер ноты
            public int Position { get; set; }     // Позиция в последовательности (начальная позиция)
            public int Duration { get; set; }     // Длительность ноты (в единицах 1/16)

            public NoteInfo(int midiNote, int position, int duration = 1)
            {
                MidiNote = midiNote;
                Position = position;
                Duration = duration > 0 ? duration : 1; // Минимальная длительность - 1/16
            }
        }

        // Список нот с позициями и длительностями
        private List<NoteInfo> _notes = new List<NoteInfo>();

        // Режим отображения: 0 = Клавиатура, 1 = Нотный стан
        private int _displayMode = 0;

        // Константы для рисования - уменьшаем высоту строки до минимума
        private const int ROW_HEIGHT = 8;  // Уменьшена до 8 для максимально компактного отображения
        private const int NOTE_WIDTH = 30;  // Ширина блока ноты
        private const int KEY_WIDTH = 35;   // Немного увеличена ширина клавиш для лучшей читаемости
        private const int HEADER_HEIGHT = 14; // Уменьшена для экономии места

        // Константы для нотного стана
        private const int STAFF_LINE_SPACING = 10;
        private const int NOTE_RADIUS = 6;

        // Константы для размера контента и прокрутки
        private const int MAX_HORIZONTAL_MEASURES = 100;  // Максимальное количество тактов по горизонтали
        private const int NOTES_PER_MEASURE = 16;         // Количество нот в одном такте

        // Диапазон MIDI-нот
        private const int MIDI_NOTE_MIN = 0;    // C-1 (самая низкая возможная нота)
        private const int MIDI_NOTE_MAX = 127;  // G9 (самая высокая возможная нота)

        // Настраиваем диапазон видимых по умолчанию нот
        private int _initialVisibleNote = 36;  // C2 - разумная начальная нота для отображения

        // Полосы прокрутки
        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;

        // Текущие позиции прокрутки
        private int scrollX = 0;
        private int scrollY = 0;

        // Размеры виртуального контента
        private int virtualWidth;
        private int virtualHeight;

        // Цвет нот - изменен на DarkViolet
        private readonly Color _noteColor = Color.DarkViolet;
        private readonly Color _textColor = Color.White;

        // Цвета для более компактного отображения
        private readonly Color _whiteKeyColor = Color.FromArgb(240, 240, 240);
        private readonly Color _blackKeyColor = Color.FromArgb(40, 40, 40);
        private readonly Color _whiteRowColor = Color.FromArgb(225, 225, 225);
        private readonly Color _blackRowColor = Color.FromArgb(70, 70, 70);
        private readonly Color _cNoteRowColor = Color.FromArgb(235, 235, 255); // Светло-голубой фон для нот C
        private readonly Color _whiteGridColor = Color.FromArgb(220, 220, 220); // Белая сетка
        private readonly Color _sixteenthGridColor = Color.FromArgb(180, 180, 180); // Цвет для 16-х долей

        // Конструктор
        public NoteDisplayControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.BackColor = Color.FromArgb(30, 30, 30); // Темный фон для контраста с белой сеткой

            // Создаем и настраиваем вертикальную полосу прокрутки
            vScrollBar = new VScrollBar();
            vScrollBar.Dock = DockStyle.Right;
            vScrollBar.ValueChanged += ScrollBar_ValueChanged;
            this.Controls.Add(vScrollBar);

            // Создаем и настраиваем горизонтальную полосу прокрутки
            hScrollBar = new HScrollBar();
            hScrollBar.Dock = DockStyle.Bottom;
            hScrollBar.ValueChanged += ScrollBar_ValueChanged;
            this.Controls.Add(hScrollBar);

            // Устанавливаем начальные размеры виртуального холста
            UpdateScrollBars();

            // Подписываемся на событие колеса мыши
            this.MouseWheel += NoteDisplayControl_MouseWheel;
        }

        private void NoteDisplayControl_MouseWheel(object sender, MouseEventArgs e)
        {
            // Обрабатываем событие колеса мыши для прокрутки
            int delta = e.Delta > 0 ? -3 : 3; // Меняем направление и скорость прокрутки

            // Если зажат Shift, прокручиваем по горизонтали, иначе по вертикали
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                scrollX = Math.Max(0, Math.Min(scrollX + delta * NOTE_WIDTH,
                                              virtualWidth - this.ClientSize.Width + vScrollBar.Width));
                hScrollBar.Value = Math.Min(hScrollBar.Maximum, Math.Max(hScrollBar.Minimum, scrollX));
            }
            else
            {
                scrollY = Math.Max(0, Math.Min(scrollY + delta * ROW_HEIGHT * 2, // Увеличиваем скорость для очень тонких строк
                                              virtualHeight - this.ClientSize.Height + hScrollBar.Height));
                vScrollBar.Value = Math.Min(vScrollBar.Maximum, Math.Max(vScrollBar.Minimum, scrollY));
            }

            this.Invalidate();
        }

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (sender == vScrollBar)
            {
                scrollY = vScrollBar.Value;
            }
            else if (sender == hScrollBar)
            {
                scrollX = hScrollBar.Value;
            }

            this.Invalidate();
        }

        // Метод для обновления полос прокрутки
        private void UpdateScrollBars()
        {
            if (_displayMode == 0)
            {
                // Для пиано-ролла: рассчитываем полную высоту для всех нот
                virtualHeight = (MIDI_NOTE_MAX - MIDI_NOTE_MIN + 1) * ROW_HEIGHT;

                // Вычисляем ширину для всех тактов
                virtualWidth = KEY_WIDTH + MAX_HORIZONTAL_MEASURES * NOTES_PER_MEASURE * NOTE_WIDTH;
            }
            else
            {
                // Для нотного стана
                virtualHeight = 500; // Фиксированная высота для нотного стана
                virtualWidth = 60 + MAX_HORIZONTAL_MEASURES * NOTES_PER_MEASURE * NOTE_WIDTH;
            }

            // Настраиваем вертикальную полосу прокрутки
            vScrollBar.Minimum = 0;
            vScrollBar.Maximum = Math.Max(0, virtualHeight - (this.ClientSize.Height - hScrollBar.Height)) + vScrollBar.LargeChange;
            vScrollBar.LargeChange = ROW_HEIGHT * 15; // Прокрутка на больше нот за раз
            vScrollBar.SmallChange = ROW_HEIGHT * 3;  // Увеличиваем шаг прокрутки для тонких строк

            // Настраиваем горизонтальную полосу прокрутки
            hScrollBar.Minimum = 0;
            hScrollBar.Maximum = Math.Max(0, virtualWidth - (this.ClientSize.Width - vScrollBar.Width)) + hScrollBar.LargeChange;
            hScrollBar.LargeChange = NOTE_WIDTH * 10; // Прокрутка на 10 нот за раз
            hScrollBar.SmallChange = NOTE_WIDTH;

            // Центрируем на начальной позиции, если это первая настройка
            if (scrollY == 0 && _initialVisibleNote > 0)
            {
                // Инвертируем индекс ноты, чтобы высокие ноты были сверху
                int invertedNoteIndex = MIDI_NOTE_MAX - _initialVisibleNote;
                scrollY = Math.Max(0, invertedNoteIndex * ROW_HEIGHT - (this.Height / 2));
                vScrollBar.Value = Math.Min(vScrollBar.Maximum, Math.Max(vScrollBar.Minimum, scrollY));
            }
        }

        // Новый метод для установки нот с длительностями
        public void SetNotesWithDurations(List<int> notes, List<int> durations = null)
        {
            _notes.Clear();

            // Добавляем ноты с их позициями и длительностями
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i] > 0)
                {
                    int duration = 1; // По умолчанию длительность 1/16
                    if (durations != null && i < durations.Count)
                    {
                        duration = durations[i];
                    }

                    _notes.Add(new NoteInfo(notes[i], i, duration));
                }
            }

            this.Invalidate(); // Форсируем перерисовку
        }

        // Обратная совместимость с существующим методом
        public void SetNotes(List<int> notes)
        {
            SetNotesWithDurations(notes, null);
        }

        // Метод для установки режима отображения
        public void SetDisplayMode(int mode)
        {
            _displayMode = mode;
            UpdateScrollBars(); // Обновляем настройки прокрутки при изменении режима
            this.Invalidate(); // Форсируем перерисовку
        }

        // Метод для прокрутки к определенной ноте
        public void ScrollToNote(int midiNote)
        {
            // Инвертируем индекс ноты, чтобы высокие ноты были сверху
            int invertedNoteIndex = MIDI_NOTE_MAX - midiNote;
            scrollY = Math.Max(0, Math.Min(invertedNoteIndex * ROW_HEIGHT - (this.Height / 2),
                                          virtualHeight - this.ClientSize.Height + hScrollBar.Height));

            // Обновляем полосу прокрутки
            vScrollBar.Value = Math.Min(vScrollBar.Maximum, Math.Max(vScrollBar.Minimum, scrollY));

            this.Invalidate();
        }

        // Метод для прокрутки к определенной позиции (такту)
        public void ScrollToPosition(int position)
        {
            // Вычисляем X-координату для указанной позиции
            scrollX = Math.Max(0, Math.Min(position * NOTE_WIDTH - (this.Width / 2),
                                          virtualWidth - this.ClientSize.Width + vScrollBar.Width));

            // Обновляем полосу прокрутки
            hScrollBar.Value = Math.Min(hScrollBar.Maximum, Math.Max(hScrollBar.Minimum, scrollX));

            this.Invalidate();
        }

        // Переопределение метода OnResize
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Позиционируем угол пересечения полос прокрутки
            vScrollBar.Height = this.ClientSize.Height - hScrollBar.Height;
            hScrollBar.Width = this.ClientSize.Width - vScrollBar.Width;

            // Обновляем размеры и позиции полос прокрутки
            UpdateScrollBars();

            this.Invalidate();
        }

        // Переопределяем метод OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Получаем клиентскую область без полос прокрутки
            Rectangle clientRect = new Rectangle(
                0, 0,
                this.ClientSize.Width - vScrollBar.Width,
                this.ClientSize.Height - hScrollBar.Height
            );

            // Очищаем фон
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, clientRect);
            }

            // Рисуем в зависимости от выбранного режима
            if (_displayMode == 0)
                DrawPianoRoll(e.Graphics, clientRect);
            else
                DrawStaffNotation(e.Graphics, clientRect);
        }

        // Метод для рисования пиано-ролла с пользовательской прокруткой
        private void DrawPianoRoll(Graphics g, Rectangle clientRect)
        {
            // Для сверхтонких строк используем другой подход к рисованию
            // Рассчитываем, сколько нот можно отобразить по вертикали в видимой области
            int visibleNoteHeight = clientRect.Height - HEADER_HEIGHT;
            int visibleNotesCount = (visibleNoteHeight / ROW_HEIGHT) + 2; // +2 для дополнительного буфера

            // Определяем, какие ноты показывать на основе текущей позиции прокрутки
            int firstVisibleRow = scrollY / ROW_HEIGHT;

            // Вычисляем диапазон видимых нот (в обратном порядке: высокие сверху, низкие внизу)
            int highestVisibleNote = MIDI_NOTE_MAX - firstVisibleRow;
            int lowestVisibleNote = Math.Max(MIDI_NOTE_MIN, highestVisibleNote - visibleNotesCount);

            // Определяем диапазон видимых горизонтальных позиций
            int firstVisiblePos = scrollX / NOTE_WIDTH;
            int lastVisiblePos = Math.Min(MAX_HORIZONTAL_MEASURES * NOTES_PER_MEASURE,
                                         (scrollX + clientRect.Width) / NOTE_WIDTH + 2);

            // Вычисляем смещение для более точного позиционирования
            int offsetY = -(scrollY % ROW_HEIGHT);
            int offsetX = -(scrollX);

            // Сначала рисуем сетку и фон всех рядов за один проход для минимизации операций рисования
            using (Pen gridPen = new Pen(Color.FromArgb(60, 60, 60), 1)) // Тонкая серая сетка
            using (Pen whiteGridPen = new Pen(_whiteGridColor, 1)) // Белая сетка для выделения
            using (Pen sixteenthGridPen = new Pen(_sixteenthGridColor, 1)) // Сетка для 16-х
            {
                // Рисуем верхнюю шкалу тактов
                using (SolidBrush headerBrush = new SolidBrush(Color.FromArgb(40, 40, 40))) // Темный заголовок
                {
                    g.FillRectangle(headerBrush, 0, 0, clientRect.Width, HEADER_HEIGHT);

                    // Рисуем разделительную линию под заголовком
                    g.DrawLine(whiteGridPen, 0, HEADER_HEIGHT, clientRect.Width, HEADER_HEIGHT);

                    // Рисуем метки тактов
                    using (Font headerFont = new Font("Arial", 6))
                    using (SolidBrush textBrush = new SolidBrush(Color.White)) // Белый текст для лучшей видимости
                    {
                        // Рисуем только каждый второй такт для экономии места
                        for (int measure = 0; measure < MAX_HORIZONTAL_MEASURES; measure += 2)
                        {
                            int measureX = KEY_WIDTH + measure * NOTES_PER_MEASURE * NOTE_WIDTH + offsetX;

                            if (measureX >= 0 && measureX < clientRect.Width - KEY_WIDTH)
                            {
                                g.DrawString($"T{measure + 1}", headerFont, textBrush,
                                            measureX, (HEADER_HEIGHT - headerFont.Height) / 2);
                            }
                        }
                    }
                }

                int yPosition = HEADER_HEIGHT;

                // Проходим по нотам в порядке от высоких к низким
                for (int i = 0; i < visibleNotesCount && highestVisibleNote - i >= lowestVisibleNote; i++)
                {
                    int midiNote = highestVisibleNote - i;

                    // Проверяем валидность ноты
                    if (midiNote < MIDI_NOTE_MIN || midiNote > MIDI_NOTE_MAX)
                        continue;

                    // Определяем, является ли нота До (C)
                    bool isCNote = midiNote % 12 == 0;

                    // Определяем цвет строки на основе типа клавиши
                    bool isBlackKey = IsBlackKey(midiNote);
                    Color rowColor;

                    if (isCNote)
                    {
                        rowColor = _cNoteRowColor; // Специальный цвет для нот До (C)
                    }
                    else
                    {
                        rowColor = isBlackKey ? _blackRowColor : _whiteRowColor;
                    }

                    using (SolidBrush rowBrush = new SolidBrush(rowColor))
                    {
                        // Рисуем фон строки для всей видимой области
                        g.FillRectangle(rowBrush, KEY_WIDTH, yPosition, clientRect.Width - KEY_WIDTH, ROW_HEIGHT);
                    }

                    // Рисуем клавишу слева
                    using (SolidBrush keyBrush = new SolidBrush(isBlackKey ? _blackKeyColor : _whiteKeyColor))
                    {
                        g.FillRectangle(keyBrush, 0, yPosition, KEY_WIDTH, ROW_HEIGHT);

                        // Более тонкая рамка для клавиш
                        using (Pen keyBorderPen = new Pen(Color.FromArgb(120, 120, 120), 1))
                        {
                            g.DrawLine(keyBorderPen, 0, yPosition, KEY_WIDTH, yPosition);
                            g.DrawLine(keyBorderPen, 0, yPosition + ROW_HEIGHT, KEY_WIDTH, yPosition + ROW_HEIGHT);
                            g.DrawLine(keyBorderPen, KEY_WIDTH, yPosition, KEY_WIDTH, yPosition + ROW_HEIGHT);
                        }

                        // Для ноты C используем яркую метку и выделение
                        if (isCNote)
                        {
                            // Отображаем C с номером октавы
                            int octave = (midiNote / 12) - 1;
                            using (Font font = new Font("Arial", 5, FontStyle.Bold))
                            using (SolidBrush textBrush = new SolidBrush(Color.Red)) // Красный цвет для C
                            {
                                g.DrawString($"C{octave}", font, textBrush, 2, yPosition);
                            }

                            // Рисуем горизонтальную белую линию для выделения нот C
                            g.DrawLine(whiteGridPen, KEY_WIDTH, yPosition, clientRect.Width, yPosition);
                        }
                    }

                    yPosition += ROW_HEIGHT;
                }

                // ===== РИСУЕМ ВЕРТИКАЛЬНЫЕ ЛИНИИ СЕТКИ =====

                // 1. Сначала рисуем вертикальные линии для каждой 1/16 ноты
                for (int position = 0; position <= MAX_HORIZONTAL_MEASURES * NOTES_PER_MEASURE; position++)
                {
                    int posX = KEY_WIDTH + position * NOTE_WIDTH + offsetX;

                    if (posX >= KEY_WIDTH && posX <= clientRect.Width)
                    {
                        // Определяем, какая это доля в такте
                        int beatInMeasure = position % NOTES_PER_MEASURE;

                        if (beatInMeasure == 0) // Начало такта - рисуем позже специальной линией
                        {
                            continue;
                        }
                        else if (beatInMeasure % 4 == 0) // Каждая 1/4 такта (4-я, 8-я, 12-я 16-е)
                        {
                            // Используем более темную линию для четвертей
                            using (Pen quarterPen = new Pen(Color.FromArgb(100, 100, 100), 1))
                            {
                                g.DrawLine(quarterPen, posX, HEADER_HEIGHT,
                                          posX, HEADER_HEIGHT + Math.Min(visibleNotesCount * ROW_HEIGHT, clientRect.Height - HEADER_HEIGHT));
                            }
                        }
                        else // Остальные 16-е
                        {
                            g.DrawLine(sixteenthGridPen, posX, HEADER_HEIGHT,
                                      posX, HEADER_HEIGHT + Math.Min(visibleNotesCount * ROW_HEIGHT, clientRect.Height - HEADER_HEIGHT));
                        }
                    }
                }

                // 2. Теперь рисуем вертикальные линии для тактов (поверх других линий)
                for (int measure = 0; measure <= MAX_HORIZONTAL_MEASURES; measure++)
                {
                    int measureX = KEY_WIDTH + measure * NOTES_PER_MEASURE * NOTE_WIDTH + offsetX;

                    if (measureX >= KEY_WIDTH && measureX <= clientRect.Width)
                    {
                        // Чтобы линия такта выделялась, рисуем ее более толстой черного цвета
                        using (Pen thickPen = new Pen(Color.Black, 2.0f))
                        {
                            g.DrawLine(thickPen, measureX, HEADER_HEIGHT,
                                       measureX, HEADER_HEIGHT + Math.Min(visibleNotesCount * ROW_HEIGHT, clientRect.Height - HEADER_HEIGHT));
                        }
                    }
                }

                // Рисуем горизонтальные линии для нот C
                for (int i = 0; i < visibleNotesCount && highestVisibleNote - i >= lowestVisibleNote; i++)
                {
                    int midiNote = highestVisibleNote - i;
                    if (midiNote % 12 == 0) // Для нот C
                    {
                        int y = HEADER_HEIGHT + i * ROW_HEIGHT;
                        g.DrawLine(whiteGridPen, KEY_WIDTH, y, clientRect.Width, y);
                    }
                }

                // Рисуем тонкие серые линии горизонтальной сетки
                for (int i = 1; i < visibleNotesCount && highestVisibleNote - i >= lowestVisibleNote; i++)
                {
                    int midiNote = highestVisibleNote - i;
                    if (midiNote % 12 != 0) // Для всех нот, кроме C
                    {
                        int y = HEADER_HEIGHT + i * ROW_HEIGHT;
                        g.DrawLine(gridPen, KEY_WIDTH, y, clientRect.Width, y);
                    }
                }
            }

            // Рисуем ноты в видимом диапазоне с учетом их длительности
            using (Pen noteBorderPen = new Pen(Color.White, 1)) // Белая рамка для лучшей видимости
            {
                foreach (var note in _notes)
                {
                    int midiNote = note.MidiNote;
                    int position = note.Position;
                    int duration = note.Duration;

                    // Проверяем, что нота в видимом диапазоне по вертикали и горизонтали
                    if (midiNote <= highestVisibleNote && midiNote >= lowestVisibleNote &&
                        (position + duration) >= firstVisiblePos && position <= lastVisiblePos)
                    {
                        // Вычисляем координаты ноты на экране
                        int noteYIndex = highestVisibleNote - midiNote;
                        int x = KEY_WIDTH + position * NOTE_WIDTH + offsetX;
                        int y = HEADER_HEIGHT + noteYIndex * ROW_HEIGHT;

                        // Ширина ноты зависит от ее длительности
                        int noteWidth = duration * NOTE_WIDTH - 1;

                        // Определяем, является ли нота До (C)
                        bool isCNote = midiNote % 12 == 0;

                        // Для всех нот используем фиолетовый цвет, но для нот C делаем немного темнее
                        using (SolidBrush noteBrush = new SolidBrush(isCNote ?
                            Color.FromArgb(75, 0, 130) : _noteColor))
                        {
                            g.FillRectangle(noteBrush, x, y, noteWidth, ROW_HEIGHT);

                            // Белая рамка вокруг ноты для лучшей видимости
                            g.DrawRectangle(noteBorderPen, x, y, noteWidth, ROW_HEIGHT);

                            // Если длительность больше 1/16, добавляем текст с длительностью
                            if (duration > 1)
                            {
                                using (Font durationFont = new Font("Arial", 5))
                                using (SolidBrush textBrush = new SolidBrush(Color.White))
                                {
                                    string durationText = $"{duration}/16";
                                    if (noteWidth >= 50) // Если достаточно места, отображаем длительность
                                    {
                                        g.DrawString(durationText, durationFont, textBrush,
                                                    x + 2, y + (ROW_HEIGHT - durationFont.Height) / 2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Метод для рисования нотного стана
        private void DrawStaffNotation(Graphics g, Rectangle clientRect)
        {
            // Константы для нотного стана
            int staffX = 60;  // Отступ для ключа и т.д.
            int staffY = 50;  // Верхний отступ
            int lineCount = 5;  // Стандартное количество линий

            // Вычисляем смещение для рисования с учетом прокрутки
            int offsetX = -scrollX;
            int offsetY = -scrollY;

            // Рисуем линии нотного стана
            using (Pen staffPen = new Pen(Color.White, 1)) // Белые линии для лучшей видимости
            {
                for (int i = 0; i < lineCount; i++)
                {
                    int y = staffY + i * STAFF_LINE_SPACING + offsetY;

                    // Рисуем только если линия видима
                    if (y >= 0 && y <= clientRect.Height)
                    {
                        g.DrawLine(staffPen, staffX, y, clientRect.Width, y);
                    }
                }
            }

            // Рисуем бас-ключ
            if (staffX + offsetX > 0 || staffX + offsetX < clientRect.Width)
            {
                using (Font clefFont = new Font("Arial", 30, FontStyle.Bold))
                using (SolidBrush clefBrush = new SolidBrush(Color.White))
                {
                    g.DrawString("?", clefFont, clefBrush, staffX - 40 + offsetX, staffY - 15 + offsetY);
                }
            }

            // Словарь для сопоставления MIDI-номеров нот и их позиций на нотном стане
            Dictionary<int, int> notePositions = new Dictionary<int, int>
            {
                { 36, staffY + 4 * STAFF_LINE_SPACING },    // C2
                { 38, staffY + 3 * STAFF_LINE_SPACING },    // D2
                { 40, staffY + 2 * STAFF_LINE_SPACING },    // E2
                { 41, staffY + (int)(1.5 * STAFF_LINE_SPACING) }, // F2
                { 43, staffY + 1 * STAFF_LINE_SPACING },    // G2
                { 45, staffY + (int)(0.5 * STAFF_LINE_SPACING) }, // A2
                { 47, staffY },                      // B2
                
                // Добавляем также ноты нижней октавы
                { 24, staffY + 8 * STAFF_LINE_SPACING },    // C1
                { 26, staffY + 7 * STAFF_LINE_SPACING },    // D1
                { 28, staffY + 6 * STAFF_LINE_SPACING },    // E1
                { 29, staffY + 5 * STAFF_LINE_SPACING + STAFF_LINE_SPACING/2 }, // F1
                { 31, staffY + 5 * STAFF_LINE_SPACING },    // G1
                { 33, staffY + 4 * STAFF_LINE_SPACING + STAFF_LINE_SPACING/2 }, // A1
                { 35, staffY + 4 * STAFF_LINE_SPACING },    // B1
                
                // И ноты верхней октавы
                { 48, staffY - (int)(0.5 * STAFF_LINE_SPACING) }, // C3
                { 50, staffY - 1 * STAFF_LINE_SPACING },    // D3
                { 52, staffY - 2 * STAFF_LINE_SPACING },    // E3
            };

            // Расстояние между нотами по горизонтали
            int noteXSpacing = NOTE_WIDTH;

            // Рисуем вертикальные линии для каждой 16-й ноты в нотном стане
            using (Pen sixteenthPen = new Pen(Color.FromArgb(150, 150, 150), 1))
            {
                for (int position = 0; position <= MAX_HORIZONTAL_MEASURES * NOTES_PER_MEASURE; position++)
                {
                    int posX = staffX + position * noteXSpacing + offsetX;

                    if (posX >= 0 && posX <= clientRect.Width)
                    {
                        // Определяем, какая это доля в такте
                        int beatInMeasure = position % NOTES_PER_MEASURE;

                        if (beatInMeasure == 0) // Начало такта - рисуем позже специальной линией
                        {
                            continue;
                        }
                        else if (beatInMeasure % 4 == 0) // Каждая 1/4 такта (4-я, 8-я, 12-я 16-е)
                        {
                            int startY = staffY + offsetY;
                            int endY = staffY + 4 * STAFF_LINE_SPACING + offsetY;

                            if ((startY >= 0 && startY <= clientRect.Height) ||
                                (endY >= 0 && endY <= clientRect.Height))
                            {
                                // Используем более темную линию для четвертей
                                using (Pen quarterPen = new Pen(Color.FromArgb(100, 100, 100), 1))
                                {
                                    g.DrawLine(quarterPen, posX, Math.Max(0, startY),
                                              posX, Math.Min(clientRect.Height, endY));
                                }
                            }
                        }
                        else // Остальные 16-е
                        {
                            int startY = staffY + offsetY;
                            int endY = staffY + 4 * STAFF_LINE_SPACING + offsetY;

                            if ((startY >= 0 && startY <= clientRect.Height) ||
                                (endY >= 0 && endY <= clientRect.Height))
                            {
                                g.DrawLine(sixteenthPen, posX, Math.Max(0, startY),
                                          posX, Math.Min(clientRect.Height, endY));
                            }
                        }
                    }
                }
            }

            // Рисуем разделители тактов
            using (Pen measurePen = new Pen(Color.Black, 2.0f)) // Черная толстая линия для тактов
            {
                for (int measure = 0; measure <= MAX_HORIZONTAL_MEASURES; measure++)
                {
                    int measureX = staffX + measure * NOTES_PER_MEASURE * noteXSpacing + offsetX;

                    if (measureX >= 0 && measureX <= clientRect.Width)
                    {
                        int startY = staffY + offsetY;
                        int endY = staffY + 4 * STAFF_LINE_SPACING + offsetY;

                        if ((startY >= 0 && startY <= clientRect.Height) ||
                            (endY >= 0 && endY <= clientRect.Height))
                        {
                            g.DrawLine(measurePen, measureX, Math.Max(0, startY),
                                      measureX, Math.Min(clientRect.Height, endY));

                            // Добавляем номер такта
                            if (measure > 0)
                            {
                                using (Font measureFont = new Font("Arial", 8))
                                using (SolidBrush textBrush = new SolidBrush(Color.White))
                                {
                                    g.DrawString(measure.ToString(), measureFont, textBrush,
                                                measureX + 2, startY - 15);
                                }
                            }
                        }
                    }
                }
            }

            // Рисуем ноты на нотном стане с учетом длительностей
            if (_notes.Count > 0)
            {
                foreach (var note in _notes)
                {
                    int midiNote = note.MidiNote;
                    int position = note.Position;
                    int duration = note.Duration;

                    // Вычисляем позицию ноты
                    int noteX = staffX + position * noteXSpacing + offsetX;

                    // Проверяем, видна ли нота по горизонтали
                    if (noteX >= -NOTE_RADIUS * 2 && noteX <= clientRect.Width + NOTE_RADIUS * 2)
                    {
                        // Нормализуем ноту к базовой октаве, но сохраняем октаву для отображения
                        int normalizedNote = midiNote % 12 + 36;  // Приводим к C2-B2
                        while (normalizedNote < 24) normalizedNote += 12;
                        while (normalizedNote > 52) normalizedNote -= 12;

                        if (notePositions.ContainsKey(normalizedNote))
                        {
                            // Вычисляем Y-позицию ноты с учетом смещения
                            int baseNoteY = notePositions[normalizedNote];

                            // Корректируем позицию на основе реальной октавы
                            int octaveDiff = (midiNote / 12) - (normalizedNote / 12);
                            int noteY = baseNoteY - octaveDiff * 7 * STAFF_LINE_SPACING + offsetY;

                            // Проверяем, видна ли нота по вертикали
                            if (noteY >= -NOTE_RADIUS * 2 && noteY <= clientRect.Height + NOTE_RADIUS * 2)
                            {
                                // Используем фиолетовый цвет для всех нот
                                Color noteColor = _noteColor;

                                // Рисуем ноту
                                using (SolidBrush noteBrush = new SolidBrush(noteColor))
                                {
                                    g.FillEllipse(noteBrush, noteX, noteY - NOTE_RADIUS, NOTE_RADIUS * 2, NOTE_RADIUS * 2);

                                    // Добавляем штиль к ноте
                                    int stemDirection = (noteY > staffY + 2 * STAFF_LINE_SPACING + offsetY) ? -1 : 1;
                                    int stemLength = 3 * STAFF_LINE_SPACING;
                                    int stemX = noteX + NOTE_RADIUS;
                                    int stemStartY = noteY + (stemDirection == -1 ? -NOTE_RADIUS : NOTE_RADIUS);
                                    int stemEndY = stemStartY + stemDirection * stemLength;
                                    g.DrawLine(Pens.White, stemX, stemStartY, stemX, stemEndY);

                                    // Если длительность больше 1/16, отображаем горизонтальную линию для длительности
                                    if (duration > 1)
                                    {
                                        // Рисуем линию от ноты до конца ее длительности
                                        int endX = noteX + duration * noteXSpacing - NOTE_RADIUS;
                                        int beamY = stemDirection == -1 ? stemEndY : stemEndY - 3;

                                        using (Pen durationPen = new Pen(Color.DarkViolet, 2))
                                        {
                                            g.DrawLine(durationPen, stemX, beamY, endX, beamY);
                                        }
                                    }
                                }

                                // Добавляем дополнительные линии для нот за пределами стана
                                using (Pen ledgerPen = new Pen(Color.White, 1))
                                {
                                    if (noteY > staffY + 4 * STAFF_LINE_SPACING + offsetY)  // Ноты ниже стана
                                    {
                                        for (int lineY = staffY + 5 * STAFF_LINE_SPACING + offsetY;
                                             lineY <= noteY;
                                             lineY += STAFF_LINE_SPACING)
                                        {
                                            g.DrawLine(ledgerPen, noteX - NOTE_RADIUS - 2, lineY,
                                                      noteX + NOTE_RADIUS * 2 + 2, lineY);
                                        }
                                    }
                                    else if (noteY < staffY + offsetY)  // Ноты выше стана
                                    {
                                        for (int lineY = staffY - STAFF_LINE_SPACING + offsetY;
                                             lineY >= noteY - NOTE_RADIUS;
                                             lineY -= STAFF_LINE_SPACING)
                                        {
                                            g.DrawLine(ledgerPen, noteX - NOTE_RADIUS - 2, lineY,
                                                      noteX + NOTE_RADIUS * 2 + 2, lineY);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Вспомогательный метод для определения, является ли MIDI-нота черной клавишей
        private bool IsBlackKey(int midiNote)
        {
            int noteInOctave = midiNote % 12;
            return noteInOctave == 1 || noteInOctave == 3 ||
                   noteInOctave == 6 || noteInOctave == 8 || noteInOctave == 10;
        }

        // Вспомогательный метод для конвертации MIDI-номера ноты в название
        private string MidiNoteToName(int midiNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (midiNumber / 12) - 1;
            int noteIndex = midiNumber % 12;
            return noteNames[noteIndex] + octave;
        }
    }
}