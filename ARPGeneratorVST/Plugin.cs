// Plugin.cs - Основной класс VST плагина ARPGenerator
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ARPGeneratorVST
{
    /// <summary>
    /// Основной класс VST плагина ARPGenerator
    /// </summary>
    internal sealed class ARPGeneratorVstPlugin : VstPluginWithInterfaceManagerBase
    {
        private ARPGeneratorMidiProcessor _midiProcessor;

        public ARPGeneratorVstPlugin()
            : base("ARPGenerator",
                   new VstProductInfo("PsyShoutTools", "ARP Generator VST", 1000),
                   VstPluginCategory.Generator,
                   VstPluginCapabilities.NoSoundInStop,
                   0,  // initialDelay
                   0)  // tailSize
        {
        }

        public ARPGeneratorMidiProcessor MidiProcessor => _midiProcessor;

        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
                return new ARPGeneratorAudioProcessor(this);
            return instance;
        }

        protected override IVstPluginParameters CreateParameters(IVstPluginParameters instance)
        {
            if (instance == null)
                return new ARPGeneratorParameters();
            return instance;
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
                return new ARPGeneratorPrograms();
            return instance;
        }

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
                return new ARPGeneratorEditor(this);
            return instance;
        }

        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new ARPGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
        
        protected override IVstPluginMidiSource CreateMidiSource(IVstPluginMidiSource instance)
        {
            if (_midiProcessor == null)
            {
                _midiProcessor = new ARPGeneratorMidiProcessor(this);
            }
            return _midiProcessor;
        }
    }
}

