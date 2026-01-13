using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace VSTDistortion
{
    [Serializable]
    public class PresetData
    {
        public string Name { get; set; }
        public float Drive { get; set; }
        public float Output { get; set; }
        public float Type { get; set; }
        public float Mix { get; set; }
        public float Tone { get; set; }

        public PresetData()
        {
            Name = "Default";
        }

        public PresetData(string name, DistortionParameters parameters)
        {
            Name = name;
            Drive = parameters.Drive;
            Output = parameters.Output;
            Type = parameters.Type;
            Mix = parameters.Mix;
            Tone = parameters.Tone;
        }
    }

    public class PresetManager
    {
        private readonly List<PresetData> _presets;
        private readonly string _presetPath;
        private readonly DistortionParameters _parameters;

        public List<PresetData> Presets => _presets;
        public event EventHandler PresetsChanged;

        public PresetManager(DistortionParameters parameters)
        {
            _parameters = parameters;
            _presets = new List<PresetData>();
            
            // Путь к папке с пресетами - ИСПОЛЬЗУЕМ ПАПКУ ДОКУМЕНТЫ
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _presetPath = Path.Combine(documentsPath, "PsyDistortion");
            
            if (!Directory.Exists(_presetPath))
                Directory.CreateDirectory(_presetPath);

            // Логируем путь для отладки
            System.Diagnostics.Debug.WriteLine($"PsyShout Distortion пресеты сохраняются в: {_presetPath}");

            LoadDefaultPresets();
            LoadPresetsFromDisk();
        }

        private void LoadDefaultPresets()
        {
            // Заводские пресеты (значения в диапазоне 0-1) - БОЛЕЕ АГРЕССИВНЫЕ
            _presets.Add(new PresetData 
            { 
                Name = "Clean", 
                Drive = 0.0f, 
                Output = 1.0f, 
                Type = 0.0f, 
                Mix = 1.0f, 
                Tone = 0.5f 
            });

            _presets.Add(new PresetData 
            { 
                Name = "Soft Overdrive", 
                Drive = 0.3f, // Уменьшил с 0.6f до 0.3f
                Output = 0.8f, 
                Type = 0.0f, 
                Mix = 1.0f, 
                Tone = 0.6f 
            });

            _presets.Add(new PresetData 
            { 
                Name = "Heavy Distortion", 
                Drive = 0.5f, // Уменьшил с 0.85f до 0.5f
                Output = 0.75f, 
                Type = 0.33f, // HardClip (1/3 = 0.33) 
                Mix = 1.0f, 
                Tone = 0.4f 
            });

            _presets.Add(new PresetData 
            { 
                Name = "Tube Warmth", 
                Drive = 0.4f, // Уменьшил с 0.7f до 0.4f
                Output = 0.85f, 
                Type = 0.67f, // Tube (2/3 = 0.67) 
                Mix = 0.8f, 
                Tone = 0.55f 
            });

            _presets.Add(new PresetData 
            { 
                Name = "Fuzz Face", 
                Drive = 0.5f, // Уменьшил с 0.9f до 0.5f
                Output = 0.7f, 
                Type = 1.0f, // Fuzz (3/3 = 1.0) 
                Mix = 1.0f, 
                Tone = 0.65f 
            });
        }

        public void LoadPresetsFromDisk()
        {
            try
            {
                string[] presetFiles = Directory.GetFiles(_presetPath, "*.Psy"); // Используем расширение .Psy
                
                foreach (string file in presetFiles)
                {
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(PresetData));
                        using (FileStream stream = new FileStream(file, FileMode.Open))
                        {
                            PresetData preset = (PresetData)serializer.Deserialize(stream);
                            
                            // Нормализуем значения в диапазон 0-1 (если они в старом диапазоне 0-100)
                            NormalizePresetValues(preset);
                            
                            // Проверяем, нет ли уже пресета с таким именем
                            bool exists = _presets.Exists(p => p.Name == preset.Name);
                            if (!exists)
                            {
                                _presets.Add(preset);
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем поврежденные файлы пресетов
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }

        private void NormalizePresetValues(PresetData preset)
        {
            // Если значения в диапазоне 0-100, нормализуем их в 0-1
            if (preset.Drive > 1.0f) preset.Drive /= 100.0f;
            if (preset.Output > 1.0f) preset.Output /= 100.0f;
            if (preset.Mix > 1.0f) preset.Mix /= 100.0f;
            if (preset.Tone > 1.0f || preset.Tone < -1.0f) 
            {
                // Tone: -100 до +100 -> 0 до 1
                preset.Tone = (preset.Tone + 100.0f) / 200.0f;
            }
            
            // Ограничиваем значения в допустимых пределах
            preset.Drive = Math.Max(0.0f, Math.Min(1.0f, preset.Drive));
            preset.Output = Math.Max(0.0f, Math.Min(1.0f, preset.Output));
            preset.Mix = Math.Max(0.0f, Math.Min(1.0f, preset.Mix));
            preset.Tone = Math.Max(0.0f, Math.Min(1.0f, preset.Tone));
            preset.Type = Math.Max(0.0f, Math.Min(3.0f, preset.Type));
        }

        public void SavePreset(string name)
        {
            try
            {
                PresetData newPreset = new PresetData(name, _parameters);
                
                // Удаляем существующий пресет с таким же именем
                _presets.RemoveAll(p => p.Name == name);
                _presets.Add(newPreset);

                // Сохраняем на диск
                SavePresetToDisk(newPreset);
                
                PresetsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка сохранения пресета: {ex.Message}");
            }
        }

        private void SavePresetToDisk(PresetData preset)
        {
            string fileName = SanitizeFileName(preset.Name) + ".Psy"; // Используем расширение .Psy
            string filePath = Path.Combine(_presetPath, fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(PresetData));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, preset);
            }
        }

        public void LoadPreset(string name)
        {
            PresetData preset = _presets.Find(p => p.Name == name);
            if (preset != null)
            {
                // Создаем копию пресета для нормализации
                PresetData normalizedPreset = new PresetData
                {
                    Name = preset.Name,
                    Drive = preset.Drive,
                    Output = preset.Output,
                    Type = preset.Type,
                    Mix = preset.Mix,
                    Tone = preset.Tone
                };
                
                // Нормализуем значения
                NormalizePresetValues(normalizedPreset);
                
                // Применяем нормализованные значения
                _parameters.Drive = normalizedPreset.Drive;
                _parameters.Output = normalizedPreset.Output;
                _parameters.Type = normalizedPreset.Type;
                _parameters.Mix = normalizedPreset.Mix;
                _parameters.Tone = normalizedPreset.Tone;
            }
        }

        public void DeletePreset(string name)
        {
            try
            {
                _presets.RemoveAll(p => p.Name == name);
                
                // Удаляем файл с диска
                string fileName = SanitizeFileName(name) + ".Psy"; // Используем расширение .Psy
                string filePath = Path.Combine(_presetPath, fileName);
                
                if (File.Exists(filePath))
                    File.Delete(filePath);
                
                PresetsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка удаления пресета: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        // Методы для сериализации состояния плагина (для DAW)
        public byte[] SaveState()
        {
            try
            {
                PresetData currentState = new PresetData("CurrentState", _parameters);
                
                XmlSerializer serializer = new XmlSerializer(typeof(PresetData));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, currentState);
                    return stream.ToArray();
                }
            }
            catch
            {
                return new byte[0];
            }
        }

        public void LoadState(byte[] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                    return;

                XmlSerializer serializer = new XmlSerializer(typeof(PresetData));
                using (MemoryStream stream = new MemoryStream(data))
                {
                    PresetData state = (PresetData)serializer.Deserialize(stream);
                    
                    _parameters.Drive = state.Drive;
                    _parameters.Output = state.Output;
                    _parameters.Type = state.Type;
                    _parameters.Mix = state.Mix;
                    _parameters.Tone = state.Tone;
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки состояния
            }
        }
    }
}