// ScaleHelperAudioProcessor.cs - Обработка аудио
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ScaleHelperVST
{
    internal sealed class ScaleHelperAudioProcessor : VstPluginAudioProcessorBase
    {
        private ScaleHelperVstPlugin _plugin;

        public ScaleHelperAudioProcessor(ScaleHelperVstPlugin plugin)
            : base(2, 2, 0)
        {
            _plugin = plugin;
        }

        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            // Просто пропускаем аудио без изменений
            for (int i = 0; i < inChannels[0].SampleCount; i++)
            {
                outChannels[0][i] = inChannels[0][i];
                if (inChannels.Length > 1 && outChannels.Length > 1)
                    outChannels[1][i] = inChannels[1][i];
            }
        }
    }
}