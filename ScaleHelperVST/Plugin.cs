// Plugin.cs - Простое решение с сохранением состояния
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ScaleHelperVST
{
    /// <summary>
    /// Основной класс VST плагина
    /// </summary>
    internal sealed class ScaleHelperVstPlugin : VstPluginWithInterfaceManagerBase
    {
        public ScaleHelperVstPlugin()
            : base("ScaleHelper",
                   new VstProductInfo("PsyShoutTools", "Scale Helper VST", 1000),
                   VstPluginCategory.Effect,
                   VstPluginCapabilities.NoSoundInStop,
                   0,  // initialDelay
                   0)  // tailSize
        {
        }

        // Исправлено: убраны CreateXXX методы, используем CreateXXX(instance) вместо них
        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
                return new ScaleHelperAudioProcessor(this);
            return instance;
        }

        protected override IVstPluginParameters CreateParameters(IVstPluginParameters instance)
        {
            if (instance == null)
                return new ScaleHelperParameters();
            return instance;
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
                return new ScaleHelperPrograms();
            return instance;
        }

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
                return new ScaleHelperEditor(this);
            return instance;
        }
    }
}