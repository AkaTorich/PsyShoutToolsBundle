//Form1.cs - Полностью исправленная версия
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PsyShoutLauncher.Properties; // Для доступа к ресурсам

namespace PsyShoutLauncher
{
    public partial class Form1 : Form
    {
        // Объект для вывода подсказок
        private ToolTip toolTip;

        // Базовый путь к исполняемым файлам (относительно расположения лаунчера)
        private readonly string basePath;

        // Список всех доступных утилит в ОРИГИНАЛЬНОМ порядке из твоего проекта
        private readonly List<ToolInfo> tools = new List<ToolInfo>
        {
            new ToolInfo("ARPGenerator.exe", "ARP Generator - Генератор арпеджио", 1),
            new ToolInfo("AudioConverter.exe", "Audio Converter - Конвертер аудио форматов", 2),
            new ToolInfo("AudioCutter.exe", "Audio Cutter - Обрезка аудио файлов", 3),
            new ToolInfo("DreamDataManager.exe", "Dream Data Manager - Дневник снов", 4),
            new ToolInfo("ImageResizer.exe", "Image Resizer - Изменение размера изображений", 5),
            new ToolInfo("LayoutConverter.exe", "Layout Converter - Конвертер раскладок", 6),
            new ToolInfo("MediciGet.exe", "Medici Get - Гадание медичи", 7),
            new ToolInfo("PADGenerator.exe", "PAD Generator - Генератор атмосферных пэдов", 8),
            new ToolInfo("PomedoroWorker.exe", "Pomodoro Worker - Таймер продуктивности", 9),
            new ToolInfo("ProjectManager.exe", "Project Manager - Менеджер проектов", 10),
            new ToolInfo("ScaleSelector.exe", "Scale Selector - Выбор музыкальных гамм", 11),
            new ToolInfo("TaskMan.exe", "Task Manager - Системный диспетчер задач", 12),
            new ToolInfo("AmAwake.exe", "AmAwake - Напоминание для пользователя", 13),
            new ToolInfo("BrainWaveGenerator.exe", "Brain Wave Generator - Генератор бинауральных ритмов", 14),
            new ToolInfo("IChing.exe", "I-Ching - Гадание И-Цзин", 15),
            new ToolInfo("TasksReminder.exe", "Tasks Reminder - Напоминания о задачах", 16),
            new ToolInfo("PlayOnMe.exe", "Play On Me - Аудио плеер", 17),
            new ToolInfo("Archiver.exe", "Archiver - Архивирование с шифрованием", 18),
            new ToolInfo("MailClient.exe", "Mail Client - Почтовый клиент", 19),
            new ToolInfo("NoteTray.exe", "Note Tray - Заметки в системном трее", 20),
            new ToolInfo("BASSGenerator.exe", "BASS Generator - Генератор басовых линий", 21),
            new ToolInfo("BpmKeyDetector.exe", "BPM Key Detector - Анализатор темпа и тональности", 22),
            new ToolInfo("BackupManager.exe", "Backup Manager - Менеджер резервных копий", 23),
            new ToolInfo("WavTagEditor.exe", "WAV Tag Editor - Редактор тегов аудио", 24)
        };

        public Form1()
        {
            // Определяем базовый путь (папка с лаунчером)
            basePath = Path.GetDirectoryName(Application.ExecutablePath) ?? Environment.CurrentDirectory;

            InitializeComponent();
            InitializeToolTips();
            AssignButtonIcons();
            CheckToolAvailability();
        }

        /// <summary>
        /// Инициализация подсказок для кнопок
        /// </summary>
        private void InitializeToolTips()
        {
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // Назначаем подсказки для кнопок на основе списка инструментов
            for (int i = 0; i < Math.Min(tools.Count, 24); i++)
            {
                var button = GetButtonByIndex(i + 1);
                if (button != null && i < tools.Count)
                {
                    toolTip.SetToolTip(button, tools[i].Description);
                }
            }
        }

        /// <summary>
        /// Назначение иконок кнопкам
        /// </summary>
        private void AssignButtonIcons()
        {
            // Назначаем иконки только тем кнопкам, для которых есть ресурсы
            for (int i = 0; i < Math.Min(tools.Count, 24); i++)
            {
                var button = GetButtonByIndex(i + 1);
                if (button != null)
                {
                    Icon icon = GetIconForButton(i + 1);
                    AssignButtonIcon(button, icon);
                }
            }
        }

