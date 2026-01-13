// Plugin.cs - Основной класс VST плагина PADGenerator
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace PADGeneratorVST
{
    /// <summary>
    /// Основной класс VST плагина PADGenerator
    /// </summary>
    internal sealed class PADGeneratorVstPlugin : VstPluginWithInterfaceManagerBase
    {
        private PADGeneratorMidiProcessor _midiProcessor;

        public PADGeneratorVstPlugin()
            : base("PADGenerator",
                   new VstProductInfo("PsyShoutTools", "PAD Generator VST", 1000),
                   VstPluginCategory.Generator,
                   VstPluginCapabilities.NoSoundInStop,
                   0,  // initialDelay
                   0)  // tailSize
        {
        }

        public PADGeneratorMidiProcessor MidiProcessor => _midiProcessor;

        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
                return new PADGeneratorAudioProcessor(this);
            return instance;
        }

        protected override IVstPluginParameters CreateParameters(IVstPluginParameters instance)
        {
            if (instance == null)
                return new PADGeneratorParameters();
            return instance;
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
                return new PADGeneratorPrograms();
            return instance;
        }

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
                return new PADGeneratorEditor(this);
            return instance;
        }

        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new PADGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
        
        protected override IVstPluginMidiSource CreateMidiSource(IVstPluginMidiSource instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new PADGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
    }
}

