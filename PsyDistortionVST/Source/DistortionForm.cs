using System;
using System.Windows.Forms;

namespace VSTDistortion
{
    public partial class DistortionForm : Form
    {
        private DistortionPlugin _plugin;
        private DistortionParameters _parameters;
        private PresetManager _presetManager;
        private bool _isUpdatingControls = false;

        public DistortionForm(DistortionPlugin plugin)
        {
            InitializeComponent();
            _plugin = plugin;
            _parameters = plugin.Parameters; // Используем параметры из плагина
            _presetManager = plugin.PresetManager; // Используем менеджер пресетов из плагина
            
            InitializeEventHandlers();
            InitializePresets();
            UpdateControlsFromParameters();
        }

        private void InitializeEventHandlers()
        {
            // TrackBar события
            trackBarDrive.ValueChanged += TrackBarDrive_ValueChanged;
            trackBarOutput.ValueChanged += TrackBarOutput_ValueChanged;
            trackBarMix.ValueChanged += TrackBarMix_ValueChanged;
            trackBarTone.ValueChanged += TrackBarTone_ValueChanged;
            
            // ComboBox события
            comboBoxType.SelectedIndexChanged += ComboBoxType_SelectedIndexChanged;
            comboBoxPresets.SelectedIndexChanged += ComboBoxPresets_SelectedIndexChanged;
            
            // Button события
            buttonSavePreset.Click += ButtonSavePreset_Click;
            buttonDeletePreset.Click += ButtonDeletePreset_Click;
            
            // Preset manager события
            _presetManager.PresetsChanged += PresetManager_PresetsChanged;
        }

        private void InitializePresets()
        {
            RefreshPresetList();
        }

        private void RefreshPresetList()
        {
            comboBoxPresets.Items.Clear();
            foreach (var preset in _presetManager.Presets)
            {
                comboBoxPresets.Items.Add(preset.Name);
            }
            
            if (comboBoxPresets.Items.Count > 0)
                comboBoxPresets.SelectedIndex = 0;
        }

        private void UpdateControlsFromParameters()
        {
            if (_isUpdatingControls) return;
            
            _isUpdatingControls = true;
            
            try
            {
                // ИСПРАВЛЕНИЕ: денормализуем значения параметров обратно к диапазонам TrackBar'ов
                trackBarDrive.Value = (int)(_parameters.Drive * 100.0f);
                trackBarOutput.Value = (int)(_parameters.Output * 100.0f);
                trackBarMix.Value = (int)(_parameters.Mix * 100.0f);
                
                // ИСПРАВЛЕНИЕ: для Tone обратное преобразование: (-1.0 до +1.0) -> (-100 до +100)
                trackBarTone.Value = (int)(_parameters.Tone * 100.0f);
                
                comboBoxType.SelectedIndex = (int)(_parameters.Type * 3.0f); // Денормализуем обратно
                
                UpdateValueLabels();
            }
            finally
            {
                _isUpdatingControls = false;
            }
        }

        private void UpdateValueLabels()
        {
            // Показываем значения параметров в правильном диапазоне
            labelDriveValue.Text = trackBarDrive.Value.ToString();
            labelOutputValue.Text = trackBarOutput.Value.ToString();
            labelMixValue.Text = trackBarMix.Value.ToString();
            labelToneValue.Text = trackBarTone.Value.ToString();
        }

        #region TrackBar Event Handlers

        private void TrackBarDrive_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            
            // ИСПРАВЛЕНИЕ: нормализуем значение TrackBar (0-100) в диапазон VstParameter (0.0-1.0)
            _parameters.Drive = trackBarDrive.Value / 100.0f;
            labelDriveValue.Text = trackBarDrive.Value.ToString();
        }

        private void TrackBarOutput_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            
            // ИСПРАВЛЕНИЕ: для Output используем диапазон 0-200, нормализуем к 0.0-2.0
            _parameters.Output = trackBarOutput.Value / 100.0f;
            labelOutputValue.Text = trackBarOutput.Value.ToString();
        }

        private void TrackBarMix_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            
            // ИСПРАВЛЕНИЕ: нормализуем значение Mix (0-100) к (0.0-1.0)
            _parameters.Mix = trackBarMix.Value / 100.0f;
            labelMixValue.Text = trackBarMix.Value.ToString();
        }

        private void TrackBarTone_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            
            // ИСПРАВЛЕНИЕ: TrackBar уже с диапазоном -100 до +100, просто нормализуем к -1.0 до +1.0
            _parameters.Tone = trackBarTone.Value / 100.0f;
            labelToneValue.Text = trackBarTone.Value.ToString();
        }

        #endregion

        #region ComboBox Event Handlers

        private void ComboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            
            // ИСПРАВЛЕНИЕ: нормализуем индекс к диапазону 0.0-1.0
            _parameters.Type = comboBoxType.SelectedIndex / 3.0f; // 4 типа = индексы 0,1,2,3 -> нормализуем к 0.0-1.0
        }

        private void ComboBoxPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingControls) return;
            if (comboBoxPresets.SelectedItem == null) return;
            
            string presetName = comboBoxPresets.SelectedItem.ToString();
            _presetManager.LoadPreset(presetName);
            UpdateControlsFromParameters();
        }

        #endregion

        #region Button Event Handlers

        private void ButtonSavePreset_Click(object sender, EventArgs e)
        {
            string presetName = textBoxPresetName.Text.Trim();
            
            if (string.IsNullOrEmpty(presetName))
            {
                MessageBox.Show("Введи имя пресета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (presetName.Length > 50)
            {
                MessageBox.Show("Имя пресета слишком длинное", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _presetManager.SavePreset(presetName);
                textBoxPresetName.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения пресета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonDeletePreset_Click(object sender, EventArgs e)
        {
            if (comboBoxPresets.SelectedItem == null)
            {
                MessageBox.Show("Выбери пресет для удаления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string presetName = comboBoxPresets.SelectedItem.ToString();

            // Защита заводских пресетов
            string[] protectedPresets = { "Clean", "Soft Overdrive", "Heavy Distortion", "Tube Warmth", "Fuzz Face" };
            foreach (string protectedPreset in protectedPresets)
            {
                if (presetName == protectedPreset)
                {
                    MessageBox.Show("Нельзя удалить заводской пресет", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            DialogResult result = MessageBox.Show($"Удалить пресет '{presetName}'?", "Подтверждение", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    _presetManager.DeletePreset(presetName);
                    MessageBox.Show($"Пресет '{presetName}' удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пресета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Preset Manager Event Handlers

        private void PresetManager_PresetsChanged(object sender, EventArgs e)
        {
            RefreshPresetList();
        }

        #endregion

        // Метод для обновления интерфейса из плагина
        public void UpdateFromPlugin()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateControlsFromParameters));
            }
            else
            {
                UpdateControlsFromParameters();
            }
        }
        
        // ИСПРАВЛЕНИЕ: Метод для установки фокуса на текстовое поле
        public void FocusPresetNameTextBox()
        {
            if (textBoxPresetName != null)
            {
                textBoxPresetName.Focus();
                textBoxPresetName.SelectAll();
            }
        }
    }
} 