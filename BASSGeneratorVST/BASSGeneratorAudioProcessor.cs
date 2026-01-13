// BASSGeneratorAudioProcessor.cs - Обработка аудио
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace BASSGeneratorVST
{
    internal sealed class BASSGeneratorAudioProcessor : VstPluginAudioProcessorBase, IVstPluginProcess
    {
        private BASSGeneratorVstPlugin _plugin;

        public BASSGeneratorAudioProcessor(BASSGeneratorVstPlugin plugin)
            : base(2, 2, 0)
        {
            _plugin = plugin;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            // Плагин генерирует MIDI, аудио просто пропускаем
            for (int i = 0; i < inChannels[0].SampleCount; i++)
            {
                outChannels[0][i] = inChannels[0][i];
                if (inChannels.Length > 1 && outChannels.Length > 1)
                    outChannels[1][i] = inChannels[1][i];
            }

            // Обрабатываем MIDI события из плеера
            if (_plugin.MidiProcessor != null)
            {
                _plugin.MidiProcessor.ProcessPlayerEvents(inChannels[0].SampleCount);
            }
        }
    }
}

