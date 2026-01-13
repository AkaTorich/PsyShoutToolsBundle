using System;
using System.IO;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace VSTDistortion
{
    /// <summary>
    /// Основной класс VST плагина
    /// </summary>
    internal sealed class DistortionVstPlugin : VstPluginWithInterfaceManagerBase
    {
        public DistortionVstPlugin()
            : base("VST Distortion",
                   new VstProductInfo("VST Distortion", "Simple Distortion Plugin", 1000),
                   VstPluginCategory.Effect,
                   VstPluginCapabilities.NoSoundInStop,
                   5,  // initialDelay
                   1234)  // tailSize
        {
        }

        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
                return new DistortionPlugin();
            return instance;
        }

        protected override IVstPluginParameters CreateParameters(IVstPluginParameters instance)
        {
            if (instance == null)
                return new DistortionParameters();
            return instance;
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
                return new DistortionPrograms();
            return instance;
        }

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
            {
                // ИСПРАВЛЕНИЕ: нужно получать ссылку на существующий плагин, а не создавать новый
                var audioProcessor = GetInstance<IVstPluginAudioProcessor>();
                if (audioProcessor is DistortionPlugin plugin)
                {
                    return new DistortionPluginEditor(plugin);
                }
                return new DistortionPluginEditor(new DistortionPlugin());
            }
            return instance;
        }
    }

    /// <summary>
    /// Класс для обработки аудио
    /// </summary>
    public class DistortionPlugin : IVstPluginAudioProcessor, IVstPluginPersistence
    {
        private DistortionParameters _parameters;
        private DistortionProcessor _processor;
        private PresetManager _presetManager;
        private DistortionPluginEditor _editor;

        public IVstPluginEditor Editor
        {
            get { return _editor; }
        }

        public DistortionPlugin()
        {
            // Инициализируем параметры
            _parameters = new DistortionParameters();
            
            // Инициализируем процессор
            _processor = new DistortionProcessor(_parameters);
            
            // Инициализируем менеджер пресетов
            _presetManager = new PresetManager(_parameters);
        }

        // Свойства для доступа из UI
        public DistortionParameters Parameters => _parameters;
        public PresetManager PresetManager => _presetManager;

        public void Dispose()
        {
            // Очистка ресурсов
        }

        #region IVstPluginAudioProcessor Implementation

        public float SampleRate { get; set; } = 44100.0f;
        public int BlockSize { get; set; } = 1024;
        public int InputCount => 2;
        public int OutputCount => 2;
        public int TailSize => 0;

        public void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            if (inChannels == null || outChannels == null) return;
            if (inChannels.Length == 0 || outChannels.Length == 0) return;

            int sampleCount = inChannels[0].SampleCount;

            // Конвертируем VstAudioBuffer в float[]
            float[] leftInput = new float[sampleCount];
            float[] leftOutput = new float[sampleCount];

            // Копируем данные из VstAudioBuffer
            for (int i = 0; i < sampleCount; i++)
            {
                leftInput[i] = inChannels[0][i];
            }

            // Обрабатываем левый канал
            _processor.Process(leftInput, leftOutput, sampleCount);

            // Копируем обратно в VstAudioBuffer
            for (int i = 0; i < sampleCount; i++)
            {
                outChannels[0][i] = leftOutput[i];
            }

            // Обрабатываем правый канал (стерео)
            if (inChannels.Length > 1 && outChannels.Length > 1)
            {
                float[] rightInput = new float[sampleCount];
                float[] rightOutput = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    rightInput[i] = inChannels[1][i];
                }

                _processor.Process(rightInput, rightOutput, sampleCount);

                for (int i = 0; i < sampleCount; i++)
                {
                    outChannels[1][i] = rightOutput[i];
                }
            }
        }

        public void ProcessReplacing(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            Process(inChannels, outChannels);
        }

        public void ProcessDoubleReplacing(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            Process(inChannels, outChannels);
        }

        public void SetProcessPrecision(VstProcessPrecision precision)
        {
            // Поддерживаем только 32-bit float
        }

        public VstProcessPrecision ProcessPrecision => VstProcessPrecision.Process32;

        public bool SetPanLaw(VstPanLaw panLaw, float gain)
        {
            return false;
        }

        public bool SetSpeakerArrangement(VstSpeakerArrangement input, VstSpeakerArrangement output)
        {
            // Поддерживаем стандартные стерео настройки
            return true;
        }

        public VstSpeakerArrangement InputSpeakerArrangement => new VstSpeakerArrangement();
        public VstSpeakerArrangement OutputSpeakerArrangement => new VstSpeakerArrangement();

        public int GetChunk(bool isPreset, out VstPatchChunkInfo chunkInfo)
        {
            try
            {
                byte[] data = _presetManager.SaveState();
                chunkInfo = new VstPatchChunkInfo(data.Length, 1, 0, 1);
                return data.Length;
            }
            catch
            {
                chunkInfo = new VstPatchChunkInfo(0, 1, 0, 1);
                return 0;
            }
        }

        public void SetChunk(byte[] data, bool isPreset, VstPatchChunkInfo chunkInfo)
        {
            try
            {
                if (data != null && data.Length > 0)
                {
                    _presetManager.LoadState(data);
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }

        #endregion

        #region IVstPluginPersistence Implementation

        public bool CanLoadChunk(VstPatchChunkInfo chunkInfo)
        {
            return true;
        }

        public void LoadChunk(VstPatchChunkInfo chunkInfo, bool _isPreset)
        {
            try
            {
                // VstPatchChunkInfo не имеет GetData(), используем другой подход
                // Оставляем заглушку для совместимости
                // _presetManager.LoadState(data);
                
                // Обновляем UI если нужно
                // _editor?.UpdateFromPlugin();
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }

        public VstPatchChunkInfo SaveChunk(bool _isPreset)
        {
            try
            {
                byte[] data = _presetManager.SaveState();
                return new VstPatchChunkInfo(data.Length, 1, 0, 1);
            }
            catch
            {
                return new VstPatchChunkInfo(0, 1, 0, 1);
            }
        }

        public void ReadPrograms(Stream _stream, VstProgramCollection _programs)
        {
            try
            {
                using (var reader = new BinaryReader(_stream))
                {
                    var data = reader.ReadBytes((int)_stream.Length);
                    _presetManager.LoadState(data);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        public void WritePrograms(Stream _stream, VstProgramCollection _programs)
        {
            try
            {
                var data = _presetManager.SaveState();
                _stream.Write(data, 0, data.Length);
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        #endregion

        #region Parameter Management

        public string GetParameterName(int index)
        {
            return _parameters.GetParameterName(index);
        }

        public string GetParameterLabel(int index)
        {
            return _parameters.GetParameterLabel(index);
        }

        public string GetParameterDisplay(int index)
        {
            return _parameters.GetParameterDisplay(index);
        }

        public float GetParameter(int index)
        {
            return _parameters.GetParameterValue(index);
        }

        public void SetParameter(int index, float value)
        {
            _parameters.SetParameterValue(index, value);
            
            // ИСПРАВЛЕНИЕ: уведомляем редактор об изменениях
            UpdateEditor();
        }

        #endregion

        #region Editor Management

        public IVstPluginEditor CreateEditor()
        {
            if (_editor == null)
            {
                _editor = new DistortionPluginEditor(this);
            }
            return _editor;
        }
        
        // ИСПРАВЛЕНИЕ: метод для обновления редактора
        public void UpdateEditor()
        {
            if (_editor is DistortionPluginEditor editor)
            {
                editor.UpdateFromPlugin();
            }
        }

        public bool EditorGetRect(out System.Drawing.Rectangle rect)
        {
            rect = new System.Drawing.Rectangle(0, 0, 384, 410);
            return true;
        }

        #endregion
    }
}