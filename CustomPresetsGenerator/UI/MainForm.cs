using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpirePresetsGenerator.Logic;
using SpirePresetsGenerator.Models;
using SpirePresetsGenerator.Services;

namespace SpirePresetsGenerator.UI
{
    public partial class MainForm : Form
    {
        private List<PresetFile> _generated = new List<PresetFile>();

        public MainForm()
        {
            InitializeComponent();
            
            // Устанавливаем значения по умолчанию
            if (presetType.Items.Count > 0) presetType.SelectedIndex = 0; // "random"
            
            // Устанавливаем значения по умолчанию для арпеджиатора
            if (arpMode.Items.Count > 0) arpMode.SelectedIndex = 0;
            if (arpOctave.Items.Count > 0) arpOctave.SelectedIndex = 0;
            if (arpSpeed.Items.Count > 0) arpSpeed.SelectedIndex = 2; // 1/16
            if (arpPattern.Items.Count > 0) arpPattern.SelectedIndex = 0;
            
            // Инициализируем категории шкал
            InitializeScaleCategories();
        }

        private void DrawGradientBackground(Graphics g, Rectangle rect)
        {
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.FromArgb(102, 126, 234), Color.FromArgb(118, 75, 162), 135f))
            {
                g.FillRectangle(brush, rect);
            }
        }

        private void enableArp_CheckedChanged(object sender, EventArgs e)
        {
            arpPanel.Visible = enableArp.Checked;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            int count = (int)presetCount.Value;
            if (count < 1 || count > 1000)
            {
                ShowStatus("Укажи количество от 1 до 1000", isError: true);
                return;
            }

            string type = (string)presetType.SelectedItem;
            string author = authorName.Text;

            ArpConfig arp = null;
            if (enableArp.Checked)
            {
                double mode = ParsePrefixDouble((string)arpMode.SelectedItem);
                double octave = ParsePrefixDouble((string)arpOctave.SelectedItem);
                double speed = ParsePrefixDouble((string)arpSpeed.SelectedItem);
                string pattern = (string)arpPattern.SelectedItem;
                string selectedScaleCategory = scaleCategory.SelectedItem?.ToString();
                string selectedScaleName = scaleSelection.SelectedItem?.ToString();
                arp = new ArpConfig { 
                    Enabled = true, 
                    Mode = mode, 
                    Octave = octave, 
                    Speed = speed, 
                    Pattern = pattern, 
                    ModeName = GetSuffixText((string)arpMode.SelectedItem), 
                    SpeedName = GetSuffixText((string)arpSpeed.SelectedItem),
                    ScaleCategory = selectedScaleCategory,
                    ScaleName = selectedScaleName
                };
            }

            var generator = new PresetGenerator();
            _generated = generator.GeneratePresets(count, type, author, arp);

            presetList.Items.Clear();
            for (int i = 0; i < _generated.Count; i++) presetList.Items.Add((i + 1).ToString("D2") + ". " + _generated[i].Name);
            presetList.Visible = true;

            FileService.SaveAllPresets(_generated, null);
            string genDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated");

            ShowStatus("✓ Сохранено " + _generated.Count + " пресетов в: " + genDir);
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            FileService.SaveAllPresets(_generated, null);
            ShowStatus("Файлы сохранены: " + System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated"));
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            DrawGradientBackground(e.Graphics, this.ClientRectangle);
        }

        private void ShowStatus(string text, bool isError = false)
        {
            statusLabel.Text = text;
            statusLabel.BackColor = isError ? Color.FromArgb(248, 215, 218) : Color.FromArgb(212, 237, 218);
            statusLabel.ForeColor = isError ? Color.FromArgb(114, 28, 36) : Color.FromArgb(21, 87, 36);
            statusLabel.Visible = true;
        }

        private static double ParsePrefixDouble(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            int idx = value.IndexOf(' ');
            string prefix = idx > 0 ? value.Substring(0, idx) : value;
            double.TryParse(prefix, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double num);
            return num;
        }

        private static string GetSuffixText(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            int idx = value.IndexOf('-');
            return idx >= 0 && idx + 1 < value.Length ? value.Substring(idx + 1).Trim() : value;
        }

        private void InitializeScaleCategories()
        {
            try
            {
                var categories = Logic.ScaleManager.GetCategories();
                scaleCategory.Items.Clear();
                scaleCategory.Items.AddRange(categories.ToArray());
                
                if (scaleCategory.Items.Count > 0)
                {
                    scaleCategory.SelectedIndex = 0;
                    LoadScalesForCategory();
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Ошибка инициализации шкал: " + ex.Message, isError: true);
            }
        }

        private void LoadScalesForCategory()
        {
            try
            {
                if (scaleCategory.SelectedItem == null) return;
                
                string selectedCategory = scaleCategory.SelectedItem.ToString();
                var scales = Logic.ScaleManager.GetScalesInCategory(selectedCategory);
                
                scaleSelection.Items.Clear();
                scaleSelection.Items.AddRange(scales.ToArray());
                
                if (scaleSelection.Items.Count > 0)
                {
                    scaleSelection.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Ошибка загрузки шкал: " + ex.Message, isError: true);
            }
        }

        private void scaleCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadScalesForCategory();
        }
    }
} 