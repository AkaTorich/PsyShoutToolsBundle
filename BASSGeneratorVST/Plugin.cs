// Plugin.cs - Основной класс VST плагина BASSGenerator
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace BASSGeneratorVST
{
    /// <summary>
    /// Основной класс VST плагина BASSGenerator
    /// </summary>
    internal sealed class BASSGeneratorVstPlugin : VstPluginWithInterfaceManagerBase
    {
        private BASSGeneratorMidiProcessor _midiProcessor;

        public BASSGeneratorVstPlugin()
            : base("BASSGenerator",
                   new VstProductInfo("PsyShoutTools", "BASS Generator VST", 1000),
                   VstPluginCategory.Generator,
                   VstPluginCapabilities.NoSoundInStop,
                   0,  // initialDelay
                   0)  // tailSize
        {
        }

        public BASSGeneratorMidiProcessor MidiProcessor => _midiProcessor;

        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
                return new BASSGeneratorAudioProcessor(this);
            return instance;
        }

        protected override IVstPluginParameters CreateParameters(IVstPluginParameters instance)
        {
            if (instance == null)
                return new BASSGeneratorParameters();
            return instance;
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
                return new BASSGeneratorPrograms();
            return instance;
        }

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
                return new BASSGeneratorEditor(this);
            return instance;
        }

        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new BASSGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
        
        protected override IVstPluginMidiSource CreateMidiSource(IVstPluginMidiSource instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new BASSGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
    }
}