        /// <summary>
        /// Получает иконку для кнопки по её номеру
        /// </summary>
        /// <param name="buttonNumber">Номер кнопки (1-24)</param>
        /// <returns>Объект Icon или null</returns>
        private Icon GetIconForButton(int buttonNumber)
        {
            try
            {
                // Используем все 24 иконки
                switch (buttonNumber)
                {
                    case 1: return Resources.Icon1;
                    case 2: return Resources.Icon2;
                    case 3: return Resources.Icon3;
                    case 4: return Resources.Icon4;
                    case 5: return Resources.Icon5;
                    case 6: return Resources.Icon6;
                    case 7: return Resources.Icon7;
                    case 8: return Resources.Icon8;
                    case 9: return Resources.Icon9;
                    case 10: return Resources.Icon10;
                    case 11: return Resources.Icon11;
                    case 12: return Resources.Icon12;
                    case 13: return Resources.Icon13;
                    case 14: return Resources.Icon14;
                    case 15: return Resources.Icon15;
                    case 16: return Resources.Icon16;
                    case 17: return Resources.Icon17;
                    case 18: return Resources.Icon18;
                    case 19: return Resources.Icon19;
                    case 20: return Resources.Icon20;
                    case 21: return Resources.Icon21;
                    case 22: return Resources.Icon22;
                    case 23: return Resources.Icon23;
                    case 24: return Resources.Icon24;
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения иконки для кнопки {buttonNumber}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Проверка доступности инструментов
        /// </summary>
        private void CheckToolAvailability()
        {
            for (int i = 0; i < Math.Min(tools.Count, 24); i++)
            {
                var button = GetButtonByIndex(i + 1);
                var tool = tools[i];

                if (button != null)
                {
                    string fullPath = Path.Combine(basePath, tool.FileName);
                    if (!File.Exists(fullPath))
                    {
                        button.Enabled = false;
                        button.BackColor = Color.LightGray;
                        toolTip.SetToolTip(button, $"{tool.Description}\n(Файл не найден: {tool.FileName})");
                    }
                }
            }
        }

        /// <summary>
        /// Получение кнопки по индексу
        /// </summary>
        /// <param name="index">Индекс кнопки (1-24)</param>
        /// <returns>Объект кнопки или null</returns>
        private Button GetButtonByIndex(int index)
        {
            switch (index)
            {
                case 1: return button1;
                case 2: return button2;
                case 3: return button3;
                case 4: return button4;
                case 5: return button5;
                case 6: return button6;
                case 7: return button7;
                case 8: return button8;
                case 9: return button9;
                case 10: return button10;
                case 11: return button11;
                case 12: return button12;
                case 13: return button13;
                case 14: return button14;
                case 15: return button15;
                case 16: return button16;
                case 17: return button17;
                case 18: return button18;
                case 19: return button19;
                case 20: return button20;
                case 21: return button21;
                case 22: return button22;
                case 23: return button23;
                case 24: return button24;
                default: return null;
            }
        }

        /// <summary>
        /// Назначает иконку кнопке.
        /// </summary>
        /// <param name="button">Кнопка, которой назначается иконка.</param>
        /// <param name="icon">Иконка для кнопки.</param>
        private void AssignButtonIcon(Button button, Icon icon)
        {
            if (icon != null)
            {
                // Масштабирование иконки до 28x28 пикселей (меньше для 24 кнопок)
                Bitmap scaledIcon = new Bitmap(icon.ToBitmap(), new Size(28, 28));

                // Назначаем иконку через свойство Image
                button.Image = scaledIcon;
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.Text = ""; // Удаляем текст, если он не нужен
                button.TextImageRelation = TextImageRelation.ImageAboveText;

                Debug.WriteLine($"Иконка для {button.Name} успешно назначена.");
            }
            else
            {
                button.Text = "?";
                button.Font = new Font("Arial", 14, FontStyle.Bold);
                Debug.WriteLine($"Иконка для {button.Name} не найдена.");
            }
        }

        /// <summary>
        /// Универсальный метод для запуска приложения по указанному пути.
        /// </summary>
        /// <param name="toolInfo">Информация об инструменте</param>
        private void LaunchApplication(ToolInfo toolInfo)
        {
            string path = Path.Combine(basePath, toolInfo.FileName);

            try
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true,
                        WorkingDirectory = basePath
                    });

                    Debug.WriteLine($"Запущен: {toolInfo.Description}");
                }
                else
                {
                    MessageBox.Show($"Файл не найден: {path}\n\nИнструмент: {toolInfo.Description}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска приложения:\n{ex.Message}\n\nИнструмент: {toolInfo.Description}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Универсальный обработчик клика кнопки
        /// </summary>
        /// <param name="buttonIndex">Индекс кнопки (1-24)</param>
        private void HandleButtonClick(int buttonIndex)
        {
            if (buttonIndex > 0 && buttonIndex <= tools.Count)
            {
                LaunchApplication(tools[buttonIndex - 1]);
            }
        }

        // Обработчики событий для всех 24 кнопок
        private void button1_Click(object sender, EventArgs e) => HandleButtonClick(1);
        private void button2_Click(object sender, EventArgs e) => HandleButtonClick(2);
        private void button3_Click(object sender, EventArgs e) => HandleButtonClick(3);
        private void button4_Click(object sender, EventArgs e) => HandleButtonClick(4);
        private void button5_Click(object sender, EventArgs e) => HandleButtonClick(5);
        private void button6_Click(object sender, EventArgs e) => HandleButtonClick(6);
        private void button7_Click(object sender, EventArgs e) => HandleButtonClick(7);
        private void button8_Click(object sender, EventArgs e) => HandleButtonClick(8);
        private void button9_Click(object sender, EventArgs e) => HandleButtonClick(9);
        private void button10_Click(object sender, EventArgs e) => HandleButtonClick(10);
        private void button11_Click(object sender, EventArgs e) => HandleButtonClick(11);
        private void button12_Click(object sender, EventArgs e) => HandleButtonClick(12);
        private void button13_Click(object sender, EventArgs e) => HandleButtonClick(13);
        private void button14_Click(object sender, EventArgs e) => HandleButtonClick(14);
        private void button15_Click(object sender, EventArgs e) => HandleButtonClick(15);
        private void button16_Click(object sender, EventArgs e) => HandleButtonClick(16);
        private void button17_Click(object sender, EventArgs e) => HandleButtonClick(17);
        private void button18_Click(object sender, EventArgs e) => HandleButtonClick(18);
        private void button19_Click(object sender, EventArgs e) => HandleButtonClick(19);
        private void button20_Click(object sender, EventArgs e) => HandleButtonClick(20);
        private void button21_Click(object sender, EventArgs e) => HandleButtonClick(21);
        private void button22_Click(object sender, EventArgs e) => HandleButtonClick(22);
        private void button23_Click(object sender, EventArgs e) => HandleButtonClick(23);
        private void button24_Click(object sender, EventArgs e) => HandleButtonClick(24);

        // Обработчики ссылок
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://psyshout.gumroad.com/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия ссылки:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://t.me/PsyTranceCoverArt_bot",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия ссылки:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Показать окно "О программе"
        /// </summary>
        private void ShowAbout()
        {
            var availableTools = tools.Where(t => File.Exists(Path.Combine(basePath, t.FileName))).Count();
            var totalTools = tools.Count;

            string message = $"PsyShout Tools Launcher v2.0\n\n" +
                           $"Центральный лаунчер для запуска всех инструментов PsyShout Tools\n\n" +
                           $"Доступно инструментов: {availableTools} из {totalTools}\n\n" +
                           $"Инструменты по категориям:\n" +
                           $"• Аудио инструменты: 8\n" +
                           $"• Музыкальные инструменты: 1\n" +
                           $"• Системные утилиты: 4\n" +
                           $"• Работа с данными: 4\n" +
                           $"• Коммуникация и организация: 4\n" +
                           $"• Специальные инструменты: 3\n\n" +
                           $"Версия: 2.0\n" +
                           $"© 2024 PsyShout Tools";

            MessageBox.Show(message, "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Обработка нажатия клавиш для быстрого доступа
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // F1 - показать справку
            if (keyData == Keys.F1)
            {
                ShowAbout();
                return true;
            }

            // Ctrl+R - обновить список инструментов
            if (keyData == (Keys.Control | Keys.R))
            {
                CheckToolAvailability();
                MessageBox.Show("Список инструментов обновлен!", "Обновление",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    /// <summary>
    /// Информация об инструменте
    /// </summary>
    public class ToolInfo
    {
        public string FileName { get; }
        public string Description { get; }
        public int ButtonIndex { get; }

        public ToolInfo(string fileName, string description, int buttonIndex)
        {
            FileName = fileName;
            Description = description;
            ButtonIndex = buttonIndex;
        }
    }
}