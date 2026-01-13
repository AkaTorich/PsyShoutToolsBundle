// BASSGeneratorMidiProcessor.cs - MIDI процессор для воспроизведения MIDI в VST
using System;
using System.Collections.Generic;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace BASSGeneratorVST
{
    internal sealed class BASSGeneratorMidiProcessor : IVstMidiProcessor, IVstPluginMidiSource
    {
        private BASSGeneratorVstPlugin _plugin;
        private MidiFilePlayer _midiPlayer;
        private VstEventCollection _outputEvents;

        public BASSGeneratorMidiProcessor(BASSGeneratorVstPlugin plugin)
        {
            _plugin = plugin;
            _midiPlayer = new MidiFilePlayer();
            _outputEvents = new VstEventCollection();
        }

        public MidiFilePlayer MidiPlayer => _midiPlayer;

        public int ChannelCount => 16;

        public void Process(VstEventCollection events)
        {
            // IVstMidiProcessor - для входящих MIDI событий
        }

        public void ProcessPlayerEvents(int sampleFrames)
        {
            if (!_midiPlayer.IsPlaying)
                return;

            var midiMessages = _midiPlayer.Process(sampleFrames);
            
            foreach (var msg in midiMessages)
            {
                var midiEvent = new VstMidiEvent(
                    BitConverter.ToInt32(msg, 0),
                    0,
                    0,
                    new byte[] { msg[4], msg[5], msg[6], msg[7] },
                    0,
                    0,
                    false
                );
                
                _outputEvents.Add(midiEvent);
            }
        }

        // IVstPluginMidiSource - для исходящих MIDI событий
        public VstEventCollection GetCurrentEvents()
        {
            var result = _outputEvents;
            _outputEvents = new VstEventCollection(); // Создаем новую коллекцию для следующего цикла
            return result;
        }
    }
}

