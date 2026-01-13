using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAudio.Midi;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PADGeneratorVST
{
    public partial class PADGeneratorForm : Form
    {
        // Вспомогательный класс для представления скейла с его стилем
        public class ScaleItem
        {
            public string Name { get; set; }
            public string Style { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        // Словарь скейлов с категориями, ступенями и качествами
        private readonly Dictionary<string, Dictionary<string, Tuple<string[], string[]>>> scalesChordsByCategory;
        private PADGeneratorVstPlugin _plugin;
        private string _lastGeneratedMidiPath;

        private readonly HashSet<string> psytranceScales = new HashSet<string>()
        {
            "Harmonic Minor", "Phrygian", "Hirajoshi", "In Sen", "Phrygian Dominant", "Double Harmonic Major"
        };

        private readonly HashSet<string> tranceScales = new HashSet<string>()
        {
            "Major", "Minor", "Dorian", "Lydian", "Mixolydian", "Melodic Minor", "Pentatonic Major",
            "Pentatonic Minor", "Blues", "Hungarian Minor", "Spanish Phrygian", "Whole Tone", "Locrian",
            "Blues Hexatonic", "Prometheus"
        };

        private readonly Dictionary<string, int[]> chordFormulas = new Dictionary<string, int[]>()
        {
            { "", new[] { 0, 4, 7 } },           // Триада мажор
            { "maj", new[] { 0, 4, 7 } },       // Мажорная триада
            { "m", new[] { 0, 3, 7 } },         // Минорная триада
            { "dim", new[] { 0, 3, 6 } },       // Уменьшённая триада
            { "aug", new[] { 0, 4, 8 } },       // Увеличенная триада
            { "sus4", new[] { 0, 5, 7 } },      // Сус4 триада
            { "add9", new[] { 0, 4, 7, 14 } },  // Мажорная триада с добавленной 9
            { "7", new[] { 0, 4, 7, 10 } },     // Доминантсептаккорд
            { "maj7", new[] { 0, 4, 7, 11 } },  // Мажорный септаккорд
            { "m7", new[] { 0, 3, 7, 10 } },    // Минорный септаккорд
            { "dim7", new[] { 0, 3, 6, 9 } },   // Уменьшённый септаккорд
            { "m7b5", new[] { 0, 3, 6, 10 } },  // Полууменьшённый септаккорд
            { "m9", new[] { 0, 3, 7, 10, 14 } }, // Минорный нонаккорд
            { "maj9", new[] { 0, 4, 7, 11, 14 } }, // Мажорный нонаккорд
            { "9", new[] { 0, 4, 7, 10, 14 } }, // Доминантнонаккорд
            { "aug7", new[] { 0, 4, 8, 10 } },  // Увеличенный септаккорд
            { "5", new[] { 0, 7 } },            // Квинтаккорд (power chord)
            { "+", new[] { 0, 4, 8 } },         // Увеличенная триада (альтернативная запись)
            { "°", new[] { 0, 3, 6 } },         // Уменьшённая триада (альтернативная запись)
        };

        private readonly Random random = new Random();
        private string lastGeneratedMidiPath = string.Empty;


        // 2. Добавьте эти методы для обработки переключения режимов отображения


        public PADGeneratorForm()
        {
            scalesChordsByCategory = InitializeScalesByCategory();
            InitializeComponent(); // Этот метод создает все элементы управления, включая noteDisplayControl

            // Теперь можно безопасно настраивать noteDisplayControl
            if (noteDisplayControl != null)
            {
                noteDisplayControl.SetDisplayMode(0); // По умолчанию режим пиано-ролла
            }

            // Настраиваем радиокнопки
            rbPianoRoll.Checked = true;

            SetupTextBoxEvents();

            // Продолжаем инициализацию других компонентов
            InitializeTonalityComboBox();
            InitializeCategoryComboBox();
            InitializeScaleComboBox();

            cmbBoxTonality.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBoxCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBoxScale.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbBoxCategory.SelectedIndexChanged += CmbBoxCategory_SelectedIndexChanged;

            foreach (var category in scalesChordsByCategory)
            {
                foreach (var scale in category.Value)
                {
                    if (scale.Value.Item1.Length != scale.Value.Item2.Length)
                    {
                        Log($"Несоответствие количества ступеней и качеств в скейле: {scale.Key} (категория: {category.Key})");
                    }
                }
            }

            // Кнопка воспроизведения MIDI должна быть неактивна при запуске
            btnPlayMIDI.Enabled = false;
        }
        private void rbPianoRoll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPianoRoll.Checked && noteDisplayControl != null)
            {
                noteDisplayControl.SetDisplayMode(0); // Режим пиано-ролла
            }
        }

        private void SetupTextBoxEvents()
        {
            // Подписываемся на события Enter/Leave для всех TextBox
            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Enter += TextBox_Enter;
                    textBox.Leave += TextBox_Leave;
                }
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            // Когда TextBox получает фокус, отключаем обработку клавиш формой
            this.KeyPreview = false;
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            // Когда TextBox теряет фокус, включаем обработку клавиш формой
            this.KeyPreview = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Если фокус на TextBox, пропускаем обработку клавиш хостом
            if (this.ActiveControl is TextBox)
            {
                return false;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        internal void SetPlugin(PADGeneratorVstPlugin plugin)
        {
            _plugin = plugin;
        }

        private void rbStaffNotation_CheckedChanged(object sender, EventArgs e)
        {
            if (rbStaffNotation.Checked && noteDisplayControl != null)
            {
                noteDisplayControl.SetDisplayMode(1); // Режим нотного стана
            }
        }
        private static Dictionary<string, Dictionary<string, Tuple<string[], string[]>>> InitializeScalesByCategory()
        {
            var result = new Dictionary<string, Dictionary<string, Tuple<string[], string[]>>>();

            // Вставьте полный словарь BigScalePatterns из ScaleSelector здесь
            var bigScalePatterns = new Dictionary<string, Dictionary<string, List<int>>>
{
    // Категория: Классические/Западные
    {
        "Классические/Западные", new Dictionary<string, List<int>>
        {
            {"Мажор (Ionian)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Натуральный минор (Aeolian)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Гармонический минор", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Мелодический минор (Asc.)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Дорийский (Dorian)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фригийский (Phrygian)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Лидийский (Lydian)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Миксолидийский (Mixolyd.)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский (Locrian)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Неаполитанский мажор", new List<int> {1, 2, 2, 2, 2, 2, 1}},
            {"Неаполитанский минор", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Целотонный (Whole Tone)", new List<int> {2, 2, 2, 2, 2, 2}},
            {"Хроматический (12 Half)", new List<int> {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
            {"Лидийский доминантный (Lydian Dominant)", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Фригийский доминантный (Phrygian Dominant)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Миксолидийский ♭6", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Локрийский ♮2", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Аугментированный (Augmented)", new List<int> {3, 1, 3, 1, 3, 1}},
            {"Эолийский ♭5 (Aeolian ♭5)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Дорийский ♯4", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Сверхлокрийский (Super Locrian)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Миксолидийский ♭2", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Дорийский ♭5", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Лидийский ♯2", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Фригийский ♯6", new List<int> {1, 2, 2, 2, 2, 1, 2}},
            {"Миксолидийский ♯11", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Локрийский ♭4", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Гармонический мажор", new List<int> {2, 2, 1, 2, 1, 3, 1}},
            {"Мелодический мажор", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Эолийский ♯7", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Дорийский ♯2", new List<int> {3, 1, 2, 2, 1, 2, 1}},
            {"Лидийский ♭3", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Фригийский ♯3", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Миксолидийский ♭13", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Локрийский ♮6", new List<int> {1, 2, 2, 1, 3, 1, 2}},
            {"Ионийский ♯5", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Эолийский ♭1", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Ионийский ♭6", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дорийский ♭13", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♭7", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Лидийский ♯5", new List<int> {2, 2, 2, 2, 1, 2, 1}},
            {"Миксолидийский ♯9", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Локрийский ♯7", new List<int> {1, 2, 2, 1, 2, 3, 1}},
            {"Эолийский ♯3", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дорийский ♭3", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Фригийский ♯2", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♭9", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♭5", new List<int> {2, 2, 1, 1, 2, 2, 2}},
            {"Локрийский ♮3", new List<int> {1, 3, 1, 1, 2, 2, 2}},
            {"Ионийский ♭2", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Эолийский ♯6", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Дорийский ♯7", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Фригийский ♭5", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Лидийский ♯11", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯2", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Локрийский ♭6", new List<int> {1, 2, 2, 1, 2, 1, 3}},
            {"Ионийский ♭3", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Эолийский ♭9", new List<int> {1, 3, 2, 2, 1, 2, 2}},
            {"Дорийский ♭4", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Фригийский ♯9", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♭13", new List<int> {2, 2, 2, 1, 1, 2, 2}},
            {"Миксолидийский ♭7", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♯5", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Ионийский ♯9", new List<int> {3, 1, 1, 2, 2, 2, 1}},
            {"Эолийский ♭2", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Дорийский ♯6", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фригийский ♭3", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Лидийский ♯3", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯4", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Локрийский ♮9", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Ионийский ♭5", new List<int> {2, 2, 1, 1, 2, 2, 2}},
            {"Эолийский ♯4", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Дорийский ♭6", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯4", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Лидийский ♭2", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♭9", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Локрийский ♭3", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Ионийский ♯6", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♭7", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Дорийский ♯9", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Фригийский ♭6", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Лидийский ♯7", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♭4", new List<int> {2, 2, 1, 1, 3, 1, 2}},
            {"Локрийский ♮4", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Ионийский ♭9", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Эолийский ♯5", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Дорийский ♭7", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯5", new List<int> {1, 2, 2, 2, 2, 1, 2}},
            {"Лидийский ♭4", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯6", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♭9", new List<int> {1, 3, 1, 1, 2, 2, 2}},
            {"Ионийский ♯2", new List<int> {3, 1, 1, 2, 2, 2, 1}},
            {"Эолийский ♭3", new List<int> {2, 1, 1, 3, 1, 2, 2}},
            {"Дорийский ♯5", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фригийский ♭9", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Лидийский ♯6", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭3", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Локрийский ♮7", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Ионийский ♭7", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯9", new List<int> {3, 1, 2, 2, 1, 2, 2}},
            {"Дорийский ♭2", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Фригийский ♯7", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Лидийский ♭5", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯5", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Локрийский ♭2", new List<int> {1, 1, 3, 1, 2, 2, 2}},
            {"Ионийский ♯3", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Эолийский ♭6", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            // Новые лады (70)
            {"Ионийский ♭13", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дорийский ♯13", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фригийский ♭13", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Лидийский ♯13", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭11", new List<int> {2, 2, 1, 1, 3, 1, 2}},
            {"Локрийский ♯11", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Эолийский ♭4", new List<int> {2, 1, 1, 3, 1, 2, 2}},
            {"Дорийский ♭11", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Фригийский ♯11", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Лидийский ♭11", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯13", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♭5", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Ионийский ♯11", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Эолийский ♯11", new List<int> {2, 1, 3, 1, 1, 2, 2}},
            {"Дорийский ♭9", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Фригийский ♭4", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Лидийский ♯9", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Локрийский ♮5", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Ионийский ♭4", new List<int> {2, 2, 1, 1, 3, 2, 1}},
            {"Эолийский ♭13", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Дорийский ♯3", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♯13", new List<int> {1, 2, 2, 2, 2, 1, 2}},
            {"Лидийский ♭7", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯7", new List<int> {2, 2, 1, 2, 1, 3, 1}},
            {"Локрийский ♭11", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Ионийский ♯13", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯2", new List<int> {3, 1, 2, 2, 1, 2, 2}},
            {"Дорийский ♭10", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Фригийский ♭10", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Лидийский ♯10", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♭10", new List<int> {2, 2, 1, 1, 2, 2, 2}},
            {"Локрийский ♮10", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Ионийский ♭10", new List<int> {2, 2, 1, 1, 2, 2, 2}},
            {"Эолийский ♯10", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Дорийский ♭8", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯8", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Лидийский ♭8", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯8", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♮8", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Ионийский ♭8", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯8", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Дорийский ♭12", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯12", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Лидийский ♭12", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯12", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♮12", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Ионийский ♭12", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯12", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Дорийский ♭14", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯14", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Лидийский ♭14", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯14", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♮14", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Ионийский ♭14", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯14", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Дорийский ♭15", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фригийский ♯15", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Лидийский ♭15", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♯15", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Локрийский ♮15", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Ионийский ♭15", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Эолийский ♯15", new List<int> {2, 1, 2, 2, 1, 3, 1}}
        }
    },
    // Категория: Джазовые
    {
        "Джазовые", new Dictionary<string, List<int>>
        {
            {"Мажорный бибоп", new List<int> {2, 2, 1, 2, 2, 1, 1, 2}},
            {"Минорный бибоп", new List<int> {2, 1, 2, 2, 1, 1, 2, 2}},
            {"Доминиантный бибоп", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Джаз-фанк (Jazz Funk)", new List<int> {2, 2, 1, 2, 1, 1, 2}},
            {"Афро-джаз (Afro-Jazz)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Джазовая целотонная (Jazz Whole Tone)", new List<int> {2, 2, 2, 2, 1, 3}},
            {"Лидийский ♭7 (Lydian ♭7)", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭9", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Дорийский ♭2", new List<int> {1, 2, 2, 2, 2, 1, 2}},
            {"Хроматический бибоп (Chromatic Bebop)", new List<int> {2, 1, 1, 2, 2, 1, 1, 2}},
            {"Фригийский ♭4", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Лидийский увеличенный (Lydian Augmented)", new List<int> {2, 2, 2, 2, 1, 2, 1}},
            {"Полутон-целотон (Half-Whole)", new List<int> {1, 2, 1, 2, 1, 2, 1, 2}},
            {"Целотон-полутон (Whole-Half)", new List<int> {2, 1, 2, 1, 2, 1, 2, 1}},
            {"Дорийский ♭9", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Лидийский ♯9", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♯5", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Фригийский ♯7", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Локрийский ♭13", new List<int> {1, 2, 2, 1, 2, 1, 3}},
            {"Бибоп мажор ♭6", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Бибоп минор ♯4", new List<int> {2, 1, 3, 1, 1, 1, 2, 2}},
            {"Дорийский ♯11", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Лидийский ♭6", new List<int> {2, 2, 2, 1, 1, 2, 2}},
            {"Миксолидийский ♭3", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фригийский ♭2", new List<int> {1, 1, 3, 2, 1, 2, 2}},
            {"Локрийский ♯3", new List<int> {1, 3, 1, 1, 2, 2, 2}},
            {"Джазовый симметричный", new List<int> {1, 3, 1, 3, 1, 3}},
            {"Целотонный ♭5", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Бибоп доминант ♯9", new List<int> {3, 1, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♯5", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Лидийский ♯2", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♭13", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Фригийский ♯6", new List<int> {1, 2, 2, 2, 2, 1, 2}},
            {"Локрийский ♮2", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Бибоп мажор ♯5", new List<int> {2, 2, 2, 1, 1, 1, 2, 2}},
            {"Бибоп минор ♭9", new List<int> {1, 3, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭4", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Лидийский ♭3", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Миксолидийский ♯11", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Фригийский ♭5", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Локрийский ♯4", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Джазовый ♭6", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Целотонный ♯5", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Бибоп доминант ♭13", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭7", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♯6", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭2", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Фригийский ♯9", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Локрийский ♮6", new List<int> {1, 2, 2, 1, 3, 1, 2}},
            {"Бибоп мажор ♭9", new List<int> {1, 3, 1, 2, 2, 1, 1, 2}},
            {"Бибоп минор ♯7", new List<int> {2, 1, 2, 2, 1, 1, 3, 1}},
            {"Дорийский ♯3", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Лидийский ♭4", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯4", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Фригийский ♭3", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Локрийский ♯5", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Джазовый ♭2", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Целотонный ♭3", new List<int> {2, 1, 2, 2, 2, 3}},
            {"Бибоп доминант ♯5", new List<int> {2, 2, 2, 1, 1, 1, 2, 2}},
            {"Дорийский ♭6", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♯3", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♭7", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♯2", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Локрийский ♮9", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Бибоп мажор ♯9", new List<int> {3, 1, 1, 2, 2, 1, 1, 2}},
            {"Бибоп минор ♭13", new List<int> {2, 1, 2, 2, 1, 1, 2, 2}},
            {"Дорийский ♯6", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Лидийский ♭9", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♯6", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♭6", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Локрийский ♮4", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Джазовый ♭3", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Целотонный ♯2", new List<int> {3, 1, 2, 2, 2, 2}},
            {"Бибоп доминант ♭9", new List<int> {1, 3, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭3", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Лидийский ♯4", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭4", new List<int> {2, 2, 1, 1, 3, 1, 2}},
            {"Фригийский ♯3", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Локрийский ♭7", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Джазовый ♯5", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Целотонный ♭6", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Бибоп мажор ♭2", new List<int> {1, 3, 1, 2, 2, 1, 1, 2}},
            {"Бибоп минор ♯6", new List<int> {2, 1, 2, 2, 2, 1, 1, 2}},
            {"Дорийский ♯9", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Лидийский ♭2", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Миксолидийский ♯9", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Фригийский ♭9", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Локрийский ♮5", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Джазовый ♭9", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Целотонный ♯3", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Бибоп доминант ♯2", new List<int> {3, 1, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭5", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Лидийский ♯7", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Фригийский ♯4", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Локрийский ♭2", new List<int> {1, 1, 3, 1, 2, 2, 2}},
            {"Джазовый ♯2", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Целотонный ♭7", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            // Новые лады (70)
            {"Джазовый ♭13", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Бибоп мажор ♭13", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Бибоп минор ♯9", new List<int> {3, 1, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭11", new List<int> {2, 1, 1, 3, 2, 1, 2}},
            {"Лидийский ♯11", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭11", new List<int> {2, 2, 1, 1, 3, 1, 2}},
            {"Фригийский ♯11", new List<int> {1, 2, 3, 1, 1, 2, 2}},
            {"Локрийский ♭4", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Джазовый ♯6", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Целотонный ♯9", new List<int> {3, 1, 2, 2, 2, 2}},
            {"Бибоп доминант ♭6", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♯13", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Лидийский ♭13", new List<int> {2, 2, 2, 1, 1, 2, 2}},
            {"Миксолидийский ♯13", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♭13", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Локрийский ♯6", new List<int> {1, 2, 2, 1, 3, 1, 2}},
            {"Джазовый ♭4", new List<int> {2, 2, 1, 1, 3, 2, 1}},
            {"Целотонный ♭2", new List<int> {1, 3, 2, 2, 2, 2}},
            {"Бибоп мажор ♯11", new List<int> {2, 2, 2, 1, 1, 1, 2, 2}},
            {"Бибоп минор ♭2", new List<int> {1, 3, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭8", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♯8", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭8", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♯8", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Локрийский ♮8", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Джазовый ♯9", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Целотонный ♭13", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Бибоп доминант ♯11", new List<int> {2, 2, 2, 1, 1, 1, 2, 2}},
            {"Дорийский ♯4", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Лидийский ♭5", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯2", new List<int> {3, 1, 1, 2, 2, 1, 2}},
            {"Фригийский ♭7", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Локрийский ♯7", new List<int> {1, 2, 2, 1, 2, 3, 1}},
            {"Джазовый ♭5", new List<int> {2, 2, 1, 1, 2, 2, 2}},
            {"Целотонный ♯6", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Бибоп мажор ♭3", new List<int> {2, 1, 2, 2, 2, 1, 1, 2}},
            {"Бибоп минор ♯3", new List<int> {2, 2, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♭12", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♯12", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭12", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♯12", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Локрийский ♮12", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Джазовый ♯11", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Целотонный ♭9", new List<int> {1, 3, 2, 2, 2, 2}},
            {"Бибоп доминант ♭2", new List<int> {1, 3, 1, 2, 1, 1, 2, 2}},
            {"Дорийский ♯10", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Лидийский ♭10", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Миксолидийский ♯10", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♭10", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Локрийский ♮10", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Джазовый ♭11", new List<int> {2, 2, 1, 1, 3, 1, 2}},
            {"Целотонный ♯11", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Бибоп мажор ♭4", new List<int> {2, 2, 1, 1, 2, 1, 2, 2}},
            {"Бибоп минор ♯5", new List<int> {2, 1, 2, 2, 2, 1, 1, 2}},
            {"Дорийский ♭14", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Лидийский ♯14", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Миксолидийский ♭14", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фригийский ♯14", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Локрийский ♮14", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Джазовый ♭7", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Целотонный ♯13", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Бибоп доминант ♭3", new List<int> {2, 1, 2, 2, 1, 1, 2, 2}}
        }
    },
    // Категория: Пентатоники/Блюз
    {
        "Пентатоники/Блюз", new Dictionary<string, List<int>>
        {
            {"Мажорная пентатоника", new List<int> {2, 2, 3, 2, 3}},
            {"Минорная пентатоника", new List<int> {3, 2, 2, 3, 2}},
            {"Блюзовая (Blues)", new List<int> {3, 2, 1, 1, 3, 2}},
            {"Египетская (Egyptian)", new List<int> {2, 1, 2, 2, 2, 2, 3}},
            {"Японская (Hirajoshi)", new List<int> {2, 1, 4, 1, 4}},
            {"Японская (Iwato)", new List<int> {1, 4, 1, 4, 2}},
            {"Китайская (Mongolian)", new List<int> {2, 2, 3, 2, 3}},
            {"Вьетнамская (Vietnamese)", new List<int> {2, 3, 2, 2, 3}},
            {"Тибетская (Tibetan)", new List<int> {2, 2, 3, 2, 3}},
            {"Малайская (Pelog)", new List<int> {1, 2, 4, 1, 4}},
            {"Индийская (Malkauns)", new List<int> {3, 2, 2, 3, 2}},
            {"Индийская (Bhoopali)", new List<int> {2, 2, 1, 2, 3}},
            {"Индийская (Shivranjani)", new List<int> {2, 1, 2, 3, 4}},
            {"Японская (Kumoi)", new List<int> {2, 1, 4, 2, 3}},
            {"Индийская (Hamsadhwani)", new List<int> {2, 2, 2, 3, 3}},
            {"Блюзовая мажорная (Major Blues)", new List<int> {2, 1, 1, 2, 3, 3}},
            {"Пентатоника доминантная (Dominant Pentatonic)", new List<int> {2, 2, 3, 2, 3}},
            {"Кельтская пентатоника (Celtic Pentatonic)", new List<int> {2, 3, 2, 3, 2}},
            {"Балийская (Balinese)", new List<int> {1, 2, 4, 1, 4}},
            {"Индийская (Khamaj)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Японская (Nohkan)", new List<int> {2, 3, 2, 1, 4}},
            {"Африканская (Zezuru)", new List<int> {3, 2, 2, 2, 3}},
            {"Индийская (Mand)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Ritsu)", new List<int> {2, 2, 3, 1, 4}},
            {"Китайская (Yu)", new List<int> {2, 3, 2, 2, 3}},
            {"Тайская (Sawasdee)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Nguni)", new List<int> {3, 2, 1, 3, 3}},
            {"Индийская (Basant)", new List<int> {2, 2, 2, 1, 3}},
            {"Японская (Miyako)", new List<int> {1, 4, 1, 3, 3}},
            {"Кельтская (Breton)", new List<int> {2, 1, 3, 3, 3}},
            {"Блюзовая ♯5", new List<int> {3, 2, 2, 1, 3, 1}},
            {"Пентатоника ♭9", new List<int> {1, 3, 2, 3, 3}},
            {"Индийская (Bilaval)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Японская (Yo)", new List<int> {2, 2, 3, 2, 3}},
            {"Китайская (Pi)", new List<int> {3, 2, 2, 2, 3}},
            {"Тайская (Phaung)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Baka)", new List<int> {2, 2, 2, 3, 3}},
            {"Индийская (Megh)", new List<int> {2, 1, 2, 3, 4}},
            {"Японская (In)", new List<int> {1, 4, 1, 4, 2}},
            {"Кельтская (Galician)", new List<int> {2, 1, 3, 2, 4}},
            {"Блюзовая ♭13", new List<int> {3, 2, 1, 1, 2, 2, 2}},
            {"Пентатоника ♯2", new List<int> {3, 1, 2, 3, 3}},
            {"Индийская (Sarang)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Shinobue)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Qing)", new List<int> {2, 3, 2, 1, 4}},
            {"Тайская (Lanna)", new List<int> {2, 2, 2, 1, 3}},
            {"Африканская (Shona)", new List<int> {3, 2, 2, 1, 4}},
            {"Индийская (Malhar)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Koto)", new List<int> {1, 4, 2, 1, 4}},
            {"Кельтская (Welsh)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♯9", new List<int> {3, 1, 1, 1, 3, 2}},
            {"Пентатоника ♭3", new List<int> {2, 1, 3, 3, 3}},
            {"Индийская (Gaud)", new List<int> {2, 2, 2, 1, 3}},
            {"Японская (Shakuhachi)", new List<int> {2, 3, 1, 4, 2}},
            {"Китайская (Xun)", new List<int> {2, 2, 3, 2, 3}},
            {"Тайская (Isan)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Tswana)", new List<int> {3, 2, 1, 3, 3}},
            {"Индийская (Tilang)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Hichiriki)", new List<int> {1, 4, 1, 3, 3}},
            {"Кельтская (Cornish)", new List<int> {2, 1, 3, 2, 4}},
            {"Пентатоника ♯5", new List<int> {2, 2, 2, 3, 2}},
            {"Индийская (Ahir)", new List<int> {2, 1, 3, 2, 3}},
            {"Японская (Gagaku)", new List<int> {2, 2, 3, 1, 4}},
            {"Китайская (Zheng)", new List<int> {2, 3, 2, 2, 3}},
            {"Тайская (Morlam)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Xhosa)", new List<int> {3, 2, 1, 3, 3}},
            {"Индийская (Sohini)", new List<int> {2, 2, 2, 1, 3}},
            {"Японская (Ryukyu)", new List<int> {1, 4, 1, 3, 3}},
            {"Кельтская (Manx)", new List<int> {2, 1, 3, 2, 4}},
            {"Блюзовая ♯2", new List<int> {3, 1, 1, 1, 3, 2}},
            {"Пентатоника ♭6", new List<int> {2, 2, 2, 1, 3}},
            {"Индийская (Vachaspati)", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Японская (Sanshin)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Pipa)", new List<int> {2, 3, 2, 1, 4}},
            {"Тайская (Khene)", new List<int> {2, 2, 2, 1, 3}},
            {"Африканская (Venda)", new List<int> {3, 2, 2, 1, 4}},
            {"Индийская (Shyam)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Taiko)", new List<int> {1, 4, 2, 1, 4}},
            {"Кельтская (Pictish)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♭3", new List<int> {2, 1, 2, 1, 3, 2}},
            {"Пентатоника ♯11", new List<int> {2, 2, 2, 1, 3}},
            // Новые лады (50)
            {"Индийская (Kapi)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Okinawa)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Bianzhong)", new List<int> {2, 3, 2, 2, 3}},
            {"Тайская (Sukothai)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Akan)", new List<int> {3, 2, 1, 3, 3}},
            {"Индийская (Jhinjhoti)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Японская (Komabue)", new List<int> {1, 4, 1, 4, 2}},
            {"Кельтская (Hebridean)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♭6", new List<int> {3, 2, 1, 1, 2, 2, 2}},
            {"Пентатоника ♯13", new List<int> {2, 2, 2, 1, 3}},
            {"Индийская (Deshkar)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Nokan)", new List<int> {2, 3, 2, 1, 4}},
            {"Китайская (Yueqin)", new List<int> {2, 3, 2, 2, 3}},
            {"Тайская (Ayutthaya)", new List<int> {2, 1, 3, 2, 4}},
            {"Африканская (Yoruba)", new List<int> {3, 2, 1, 3, 3}},
            {"Индийская (Mianki)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Tsugaru)", new List<int> {1, 4, 2, 1, 4}},
            {"Кельтская (Shetland)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♯11", new List<int> {3, 2, 2, 1, 3, 1}},
            {"Пентатоника ♭2", new List<int> {1, 3, 2, 3, 3}},
            {"Индийская (Patdeep)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Biwa)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Sanxian)", new List<int> {2, 3, 2, 1, 4}},
            {"Тайская (Chiang Mai)", new List<int> {2, 2, 2, 1, 3}},
            {"Африканская (Hausa)", new List<int> {3, 2, 2, 1, 4}},
            {"Индийская (Bageshree)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Shamisen)", new List<int> {1, 4, 2, 1, 4}},
            {"Кельтская (Orkney)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♭2", new List<int> {1, 3, 1, 1, 3, 2}},
            {"Пентатоника ♯9", new List<int> {3, 1, 2, 3, 3}},
            {"Индийская (Maru)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Kagura)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Guzheng)", new List<int> {2, 3, 2, 1, 4}},
            {"Тайская (Lan Chang)", new List<int> {2, 2, 2, 1, 3}},
            {"Африканская (Zulu)", new List<int> {3, 2, 2, 1, 4}},
            {"Индийская (Pahadi)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Японская (Noh)", new List<int> {1, 4, 1, 4, 2}},
            {"Кельтская (Faroese)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♯13", new List<int> {3, 2, 1, 1, 3, 1}},
            {"Пентатоника ♭13", new List<int> {2, 2, 1, 3, 3}},
            {"Индийская (Sindhu)", new List<int> {2, 1, 2, 2, 3}},
            {"Японская (Ainu)", new List<int> {2, 1, 4, 1, 4}},
            {"Китайская (Hulusi)", new List<int> {2, 3, 2, 1, 4}},
            {"Тайская (Phin)", new List<int> {2, 2, 2, 1, 3}},
            {"Африканская (Swahili)", new List<int> {3, 2, 2, 1, 4}},
            {"Индийская (Kedara)", new List<int> {2, 2, 1, 2, 3}},
            {"Японская (Kabuki)", new List<int> {1, 4, 2, 1, 4}},
            {"Кельтская (Breton Minor)", new List<int> {2, 1, 2, 2, 3}},
            {"Блюзовая ♭11", new List<int> {3, 2, 1, 1, 2, 2, 2}},
            {"Пентатоника ♯6", new List<int> {2, 2, 2, 1, 3}},
        }
    },
    // Категория: Восточные/Экзотические
    {
        "Восточные/Экзотические", new Dictionary<string, List<int>>
        {
            {"Арабский (Bhairav)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Индийская (Todi)", new List<int> {1, 2, 2, 2, 2, 2, 1}},
            {"Персидская (Persian)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Арабская (Hijaz)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Суфийская (Sufi)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Двойной гармонический (Double Harm.)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Венгерский минор (Hungarian Minor)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Венгерский мажор (Hungarian Major)", new List<int> {3, 1, 2, 1, 2, 1, 2}},
            {"Индийская (Purvi)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Турецкая (Makam Rast)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Индийская (Bhairavi)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Греческая (Enigmatic)", new List<int> {1, 3, 2, 1, 2, 1, 2}},
            {"Индийская (Marwa)", new List<int> {1, 3, 2, 2, 2, 1, 1}},
            {"Корейская (Pyeongjo)", new List<int> {2, 2, 3, 2, 3}},
            {"Марокканская (Maqam)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Индонезийская (Slendro)", new List<int> {2, 2, 3, 3, 2}},
            {"Сирийская (Bayati)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Yaman)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Индийская (Kafi)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Японская (Ryo)", new List<int> {2, 2, 3, 2, 3}},
            {"Тунисская (Tunisian)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Индийская (Darbari)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Камбоджийская (Khmer)", new List<int> {2, 2, 3, 2, 3}},
            {"Ливанская (Lebanese)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Азербайджанская (Mugham)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Казахская (Kazakh)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Индийская (Bageshri)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Throat)", new List<int> {2, 2, 3, 2, 3}},
            {"Иракская (Iraqi)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Узбекская (Uzbek)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Йеменская (Yemeni)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Туркменская (Turkmen)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Индийская (Asavari)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Турецкая (Hicazkar)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Персидская (Shur)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Desh)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Арабская (Nahawand)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Японская (Sakura)", new List<int> {1, 4, 2, 1, 4}},
            {"Индийская (Kalyan)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Тайская (Phimai)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Gong)", new List<int> {2, 2, 3, 2, 3}},
            {"Индийская (Pilu)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Турецкая (Segah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Jaunpuri)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Bayat-e Esfahan)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Арабская (Saba)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Индийская (Bhairav Thaat)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Тайская (Khlui)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Китайская (Zhi)", new List<int> {2, 2, 1, 2, 3, 2}},
            {"Индийская (Jog)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Узбекская (Navoi)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Монгольская (Urtyn Duu)", new List<int> {2, 2, 3, 2, 3}},
            {"Индийская (Shankarabharanam)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Турецкая (Ussak)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Dastgah-e Shur)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Арабская (Rast Panjgah)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Индийская (Kamavardhani)", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Тайская (Ranad)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Shang)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Charukesi)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Узбекская (Buzruk)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Khuumii)", new List<int> {2, 3, 2, 2, 3}},
            {"Турецкая (Kurdilihicazkar)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Natabhairavi)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Mahur)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Арабская (Ajam)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Индийская (Shuddha Dhanyasi)", new List<int> {2, 1, 2, 3, 4}},
            {"Тайская (Nok)", new List<int> {2, 1, 3, 2, 3, 1}},
            {"Китайская (Jue)", new List<int> {3, 2, 2, 3, 2}},
            {"Индийская (Hemavati)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Узбекская (Rast)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Монгольская (Bayati)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            // Новые лады (100)
            {"Индийская (Kalyani)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Турецкая (Huzzam)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Арабская (Suznak)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Индийская (Rageshri)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Тайская (Pathet)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Guqin)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Shanmukhapriya)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Узбекская (Shashmaqom)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Morin Khuur)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Nihavent)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Индийская (Dhenuka)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Персидская (Segah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Арабская (Hijaz Kar)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Kedar)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Тайская (Sralai)", new List<int> {2, 1, 3, 2, 3, 1}},
            {"Китайская (Erhu)", new List<int> {2, 2, 3, 2, 3}},
            {"Индийская (Mayamalavagowla)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Узбекская (Tanbur)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Yatga)", new List<int> {2, 3, 2, 2, 3}},
            {"Турецкая (Sultaniyegah)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Индийская (Gowrimanohari)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Chahargah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Арабская (Bayati Shuri)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Bhavapriya)", new List<int> {1, 3, 1, 2, 2, 1, 2}},
            {"Тайская (Pin)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Xiao)", new List<int> {2, 2, 3, 2, 3}},
            {"Индийская (Suryakantam)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Узбекская (Dutar)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Tsuur)", new List<int> {2, 3, 2, 2, 3}},
            {"Турецкая (Bestenigar)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Индийская (Dharmavati)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Rast-Panjgah)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Арабская (Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Индийская (Chakravakam)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Тайская (Chakhe)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Dizi)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Vagadheeswari)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Узбекская (Rubab)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Shudarga)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Huseyni)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Индийская (Hanumatodi)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Homayun)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Арабская (Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Gamanashrama)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Тайская (Saw)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Sheng)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Kokilapriya)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Узбекская (Sato)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Ikh Khuur)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Saba Zemzeme)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Ramapriya)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Dasht-e Arzoo)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Арабская (Hijaz Kar Kurd)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Senavati)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Тайская (Khong)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Yangqin)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Harikambhoji)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Узбекская (Ghidjak)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Limbe)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Acem)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Индийская (Kharaharapriya)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Bayat-e Turk)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Арабская (Nawa Athar)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Индийская (Ganamurti)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Тайская (Ranat)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Banhu)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Navaneetam)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Узбекская (Chang)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Tovshuur)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Suzidil)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Индийская (Pavani)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Персидская (Bayat-e Zand)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Арабская (Shahnaz)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Rupavati)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Тайская (Pi)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Gaohu)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Gavambodhi)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Узбекская (Nay)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Монгольская (Shanz)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Beyati)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Индийская (Bhairav Bahar)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Персидская (Afshari)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Арабская (Rahat al-Arwah)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Kalyani Yaman)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Тайская (Thon)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Liuqin)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Sarasangi)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Узбекская (Karnay)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Hoomei)", new List<int> {2, 2, 3, 2, 3}},
            {"Турецкая (Mahur)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Индийская (Vasantha)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Персидская (Bayat-e Kord)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Арабская (Hijaz Nawah)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Индийская (Jyoti)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Тайская (Khong Wong)", new List<int> {2, 2, 1, 3, 2, 2}},
            {"Китайская (Ruan)", new List<int> {2, 3, 2, 1, 4}},
            {"Индийская (Latangi)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Узбекская (Doira)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Монгольская (Choor)", new List<int> {2, 2, 3, 2, 3}},
            // Добавленные еврейские лады
            {"Еврейский (Ahava Rabbah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Еврейский (Magen Avot)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Еврейский (Mi Sheberach)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Еврейский (Adonai Malach)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Еврейский (Klezmer Phrygian)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Еврейский (Yishtabach)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Еврейский (Freygish)", new List<int> {1, 3, 1, 2, 1, 2, 2}}, // Альтернативное название для Ahava Rabbah
            {"Еврейский (Misheberach Minor)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Еврейский (Klezmer Major)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Еврейский (Shabbat Mode)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Индийская (Shri)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Индийская (Hindol)", new List<int> {2, 2, 2, 3, 3}},
            {"Индийская (Tilak Kamod)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Индийская (Chandrakauns)", new List<int> {2, 1, 3, 2, 4}},
            {"Индийская (Shuddha Sarali)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Турецкая (Çargah)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Турецкая (Buselik)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Турецкая (Isfahanek)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Турецкая (Nişaburek)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Турецкая (Hüzzam Segah)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Персидская (Nava)", new List<int> {2, 1, 2, 2, 1, 2, 2}}
        }
    },
    // Категория: Арабские макаматы
    {
        "Арабские макаматы", new Dictionary<string, List<int>>
        {
            {"Раст (راست - Rast)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Баяти (بياتي - Bayati)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Сика (سيكاه - Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз (حجاز - Hijaz)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Хиджаз Кар (حجاز كار - Hijaz Kar)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Нахаванд (نهاوند - Nahawand)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Курд (كرد - Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Адшам (عجم - Ajam)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Саба (صبا - Saba)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Сузнак (سوزناك - Suznak)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Хуззам (هزام - Huzzam)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Бастанигар (بستنكار - Bastanigar)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Шахназ (شهناز - Shahnaz)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Рахат аль-Арвах (راحة الأرواح - Rahat al-Arwah)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Нихавент (نهاوند - Nihavent)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Хусейни (حسيني - Husayni)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Шури (بياتي شوري - Bayati Shuri)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Раст Панжгах (راست پنجگاه - Rast Panjgah)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Курдилихиджазкар (كرديلهجازكار - Kurdilihijazkar)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Сабаземземе (صبا زمزمة - Saba Zemzeme)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Навва (حجاز نوا - Hijaz Nawah)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Адшам Ашир (عجم عاشير - Ajam Ashir)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Суздиль (سوزديل - Suzidil)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Султанийегях (سلطاني يگاه - Sultaniyegah)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Нахаванд Курд (نهاوند كرد - Nahawand Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Хиджаз Кар Курд (حجاز كار كرد - Hijaz Kar Kurd)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Сика Балди (سيكاه بلدي - Sikah Baladi)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Рамаль (رمل - Ramal)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Шадд Арабан (شد عربان - Shadd Araban)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Зангула (زنكولة - Zangula)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Махур (ماهور - Mahur)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Бузург (بزرگ - Buzurg)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Ирак (عراق - Iraq)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Исфахан (أصفهان - Isfahan)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Джигархан (جكارخان - Jigarkhan)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Харидж Хиджаз (خارج حجاز - Kharaj Hijaz)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Фарахфеза (فرحفزا - Farahfaza)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Шуштар (شوشتر - Shushtar)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Накш (نقش - Naqsh)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Баяти Курд (بياتي كرد - Bayati Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Ашир (حجاز عاشير - Hijaz Ashir)", new List<int> {1, 3, 1, 2, 1, 3, 1}},
            {"Сузнак Курд (سوزناك كرد - Suznak Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Раст Курд (راست كرد - Rast Kurd)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сика Хузам (سيكاه هزام - Sikah Huzam)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Нахаванд Хиджаз (نهاوند حجاز - Nahawand Hijaz)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Курд Хиджаз (كرد حجاز - Kurd Hijaz)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Адшам Мурраса (عجم مرصع - Ajam Murrasa)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Саба Далансин (صبا دلنشين - Saba Dalansin)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Хуззам Курд (هزام كرد - Huzzam Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Бастанигар Курд (بستنكار كرد - Bastanigar Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Шахназ Курд (شهناز كرد - Shahnaz Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Рахат аль-Арвах Курд (راحة الأرواح كرد - Rahat al-Arwah Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Нихавент Хиджаз (نهاوند حجاز - Nihavent Hijaz)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Хусейни Курд (حسيني كرد - Husayni Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Шури Курд (بياتي شوري كرد - Bayati Shuri Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Раст Панжгах Курд (راست پنجگاه كرد - Rast Panjgah Kurd)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Курдилихиджазкар Курд (كرديلهجازكار كرد - Kurdilihijazkar Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сабаземземе Курд (صبا زمزمة كرد - Saba Zemzeme Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Навва Курд (حجاز نوا كرد - Hijaz Nawah Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Адшам Ашир Курд (عجم عاشير كرد - Ajam Ashir Kurd)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Суздиль Курд (سوزديل كرد - Suzidil Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Султанийегях Курд (سلطاني يگاه كرد - Sultaniyegah Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Нахаванд Хиджаз Кар (نهاوند حجاز كار - Nahawand Hijaz Kar)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Хиджаз Кар Курд Ашир (حجاز كار كرد عاشير - Hijaz Kar Kurd Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сика Балди Курд (سيكاه بلدي كرد - Sikah Baladi Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Рамаль Курд (رمل كرد - Ramal Kurd)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Шадд Арабан Курд (شد عربان كرد - Shadd Araban Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Зангула Курд (زنكولة كرد - Zangula Kurd)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Махур Курд (ماهور كرد - Mahur Kurd)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Бузург Курд (بزرگ كرد - Buzurg Kurd)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Ирак Курд (عراق كرد - Iraq Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Исфахан Курд (أصفهان كرد - Isfahan Kurd)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Джигархан Курд (جكارخان كرد - Jigarkhan Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Харидж Хиджаз Курд (خارج حجاز كرد - Kharaj Hijaz Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Фарахфеза Курд (فرحفزا كرد - Farahfaza Kurd)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Шуштар Курд (شوشتر كرد - Shushtar Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Накш Курд (نقش كرد - Naqsh Kurd)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Ашир (بياتي عاشير - Bayati Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Ашир Курд (حجاز عاشير كرد - Hijaz Ashir Kurd)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сузнак Ашир (سوزناك عاشير - Suznak Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Раст Ашир (راست عاشير - Rast Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сика Хузам Ашир (سيكاه هزام عاشير - Sikah Huzam Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Нахаванд Ашир (نهاوند عاشير - Nahawand Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Курд Ашир (كرد عاشير - Kurd Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Адшам Мурраса Ашир (عجم مرصع عاشير - Ajam Murrasa Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Саба Далансин Ашир (صبا دلنشين عاشير - Saba Dalansin Ashir)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Хуззам Ашир (هزام عاشير - Huzzam Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Бастанигар Ашир (بستنكار عاشير - Bastanigar Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Шахназ Ашир (شهناز عاشير - Shahnaz Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Рахат аль-Арвах Ашир (راحة الأرواح عاشير - Rahat al-Arwah Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Нихавент Ашир (نهاوند عاشير - Nihavent Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Хусейни Ашир (حسيني عاشير - Husayni Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Шури Ашир (بياتي شوري عاشير - Bayati Shuri Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Раст Панжгах Ашир (راست پنجگاه عاشير - Rast Panjgah Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Курдилихиджазкар Ашир (كرديلهجازكار عاشير - Kurdilihijazkar Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сабаземземе Ашир (صبا زمزمة عاشير - Saba Zemzeme Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Навва Ашир (حجاز نوا عاشير - Hijaz Nawah Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Адшам Ашир Ашир (عجم عاشير عاشير - Ajam Ashir Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Суздиль Ашир (سوزديل عاشير - Suzidil Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Султанийегях Ашир (سلطاني يگاه عاشير - Sultaniyegah Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Нахаванд Хиджаз Ашир (نهاوند حجاز عاشير - Nahawand Hijaz Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Хиджаз Кар Ашир (حجاز كار عاشير - Hijaz Kar Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сика Балди Ашир (سيكاه بلدي عاشير - Sikah Baladi Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Рамаль Ашир (رمل عاشير - Ramal Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Шадд Арабан Ашир (شد عربان عاشير - Shadd Araban Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Зангула Ашир (زنكولة عاشير - Zangula Ashir)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Махур Ашир (ماهور عاشير - Mahur Ashir)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Бузург Ашир (بزرگ عاشير - Buzurg Ashir)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Ирак Ашир (عراق عاشير - Iraq Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Исфахан Ашир (أصفهان عاشير - Isfahan Ashir)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Джигархан Ашир (جكارخان عاشير - Jigarkhan Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Харидж Хиджаз Ашир (خارج حجاز عاشير - Kharaj Hijaz Ashir)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Фарахфеза Ашир (فرحفزا عاشير - Farahfaza Ashir)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Шуштар Ашир (شوشتر عاشير - Shushtar Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Накш Ашир (نقش عاشير - Naqsh Ashir)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Хиджаз (بياتي حجاز - Bayati Hijaz)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Хиджаз Сика (حجاز سيكاه - Hijaz Sikah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сузнак Сика (سوزناك سيكاه - Suznak Sikah)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Раст Сика (راست سيكاه - Rast Sikah)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сика Нахаванд (سيكاه نهاوند - Sikah Nahawand)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Нахаванд Сика (نهاوند سيكاه - Nahawand Sikah)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Курд Сика (كرد سيكاه - Kurd Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Адшам Сика (عجم سيكاه - Ajam Sikah)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Саба Сика (صبا سيكاه - Saba Sikah)", new List<int> {1, 2, 1, 2, 2, 2, 2}},
            {"Хуззам Сика (هزام سيكاه - Huzzam Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Бастанигар Сика (بستنكار سيكاه - Bastanigar Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Шахназ Сика (شهناز سيكاه - Shahnaz Sikah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Рахат аль-Арвах Сика (راحة الأرواح سيكاه - Rahat al-Arwah Sikah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Нихавент Сика (نهاوند سيكاه - Nihavent Sikah)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Хусейни Сика (حسيني سيكاه - Husayni Sikah)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Баяти Шури Сика (بياتي شوري سيكاه - Bayati Shuri Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Раст Панжгах Сика (راست پنجگاه سيكاه - Rast Panjgah Sikah)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Курдилихиджазкар Сика (كرديلهجازكار سيكاه - Kurdilihijazkar Sikah)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Сабаземземе Сика (صبا زمزمة سيكاه - Saba Zemzeme Sikah)", new List<int> {1, 2, 2, 2, 1, 2, 2}},
            {"Хиджаз Навва Сика (حجاز نوا سيكاه - Hijaz Nawah Sikah)", new List<int> {1, 3, 1, 2, 1, 2, 2}}
        }
    },
    // Категория: Европейские/Фольклорные
    {
        "Европейские/Фольклорные", new Dictionary<string, List<int>>
        {
            {"Испанский фригийский (Spanish Phrygian)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Балканская (Balkan)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Цыганская (Gypsy)", new List<int> {1, 3, 2, 1, 1, 3, 1}},
            {"Кельтская (Celtic)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Фламенко (Flamenco)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Гавайская (Hawaiian)", new List<int> {2, 1, 2, 2, 3, 2}},
            {"Румынская (Romanian Minor)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Ирландская (Irish)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Северная (Nordic)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Шотландская (Scottish)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Польская (Mazurka)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Русская (Russian Minor)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Испанская (Andalusian)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Тайская (Thai)", new List<int> {2, 2, 1, 2, 3, 2}},
            {"Чешская (Bohemian)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Литовская (Lithuanian)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Французская (Chanson)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Украинская (Ukrainian)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Бирманская (Burmese)", new List<int> {2, 2, 2, 3, 3}},
            {"Португальская (Fado)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Армянская (Armenian)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Грузинская (Georgian)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Непальская (Nepali)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Исландская (Icelandic)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Болгарская (Bulgarian)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Итальянская (Tarantella)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Белорусская (Belarusian)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Финская (Finnish)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Сербийская (Serbian)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Эстонская (Estonian)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Норвежская (Norwegian)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Хорватская (Croatian)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Галисийская (Galician)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Словацкая (Slovak)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Валлийская (Welsh)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Баскская (Basque)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Македонская (Macedonian)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Латвийская (Latvian)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Саамская (Sami)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Каталонская (Catalan)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Сардинская (Sardinian)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Албанская (Çifteli)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Бретонская (Breizh)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Словенская (Slovenska)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Корсиканская (Corsican)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Швейцарская (Alpine)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Молдавская (Moldovan)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Фламандская (Flemish)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Лапландская (Lappish)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Сицилийская (Sicilian)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Боснийская (Bosnian)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Критская (Cretan)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Датская (Danish)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Австрийская (Tyrolean)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Гэльская (Gaelic)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Кипрская (Cypriot)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            // Новые лады (70)
            {"Греческая (Chromatic Dorian)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Венгерская (Verbunkos)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Румынская (Doina)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Ирландская (Sean-nós)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Шведская (Polska)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Польская (Oberek)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Русская (Bylina)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Испанская (Malagueña)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Итальянская (Canzone)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Французская (Musette)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Украинская (Duma)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Болгарская (Rachenitsa)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Сербская (Kolo)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Хорватская (Tamburica)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Галисийская (Muiñeira)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Словацкая (Čardáš)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Валлийская (Hiraeth)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Баскская (Zortziko)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Македонская (Oro)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Латвийская (Daina)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Саамская (Joik)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Каталонская (Sardana)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Сардинская (Cantu)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Албанская (Iso-Polyphony)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Бретонская (Kan Ha Diskan)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Словенская (Ljudska)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Корсиканская (Paghjella)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Швейцарская (Jodel)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Молдавская (Horo)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Фламандская (Volkslied)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Лапландская (Reindeer)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Сицилийская (Siciliana)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Боснийская (Sevdalinka)", new List<int> {1, 2, 3, 1, 2, 2, 1}},
            {"Критская (Rizitiko)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Датская (Folkevise)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Австрийская (Ländler)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Гэльская (Puirt à Beul)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Кипрская (Tsiattista)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Немецкая (Volksmusik)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Шотландская (Pibroch)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Исландская (Rímur)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Эстонская (Regilaul)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Литовская (Sutartinė)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Финская (Kalevala)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Норвежская (Stev)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Белорусская (Kolyada)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Чешская (Lidová)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Португальская (Canto)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Грузинская (Chakrulo)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Армянская (Duduk)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Балканская (Izvorna)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Швейцарская (Ranz)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Словенская (Kajkavian)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Корсиканская (Lamentu)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Саамская (Leudd)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Каталонская (Havanera)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Сардинская (Tenore)", new List<int> {1, 3, 1, 2, 2, 2, 1}},
            {"Албанская (Këngë)", new List<int> {1, 3, 2, 1, 2, 2, 1}},
            {"Бретонская (Gwerz)", new List<int> {2, 1, 2, 2, 2, 2, 1}},
            {"Словенская (Ziljska)", new List<int> {2, 1, 3, 1, 2, 1, 2}},
            {"Корсиканская (Voceru)", new List<int> {1, 2, 2, 2, 1, 3, 1}},
            {"Швейцарская (Schwyzer)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Молдавская (Manea)", new List<int> {2, 1, 3, 1, 1, 3, 1}},
            {"Фламандская (Kempisch)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Лапландская (Vuolle)", new List<int> {2, 2, 2, 1, 2, 3}}
        }
    },
    // Категория: Африканские/Латиноамериканские
    {
        "Африканские/Латиноамериканские", new Dictionary<string, List<int>>
        {
            {"Алжирская (Algerian)", new List<int> {2, 1, 3, 1, 1, 2, 2}},
            {"Эфиопская (Ethiopian)", new List<int> {2, 2, 1, 2, 3, 2}},
            {"Африканская (Kora)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Кубинская (Cuban)", new List<int> {2, 2, 1, 2, 1, 3, 1}},
            {"Кенийская (Kenyan)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Африканская (Mbira)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Бразильская (Samba)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Карибская (Calypso)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Латино (Latin)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Нигерийская (Highlife)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Мексиканская (Mariachi)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Лаосская (Lao)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Малагасийская (Malagasy)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Венесуэльская (Joropo)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Ганская (Ghanaian)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Колумбийская (Cumbia)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танго (Tango)", new List<int> {2, 1, 2, 2, 1, 3, 1}},
            {"Филиппинская (Kundiman)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Южноафриканская (Zulu)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Перуанская (Huayno)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кубинская (Son)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Ямайская (Mento)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Африканская (Soukous)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Суданская (Nubian)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Боливийская (Saya)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Африканская (Pygmy)", new List<int> {2, 2, 2, 3, 3}},
            {"Гватемальская (Marimba)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Малийская (Griot)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Аргентинская (Chacarera)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Угандийская (Acholi)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Чилийская (Cueca)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сенегальская (Mbalax)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Эквадорская (Sanjuán)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Камерунская (Bikutsi)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Панамская (Tamborito)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танзанийская (Taarab)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Доминиканская (Merengue)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Зимбабвийская (Mbira)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Уругвайская (Candombe)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гвинейская (Kora)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Коста-Риканская (Punto)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Мозамбикская (Marrabenta)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            // Новые лады (70)
            {"Ганская (Adowa)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Аргентинская (Zamba)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кенийская (Benga)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Чилийская (Tonada)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сенегальская (Sabar)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Эквадорская (Pasillo)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Камерунская (Makosa)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Панамская (Mejorana)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танзанийская (Ngoma)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Доминиканская (Bachata)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Зимбабвийская (Chimurenga)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Уругвайская (Milonga)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гвинейская (Balafon)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Коста-Риканская (Calypso Limón)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Мозамбикская (Timbila)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Нигерийская (Juju)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Бразильская (Forró)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кубинская (Rumba)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Ямайская (Rocksteady)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Перуанская (Marinera)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Малийская (Wassoulou)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Аргентинская (Chamame)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кенийская (Ohangla)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Чилийская (Mapuche)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сенегальская (Wolof)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Эквадорская (Albazo)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Камерунская (Assiko)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Панамская (Cumbia Panameña)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танзанийская (Bajuni)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Доминиканская (Palo)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Зимбабвийская (Shona Mbira)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Уругвайская (Tango Criollo)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гвинейская (Mandinka)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Коста-Риканская (Guanacaste)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Мозамбикская (Chopi)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Ганская (Ewe)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Аргентинская (Payada)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кенийская (Luo)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Чилийская (Rapa Nui)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сенегальская (Serer)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Эквадорская (Yumbo)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Камерунская (Bamileke)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Панамская (Saloma)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танзанийская (Zaramo)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Доминиканская (Gaga)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Зимбабвийская (Ndebele)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Уругвайская (Pericón)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гвинейская (Susu)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Коста-Риканская (Siquirres)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Мозамбикская (Makua)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Нигерийская (Fuji)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Бразильская (Choro)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кубинская (Danzón)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Ямайская (Ska)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Перуанская (Festejo)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Малийская (Bambara)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Аргентинская (Carnavalito)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Кенийская (Chakacha)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Чилийская (Cueca Norteña)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Сенегальская (Tassu)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Эквадорская (Bomba)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Камерунская (Ambasse Bey)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Панамская (Punto Panameño)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Танзанийская (Mdundiko)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Доминиканская (Mangulina)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Зимбабвийская (Jerusarema)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Уругвайская (Cielito)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гвинейская (Fula)", new List<int> {2, 2, 2, 1, 2, 3}},
            {"Коста-Риканская (Tambito)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Мозамбикская (Tufo)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Африканская (Mande)", new List<int> {2, 2, 1, 2, 2, 3}},
            {"Африканская (Igbo)", new List<int> {2, 2, 2, 1, 3, 2}},
            {"Африканская (Ashanti)", new List<int> {2, 2, 1, 2, 3, 2}},
            {"Африканская (San)", new List<int> {3, 2, 2, 3, 2}},
            {"Африканская (Bakongo)", new List<int> {2, 1, 2, 2, 2, 3}}
        }
    },

    // Категория: Современные/Жанровые
    {
        "Современные/Жанровые", new Dictionary<string, List<int>>
        {
            {"Транс (Trance)", new List<int> {3, 1, 2, 2, 2, 1, 1}},
            {"Пситранс (PsyTrance)", new List<int> {3, 2, 1, 2, 1, 2, 1}},
            {"Синематик (Cinematic)", new List<int> {2, 2, 1, 3, 1, 2, 1}},
            {"Эмбиент (Ambient)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Драм-н-бейс (DnB)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Сайоми (Saiomy)", new List<int> {3, 1, 1, 3, 1, 2, 1}},
            {"Современная (Modern Fusion)", new List<int> {2, 1, 2, 1, 2, 1, 2}},
            {"Экспериментальная (Experimental)", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Техно (Techno)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Космическая (Space)", new List<int> {2, 2, 2, 1, 2, 1, 2}},
            {"Регги (Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Дабстеп (Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Минимализм (Minimal)", new List<int> {2, 2, 2, 2, 1, 2, 1}},
            {"Фьюжн (Fusion)", new List<int> {2, 1, 2, 1, 2, 2, 1}},
            {"Хип-хоп (Hip-Hop)", new List<int> {3, 2, 2, 1, 2, 2}},
            {"Брейкбит (Breakbeat)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Глитч (Glitch)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Чиллаут (Chillout)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Нью-эйдж (New Age)", new List<int> {2, 2, 2, 2, 1, 2, 1}},
            {"Рок (Rock)", new List<int> {3, 2, 2, 3, 2}},
            {"Диско (Disco)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Электро (Electro)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Индастриал (Industrial)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Даунтемпо (Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Эпик (Epic)", new List<int> {2, 2, 1, 3, 1, 2, 1}},
            {"Кантри (Country)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Ска (Ska)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Трип-хоп (Trip-Hop)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Гранж (Grunge)", new List<int> {3, 2, 2, 3, 2}},
            {"Дарк-эмбиент (Dark Ambient)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Симфонический (Symphonic)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Поп (Pop)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Соул (Soul)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Фанк (Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Хаус (House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Дрилл (Drill)", new List<int> {3, 2, 2, 1, 2, 2}},
            {"Трап (Trap)", new List<int> {3, 2, 2, 3, 2}},
            {"Ретро (Retro)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-фьюжн (Ethno Fusion)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Авангард (Avant-Garde)", new List<int> {1, 2, 1, 3, 1, 2, 2}},
            {"Барокко (Baroque)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Классический рок (Classic Rock)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Прогрессив (Progressive)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Метал (Metal)", new List<int> {3, 2, 2, 3, 2}},
            {"Дрим-поп (Dream Pop)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Лоу-фай (Lo-Fi)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Нойз (Noise)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Рагга (Ragga)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Госпел (Gospel)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-джаз (Ethno Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Синтвейв (Synthwave)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"R&B (R&B)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Блюз-рок (Blues Rock)", new List<int> {3, 2, 1, 1, 3, 2}},
            {"Альтернатива (Alternative)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Панк (Punk)", new List<int> {3, 2, 2, 3, 2}},
            {"Эмбиент-джаз (Ambient Jazz)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Хард-рок (Hard Rock)", new List<int> {3, 2, 2, 3, 2}},
            {"Джангл (Jungle)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Гараж (Garage)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Шугейз (Shoegaze)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Даб (Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-эмбиент (Ethno Ambient)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Техно-эмбиент (Techno Ambient)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Нью-скул (New School)", new List<int> {3, 2, 2, 1, 2, 2}},
            {"Прогрессив-хаус (Progressive House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Нью-джаз (Nu Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Бразильский фонк (Brazilian Phonk)", new List<int> {3, 2, 2, 1, 2, 2}},
            {"Гиперпоп (Hyperpop)", new List<int> {2, 1, 2, 1, 2, 1, 3}},
            {"Вейпорвейв (Vaporwave)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Чиллстеп (Chillstep)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фьюжн-рок (Fusion Rock)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Техно-поп (Techno Pop)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Этно-рок (Ethno Rock)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Нью-метал (Nu Metal)", new List<int> {3, 2, 2, 3, 2}},
            {"Дарквейв (Darkwave)", new List<int> {1, 2, 2, 1, 2, 2, 2}},
            {"Синти-поп (Synth Pop)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-даб (Ethno Dub)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Техно-фанк (Techno Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Глитч-хоп (Glitch Hop)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Прогрессив-метал (Progressive Metal)", new List<int> {3, 2, 1, 2, 2, 2}},
            {"Чилл-хаус (Chill House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Этно-транс (Ethno Trance)", new List<int> {3, 1, 2, 2, 1, 2, 1}},
            {"Нью-кантри (New Country)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Дарк-джаз (Dark Jazz)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Фонк (Phonk)", new List<int> {3, 2, 2, 1, 2, 2}},
            {"Этно-поп (Ethno Pop)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Техно-дабстеп (Techno Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Синти-фанк (Synth Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Глитч-рок (Glitch Rock)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Прогрессив-фанк (Progressive Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дрим-рок (Dream Rock)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Нью-фолк (New Folk)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Этно-драм (Ethno Drum)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Техно-регги (Techno Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Чилл-фонк (Chill Phonk)", new List<int> {3, 2, 2, 1, 2, 2}},
            // Новые лады (70)
            {"Дарк-хаус (Dark House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Нью-диско (New Disco)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-техно (Ethno Techno)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Прогрессив-джаз (Progressive Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Синти-рок (Synth Rock)", new List<int> {3, 2, 2, 3, 2}},
            {"Глитч-фанк (Glitch Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Чилл-даб (Chill Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Техно-фьюжн (Techno Fusion)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Нью-регги (New Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-трип (Ethno Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Дрим-хаус (Dream House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Прогрессив-рок (Progressive Rock)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Синти-джаз (Synth Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Глитч-дабстеп (Glitch Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Чилл-рок (Chill Rock)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Этно-даунтемпо (Ethno Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Техно-трип (Techno Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Нью-блюз (New Blues)", new List<int> {3, 2, 1, 1, 3, 2}},
            {"Дрим-фанк (Dream Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Прогрессив-даб (Progressive Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Синти-эмбиент (Synth Ambient)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Глитч-джаз (Glitch Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Чилл-регги (Chill Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Этно-глиц (Ethno Glitch)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Техно-рок (Techno Rock)", new List<int> {3, 2, 2, 3, 2}},
            {"Нью-поп (New Pop)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Дрим-даб (Dream Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Прогрессив-эмбиент (Progressive Ambient)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Синти-дабстеп (Synth Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Глитч-регги (Glitch Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Чилл-джаз (Chill Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Этно-фанк (Ethno Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Техно-даунтемпо (Techno Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Нью-рок (New Rock)", new List<int> {3, 2, 2, 3, 2}},
            {"Дрим-трип (Dream Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Прогрессив-регги (Progressive Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Синти-трип (Synth Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Глитч-эмбиент (Glitch Ambient)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Чилл-фьюжн (Chill Fusion)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Этно-джангл (Ethno Jungle)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Техно-джаз (Techno Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Нью-даб (New Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Дрим-джаз (Dream Jazz)", new List<int> {2, 1, 2, 2, 1, 1, 2}},
            {"Прогрессив-фолк (Progressive Folk)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Синти-фолк (Synth Folk)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Глитч-трип (Glitch Trip)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Чилл-эмбиент (Chill Ambient)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
            {"Этно-хаус (Ethno House)", new List<int> {2, 2, 1, 2, 2, 1, 2}},
            {"Техно-фолк (Techno Folk)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Нью-трип (New Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Дрим-фолк (Dream Folk)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Прогрессив-даунтемпо (Progressive Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Синти-даунтемпо (Synth Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Глитч-фолк (Glitch Folk)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Чилл-трип (Chill Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Этно-дабстеп (Ethno Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Техно-трип-хоп (Techno Trip-Hop)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Нью-фанк (New Funk)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дрим-даунтемпо (Dream Downtempo)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Прогрессив-джангл (Progressive Jungle)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Синти-регги (Synth Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Глитч-даунтемпо (Glitch Downtempo)", new List<int> {1, 2, 1, 2, 2, 1, 3}},
            {"Чилл-дабстеп (Chill Dubstep)", new List<int> {3, 1, 2, 1, 2, 2, 1}},
            {"Этно-фолк (Ethno Folk)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Техно-даб (Techno Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Нью-джангл (New Jungle)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Дрим-регги (Dream Reggae)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Прогрессив-трип (Progressive Trip)", new List<int> {2, 1, 2, 2, 1, 2, 2}},
            {"Синти-даб (Synth Dub)", new List<int> {2, 2, 1, 2, 2, 2, 1}},
            {"Глитч-джангл (Glitch Jungle)", new List<int> {2, 2, 1, 2, 1, 2, 2}},
            {"Чилл-фолк (Chill Folk)", new List<int> {2, 1, 2, 2, 2, 1, 2}},
            {"Космический минор (Cosmic Minor)", new List<int> {2, 1, 3, 1, 2, 2, 1}},
            {"Глитч-тон (Glitch Tone)", new List<int> {1, 3, 1, 2, 1, 2, 2}},
            {"Дримвейв (Dreamwave)", new List<int> {2, 2, 2, 1, 2, 2, 1}},
            {"Нойз-кор (Noise Core)", new List<int> {1, 2, 1, 2, 1, 3, 2}},
            {"Техно-микс (Techno Mix)", new List<int> {2, 1, 2, 1, 3, 2, 1}}
        }

    },
    {
    "Микротональные лады", new Dictionary<string, List<int>>
    {
        {"Бохлен-Пирс (Bohlen-Pierce)", new List<int> {3, 2, 3, 2, 2}},
        {"Гамма 19-ET (19-Tone Equal Temperament)", new List<int> {2, 1, 2, 1, 2, 2, 2}},
        {"Квартальный лад (Quarter-Tone)", new List<int> {1, 2, 1, 3, 1, 2, 2}}
    }
},
            };

            // Сопоставление интервалов с аккордами для известных ладов
            var scaleChordMappings = new Dictionary<string, Tuple<string[], string[]>>
            {
                {"Мажор (Ionian)", Tuple.Create(
                    new string[] { "I", "ii", "iii", "IV", "V", "vi", "vii°" },
                    new string[] { "maj", "min", "min", "maj", "maj", "min", "dim" }
                )},
                {"Натуральный минор (Aeolian)", Tuple.Create(
                    new string[] { "i", "ii°", "III", "iv", "v", "VI", "VII" },
                    new string[] { "min", "dim", "maj", "min", "min", "maj", "maj" }
                )},
                {"Гармонический минор", Tuple.Create(
                    new string[] { "i", "ii°", "III+", "iv", "V", "VI", "vii°" },
                    new string[] { "min", "dim", "aug", "min", "maj", "maj", "dim" }
                )},
                {"Мелодический минор (Asc.)", Tuple.Create(
                    new string[] { "i", "ii", "III+", "IV", "V", "vi°", "vii°" },
                    new string[] { "min", "min", "aug", "maj", "maj", "dim", "dim" }
                )},
                {"Дорийский (Dorian)", Tuple.Create(
                    new string[] { "i", "ii", "III", "IV", "v", "vi°", "VII" },
                    new string[] { "min", "min", "maj", "maj", "min", "dim", "maj" }
                )},
                {"Фригийский (Phrygian)", Tuple.Create(
                    new string[] { "i", "II", "III", "iv", "v°", "VI", "vii" },
                    new string[] { "min", "maj", "maj", "min", "dim", "maj", "min" }
                )},
                {"Лидийский (Lydian)", Tuple.Create(
                    new string[] { "I", "II", "iii", "#iv°", "V", "vi", "vii" },
                    new string[] { "maj", "maj", "min", "dim", "maj", "min", "min" }
                )},
                {"Миксолидийский (Mixolyd.)", Tuple.Create(
                    new string[] { "I", "ii", "iii°", "IV", "v", "vi", "VII" },
                    new string[] { "maj", "min", "dim", "maj", "min", "min", "maj" }
                )},
                {"Локрийский (Locrian)", Tuple.Create(
                    new string[] { "i°", "II", "iii", "iv", "V", "VI", "vii" },
                    new string[] { "dim", "maj", "min", "min", "maj", "maj", "min" }
                )},
                {"Мажорная пентатоника", Tuple.Create(
                    new string[] { "I", "II", "III", "V", "VI" },
                    new string[] { "maj", "maj", "maj", "maj", "maj" }
                )},
                {"Минорная пентатоника", Tuple.Create(
                    new string[] { "i", "III", "IV", "V", "VII" },
                    new string[] { "min", "min", "min", "min", "min" }
                )},
                {"Блюзовая (Blues)", Tuple.Create(
                    new string[] { "i", "III", "IV", "bV", "V", "VII" },
                    new string[] { "min", "maj", "min", "dim", "min", "maj" }
                )},
            };

            foreach (var category in bigScalePatterns)
            {
                var scales = new Dictionary<string, Tuple<string[], string[]>>();
                foreach (var scale in category.Value)
                {
                    if (scaleChordMappings.ContainsKey(scale.Key))
                    {
                        scales[scale.Key] = scaleChordMappings[scale.Key];
                    }
                    else
                    {
                        // Автоматическое построение ступеней и качеств
                        scales[scale.Key] = GenerateChordsFromIntervals(scale.Key, scale.Value);
                    }
                }
                result[category.Key] = scales;
            }

            return result;
        }

        // Метод для автоматического построения аккордов на основе интервалов
        private static Tuple<string[], string[]> GenerateChordsFromIntervals(string scaleName, List<int> intervals)
        {
            // Преобразуем интервалы в абсолютные позиции нот (относительно тоники = 0)
            var absoluteIntervals = new List<int> { 0 };
            for (int i = 0; i < intervals.Count; i++)
            {
                absoluteIntervals.Add(absoluteIntervals[i] + intervals[i]);
            }

            int noteCount = absoluteIntervals.Count - 1; // Количество нот в ладу (без октавы)
            var degrees = new List<string>();
            var qualities = new List<string>();

            // Генерируем римские цифры для каждой ступени
            for (int i = 0; i < noteCount; i++)
            {
                degrees.Add(GetRomanNumeral(i + 1, absoluteIntervals[i] % 12 >= 6));
            }

            // Определяем качество аккордов для каждой ступени
            for (int i = 0; i < noteCount; i++)
            {
                int root = absoluteIntervals[i] % 12;
                int thirdIndex = (i + 2) % noteCount;
                int fifthIndex = (i + 4) % noteCount;
                int seventhIndex = (i + 6) % noteCount;

                // Учитываем октаву, если индексы выходят за пределы
                int third = absoluteIntervals[thirdIndex] % 12 + (thirdIndex < i ? 12 : 0);
                int fifth = absoluteIntervals[fifthIndex] % 12 + (fifthIndex < i ? 12 : 0);
                int seventh = absoluteIntervals[seventhIndex] % 12 + (seventhIndex < i ? 12 : 0);

                int thirdInterval = (third - root + 12) % 12;
                int fifthInterval = (fifth - root + 12) % 12;
                int seventhInterval = (seventh - root + 12) % 12;

                string quality;

                // Сначала проверяем триады
                if (thirdInterval == 4 && fifthInterval == 7)
                {
                    if (noteCount > 5 && seventhIndex < noteCount) // Проверяем наличие септимы
                    {
                        if (seventhInterval == 11)
                            quality = "maj7";
                        else if (seventhInterval == 10)
                            quality = "7";
                        else if (seventhInterval == 14 || seventhInterval == 2)
                            quality = "add9";
                        else
                            quality = "maj";
                    }
                    else
                        quality = "maj";
                }
                else if (thirdInterval == 3 && fifthInterval == 7)
                {
                    if (noteCount > 5 && seventhIndex < noteCount)
                    {
                        if (seventhInterval == 10)
                            quality = "m7";
                        else if (seventhInterval == 14 || seventhInterval == 2)
                            quality = "m9";
                        else
                            quality = "m";
                    }
                    else
                        quality = "m";
                }
                else if (thirdInterval == 3 && fifthInterval == 6)
                {
                    if (noteCount > 5 && seventhIndex < noteCount)
                    {
                        if (seventhInterval == 9)
                            quality = "dim7";
                        else if (seventhInterval == 10)
                            quality = "m7b5";
                        else
                            quality = "dim";
                    }
                    else
                        quality = "dim";
                }
                else if (thirdInterval == 4 && fifthInterval == 8)
                {
                    if (noteCount > 5 && seventhIndex < noteCount)
                    {
                        if (seventhInterval == 10)
                            quality = "aug7";
                        else
                            quality = "aug";
                    }
                    else
                        quality = "aug";
                }
                else if (thirdInterval == 5 && fifthInterval == 7)
                {
                    quality = "sus4";
                }
                else
                {
                    quality = "m"; // По умолчанию минор, если структура не распознана
                }

                qualities.Add(quality);
            }

            var result = Tuple.Create(degrees.ToArray(), qualities.ToArray());
            Log($"Сгенерированы аккорды для '{scaleName}': {string.Join(", ", degrees.Zip(qualities, (d, q) => $"{d}{q}"))}");
            return result;
        }

        // Метод для получения римских цифр
        private static string GetRomanNumeral(int number, bool isMinor)
        {
            string[] majorNumerals = { "I", "II", "III", "IV", "V", "VI", "VII" };
            string[] minorNumerals = { "i", "ii", "iii", "iv", "v", "vi", "vii" };

            if (number >= 1 && number <= 7)
                return isMinor ? minorNumerals[number - 1] : majorNumerals[number - 1];
            return "I"; // По умолчанию
        }

        private void InitializeTonalityComboBox()
        {
            cmbBoxTonality.Items.AddRange(new string[]
            {
                "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B",
                "Cm", "C#m", "Dm", "D#m", "Em", "Fm", "F#m", "Gm", "G#m", "Am", "A#m", "Bm"
            });
            cmbBoxTonality.SelectedIndex = 0;
        }

        private void InitializeCategoryComboBox()
        {
            cmbBoxCategory.Items.AddRange(scalesChordsByCategory.Keys.OrderBy(c => c).ToArray());
            if (cmbBoxCategory.Items.Count > 0)
                cmbBoxCategory.SelectedIndex = 0;
        }

        private void InitializeScaleComboBox()
        {
            UpdateScaleComboBox();
        }

        private void UpdateScaleComboBox()
        {
            cmbBoxScale.Items.Clear();
            string selectedCategory = cmbBoxCategory.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedCategory) && scalesChordsByCategory.TryGetValue(selectedCategory, out var scales))
            {
                foreach (var scale in scales.Keys.OrderBy(s => s))
                {
                    string style = "None";
                    bool isPsytrance = psytranceScales.Contains(scale);
                    bool isTrance = tranceScales.Contains(scale);

                    if (isPsytrance && isTrance)
                        style = "Psytrance, Trance";
                    else if (isPsytrance)
                        style = "Psytrance";
                    else if (isTrance)
                        style = "Trance";

                    cmbBoxScale.Items.Add(new ScaleItem { Name = scale, Style = style });
                }
            }

            if (cmbBoxScale.Items.Count > 0)
                cmbBoxScale.SelectedIndex = 0;

            cmbBoxScale.DisplayMember = "ToString";
        }

        private void CmbBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateScaleComboBox();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string selectedTonality = cmbBoxTonality.SelectedItem.ToString();
            string selectedCategory = cmbBoxCategory.SelectedItem.ToString();
            string selectedScale = "";

            if (cmbBoxScale.SelectedItem is ScaleItem selectedScaleItem)
            {
                selectedScale = selectedScaleItem.Name;
            }
            else
            {
                MessageBox.Show("Выберите скейл из списка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int maxUniqueChords;
            int numberOfChords;

            maxUniqueChords = (int)txtNumberOfChords.Value;
            numberOfChords = (int)txtProgressionLength.Value;

            if (numberOfChords < maxUniqueChords)
            {
                MessageBox.Show("Общее количество аккордов не может быть меньше количества уникальных аккордов.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!scalesChordsByCategory.ContainsKey(selectedCategory) || !scalesChordsByCategory[selectedCategory].ContainsKey(selectedScale))
            {
                MessageBox.Show($"Скейл '{selectedScale}' в категории '{selectedCategory}' не поддерживается.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var chordProgression = GenerateChordProgression(selectedTonality, selectedCategory, selectedScale, maxUniqueChords, numberOfChords);

                if (chordProgression.Count > 0)
                {
                    txtProgression.Text = string.Join(" - ", chordProgression);

                    // Выводим статистику по уникальным аккордам
                    int actualUniqueChords = chordProgression.Distinct().Count();
                    Log($"Запрошено уникальных аккордов: {maxUniqueChords}, фактически использовано: {actualUniqueChords}");

                    if (actualUniqueChords < maxUniqueChords && actualUniqueChords < scalesChordsByCategory[selectedCategory][selectedScale].Item1.Length)
                    {
                        MessageBox.Show($"Внимание! В прогрессии использовано {actualUniqueChords} уникальных аккордов вместо запрошенных {maxUniqueChords}.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    lastGeneratedMidiPath = SaveChordProgressionToMidi(chordProgression, selectedTonality, selectedScale);
                    _lastGeneratedMidiPath = lastGeneratedMidiPath;

                    btnPlayMIDI.Enabled = true;

                    MessageBox.Show("Прогрессия успешно сохранена на рабочий стол.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось сгенерировать прогрессию аккордов.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> GenerateChordProgression(string tonality, string category, string scale, int maxUniqueChords, int numberOfChords)
        {
            var progression = new List<string>();
            var scaleData = scalesChordsByCategory[category][scale];
            var degrees = scaleData.Item1;
            var qualities = scaleData.Item2;

            var allChords = new List<string>();
            for (int i = 0; i < degrees.Length; i++)
            {
                string degree = degrees[i];
                string quality = qualities[i];

                string chordName = GetChordName(tonality, degree, quality);

                if (!string.IsNullOrEmpty(chordName))
                {
                    allChords.Add(chordName);
                }
                else
                {
                    Log($"Не удалось получить имя аккорда для ступени: {degree}");
                }
            }

            if (allChords.Count == 0)
            {
                MessageBox.Show("Не удалось получить список аккордов для выбранного скейла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return progression;
            }

            // Проверка на ограничение количества доступных аккордов
            int originalMaxUniqueChords = maxUniqueChords;
            if (maxUniqueChords > allChords.Count)
            {
                maxUniqueChords = allChords.Count;
                MessageBox.Show($"Количество уникальных аккордов уменьшено с {originalMaxUniqueChords} до {maxUniqueChords}, так как в выбранной гамме доступно только {allChords.Count} аккордов.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Выбираем точное количество уникальных аккордов
            var selectedChordsSet = new HashSet<string>();
            while (selectedChordsSet.Count < maxUniqueChords)
            {
                int index = random.Next(allChords.Count);
                selectedChordsSet.Add(allChords[index]);
            }
            var selectedChords = selectedChordsSet.ToList();

            // Начинаем с включения всех уникальных аккордов в прогрессию в случайном порядке
            var shuffledSelectedChords = selectedChords.OrderBy(x => random.Next()).ToList();
            progression.AddRange(shuffledSelectedChords);

            // Если требуется больше аккордов, добавляем их случайным образом
            while (progression.Count < numberOfChords)
            {
                int index = random.Next(selectedChords.Count);
                string nextChord = selectedChords[index];

                // Избегаем трех одинаковых аккордов подряд
                if (progression.Count >= 2 &&
                    progression[progression.Count - 1] == nextChord &&
                    progression[progression.Count - 2] == nextChord)
                {
                    continue;
                }

                progression.Add(nextChord);
            }

            // Если нужно меньше аккордов, чем уникальных, берем только часть (этот случай маловероятен из-за проверки в UI)
            if (progression.Count > numberOfChords)
            {
                progression = progression.Take(numberOfChords).ToList();
            }

            return progression;
        }

        private string SaveChordProgressionToMidi(List<string> chordProgression, string tonality, string scale)
        {
            // Получаем путь к рабочему столу пользователя
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Формируем имя файла
            string chordsShort = string.Join("_", chordProgression.Take(4)).Replace("-", "").Replace(" ", "");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                chordsShort = chordsShort.Replace(c, '_');
            }

            string fileName = $"{tonality}_{scale}_{chordsShort}.mid";
            string fullPath = Path.Combine(desktopPath, fileName);

            // Создаем MIDI-файл
            var midiFile = new MidiEventCollection(0, 480);
            midiFile.AddEvent(new TempoEvent(500000, 0), 0);

            int chordChannel = 1;
            midiFile.AddEvent(new PatchChangeEvent(0, chordChannel, 0), 0); // Acoustic Grand Piano

            int ticksPerQuarter = 480;
            int beatsPerMeasure = 4;
            int measuresPerChord = 1; // Изменено с 8 на 1 - теперь каждый аккорд длится 1 такт
            int noteDuration = ticksPerQuarter * beatsPerMeasure * measuresPerChord;

            int currentTime = 0;
            int velocity = 100;

            // Список для визуализации в NoteDisplayControl - используем формат списка списков
            List<List<int>> chords = new List<List<int>>();
            List<int> durationsVis = new List<int>();

            // Заполняем список аккордов (первоначально пустыми списками)
            for (int i = 0; i < chordProgression.Count; i++)
            {
                chords.Add(new List<int>()); // Пустой список для каждой позиции аккорда
                durationsVis.Add(16); // Длительность 16 шестнадцатых = 1 такт
            }

            // Обработка каждого аккорда
            for (int i = 0; i < chordProgression.Count; i++)
            {
                string chord = chordProgression[i];
                var notes = GetMidiNotesForChord(chord);

                // Добавляем все ноты аккорда в MIDI-файл
                foreach (var note in notes)
                {
                    midiFile.AddEvent(new NoteOnEvent(currentTime, chordChannel, note, velocity, 0), 0);

                    // Добавляем ноту в соответствующий аккорд в списке
                    chords[i].Add(note);
                }

                // Добавляем события NoteOff для всех нот аккорда
                int noteOffTime = currentTime + noteDuration;
                foreach (var note in notes)
                {
                    midiFile.AddEvent(new NoteEvent(noteOffTime, chordChannel, MidiCommandCode.NoteOff, note, 0), 0);
                }

                currentTime += noteDuration;
            }

            midiFile.AddEvent(new MetaEvent(MetaEventType.EndTrack, 0, currentTime), 0);
            MidiFile.Export(fullPath, midiFile);

            // Визуализируем прогрессию в компоненте, если он инициализирован
            if (noteDisplayControl != null)
            {
                try
                {
                    // Используем метод SetChords, который уже есть в классе NoteDisplayControl
                    noteDisplayControl.SetChords(chords, durationsVis);

                    // Прокручиваем к началу
                    noteDisplayControl.ScrollToPosition(0);

                    // Если есть хотя бы один аккорд, прокручиваем к его первой ноте
                    if (chords.Count > 0 && chords[0].Count > 0)
                    {
                        noteDisplayControl.ScrollToNote(chords[0][0]);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но продолжаем выполнение
                    Log($"Ошибка при визуализации нот: {ex.Message}");
                }
            }

            return fullPath;
        }
        private int[] GetMidiNotesForChord(string chord)
        {
            if (string.IsNullOrEmpty(chord))
            {
                Log("Название аккорда пусто или null");
                return new int[0];
            }

            string rootNote = "";
            string chordType = "";

            if (char.IsLetter(chord[0]))
            {
                rootNote += chord[0];
                if (chord.Length > 1 && (chord[1] == '#' || chord[1] == 'b'))
                {
                    rootNote += chord[1];
                    if (chord.Length > 2)
                    {
                        chordType = chord.Substring(2).ToLower();
                    }
                }
                else
                {
                    if (chord.Length > 1)
                    {
                        chordType = chord.Substring(1).ToLower();
                    }
                }
            }
            else
            {
                Log($"Неизвестный формат аккорда: {chord}");
                return new int[0];
            }

            Log($"Разбор аккорда: {chord}, корень: {rootNote}, тип: {chordType}");

            int rootMidi = GetMidiNumber(rootNote);
            if (rootMidi == -1)
            {
                Log($"Неизвестная корневая нота: {rootNote}");
                return new int[0];
            }

            if (!chordFormulas.ContainsKey(chordType))
            {
                Log($"Неизвестный тип аккорда: {chordType}");
                return new int[0];
            }

            int[] intervals = chordFormulas[chordType];

            List<int> midiNotes = new List<int>();
            foreach (var interval in intervals)
            {
                int note = rootMidi + interval;

                int lowerLimit = 60;
                int upperLimit = lowerLimit + 24;

                while (note < lowerLimit)
                {
                    note += 12;
                }
                while (note >= upperLimit)
                {
                    note -= 12;
                }

                midiNotes.Add(note);
            }

            Log($"Аккорд {chord} MIDI ноты: {string.Join(", ", midiNotes)}");
            return midiNotes.ToArray();
        }

        private int GetMidiNumber(string note)
        {
            Dictionary<string, int> noteToMidi = new Dictionary<string, int>()
            {
                { "C", 60 }, { "C#", 61 }, { "Db", 61 }, { "D", 62 }, { "D#", 63 }, { "Eb", 63 },
                { "E", 64 }, { "F", 65 }, { "F#", 66 }, { "Gb", 66 }, { "G", 67 }, { "G#", 68 },
                { "Ab", 68 }, { "A", 69 }, { "A#", 70 }, { "Bb", 70 }, { "B", 71 }
            };

            return noteToMidi.ContainsKey(note) ? noteToMidi[note] : -1;
        }

        private string GetChordName(string tonality, string degree, string quality)
        {
            string root = tonality.EndsWith("m") ? tonality.Substring(0, tonality.Length - 1) : tonality;
            Log($"Тональность: {tonality}. Корень аккорда: {root}");

            int semitones = GetSemitonesFromDegree(degree);
            if (semitones == -1)
            {
                Log($"Не удалось определить количество полутонов для ступени: {degree}");
                return null;
            }

            string chordRoot = Transpose(root, semitones);
            if (string.IsNullOrEmpty(chordRoot))
            {
                Log($"Не удалось транспонировать корень: {root} на {semitones} полутонов");
                return null;
            }

            string chordName = chordRoot;

            switch (quality)
            {
                case "maj": break;
                case "min": chordName += "m"; break;
                case "dim": chordName += "dim"; break;
                case "aug": chordName += "aug"; break;
                case "sus4": chordName += "sus4"; break;
                case "add9": chordName += "add9"; break;
                case "7": chordName += "7"; break;
                case "maj7": chordName += "maj7"; break;
                case "m7": chordName += "m7"; break;
                case "dim7": chordName += "dim7"; break;
                case "m7b5": chordName += "m7b5"; break;
                case "m9": chordName += "m9"; break;
                case "maj9": chordName += "maj9"; break;
                case "9": chordName += "9"; break;
                case "aug7": chordName += "aug7"; break;
                case "5": chordName += "5"; break;
                case "+": chordName += "+"; break;
                case "°": chordName += "°"; break;
                default: chordName += quality; break;
            }

            Log($"Сгенерирован аккорд: {chordName}");
            return chordName;
        }

        private int GetSemitonesFromDegree(string degree)
        {
            int semitones = 0;
            int accidental = 0;

            string degreeWithoutSuffix = degree.TrimEnd('°', '+');
            string pattern = @"^([b#]?)(i{1,3}|v{1,3}|iv|vi{0,2}|vii)$";
            var match = Regex.Match(degreeWithoutSuffix.ToLower(), pattern);

            if (!match.Success)
            {
                Log($"Неизвестная ступень: {degree}");
                return -1;
            }

            string accidentalSymbol = match.Groups[1].Value;
            string romanNumeral = match.Groups[2].Value;

            if (accidentalSymbol == "b")
                accidental = -1;
            else if (accidentalSymbol == "#")
                accidental = 1;

            switch (romanNumeral.ToLower())
            {
                case "i": semitones = 0; break;
                case "ii": semitones = 2; break;
                case "iii": semitones = 4; break;
                case "iv": semitones = 5; break;
                case "v": semitones = 7; break;
                case "vi": semitones = 9; break;
                case "vii": semitones = 11; break;
                default: Log($"Неизвестная ступень: {degree}"); return -1;
            }

            semitones += accidental;
            semitones = (semitones + 12) % 12;

            return semitones;
        }

        private string Transpose(string note, int semitones)
        {
            string[] notesSharp = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string[] notesFlat = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };

            int index = Array.IndexOf(notesSharp, note);
            bool isSharp = true;
            if (index == -1)
            {
                index = Array.IndexOf(notesFlat, note);
                isSharp = false;
                if (index == -1)
                {
                    Log($"Неизвестная нота для транспонирования: {note}");
                    return "C";
                }
            }

            int newIndex = (index + semitones) % 12;
            if (newIndex < 0) newIndex += 12;

            return isSharp ? notesSharp[newIndex] : notesFlat[newIndex];
        }

        private static void Log(string message)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            string logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";
            try
            {
                File.AppendAllText(logPath, logMessage, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось записать лог: {ex.Message}", "Ошибка логирования", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ Методы для сохранения/восстановления состояния (для VST) ============

        public event EventHandler StateChanged;

        public int GetCategoryIndex()
        {
            return cmbBoxCategory.SelectedIndex;
        }

        public void SetCategoryIndex(int index)
        {
            if (index >= 0 && index < cmbBoxCategory.Items.Count)
            {
                cmbBoxCategory.SelectedIndex = index;
                OnStateChanged();
            }
        }

        public int GetScaleIndex()
        {
            return cmbBoxScale.SelectedIndex;
        }

        public void SetScaleIndex(int index)
        {
            if (index >= 0 && index < cmbBoxScale.Items.Count)
            {
                cmbBoxScale.SelectedIndex = index;
                OnStateChanged();
            }
        }

        public int GetTonicIndex()
        {
            return cmbBoxTonality.SelectedIndex;
        }

        public void SetTonicIndex(int index)
        {
            if (index >= 0 && index < cmbBoxTonality.Items.Count)
            {
                cmbBoxTonality.SelectedIndex = index;
                OnStateChanged();
            }
        }

        public int GetPadNotesCount()
        {
            return (int)txtNumberOfChords.Value;
        }

        public void SetPadNotesCount(int value)
        {
            txtNumberOfChords.Value = value;
            OnStateChanged();
        }

        public int GetTactsNumber()
        {
            return (int)txtProgressionLength.Value;
        }

        public void SetTactsNumber(int value)
        {
            txtProgressionLength.Value = value;
            OnStateChanged();
        }

        public int GetRepeatsNumber()
        {
            // PADGenerator не имеет поля повторов, возвращаем 0
            return 0;
        }

        public void SetRepeatsNumber(int value)
        {
            // PADGenerator не имеет поля повторов, ничего не делаем
        }

        public string GetGeneratedText()
        {
            return txtProgression.Text;
        }

        public void SetGeneratedText(string text)
        {
            txtProgression.Text = text;
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        // ============ Drag and Drop MIDI в DAW ============

        private void BtnDragMidi_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !string.IsNullOrEmpty(lastGeneratedMidiPath))
            {
                if (File.Exists(lastGeneratedMidiPath))
                {
                    // Создаем DataObject с путём к MIDI файлу
                    DataObject data = new DataObject();
                    data.SetData(DataFormats.FileDrop, new string[] { lastGeneratedMidiPath });
                    
                    // Запускаем операцию drag-and-drop
                    // Важно: используем btnPlayMIDI, а не sender, чтобы кнопка не двигалась
                    DragDropEffects result = btnPlayMIDI.DoDragDrop(data, DragDropEffects.Copy);
                    
                    if (result == DragDropEffects.Copy)
                    {
                        txtProgression.AppendText("\r\n✓ MIDI файл перетащен в DAW!");
                    }
                }
                else
                {
                    MessageBox.Show("Сначала сгенерируйте MIDI файл!", "Предупреждение", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnPlayInDAW_Click(object sender, EventArgs e)
        {
            if (_plugin == null || _plugin.MidiProcessor == null)
            {
                MessageBox.Show("Плагин не инициализирован!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var midiPlayer = _plugin.MidiProcessor.MidiPlayer;

            if (midiPlayer.IsPlaying)
            {
                midiPlayer.Stop();
                ((System.Windows.Forms.Button)sender).Text = "▶ Воспроизвести в DAW";
                txtProgression.AppendText("\r\n⏹ Воспроизведение остановлено");
            }
            else
            {
                if (!string.IsNullOrEmpty(_lastGeneratedMidiPath) && File.Exists(_lastGeneratedMidiPath))
                {
                    try
                    {
                        midiPlayer.LoadMidiFile(_lastGeneratedMidiPath);
                        midiPlayer.SetSampleRate(44100);
                        midiPlayer.SetLoop(true);
                        midiPlayer.Play();
                        ((System.Windows.Forms.Button)sender).Text = "⏹ Остановить";
                        txtProgression.AppendText($"\r\n▶ Воспроизведение начато: {Path.GetFileName(_lastGeneratedMidiPath)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при воспроизведении MIDI: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"MIDI файл не найден!\nПуть: {_lastGeneratedMidiPath ?? "не задан"}", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}