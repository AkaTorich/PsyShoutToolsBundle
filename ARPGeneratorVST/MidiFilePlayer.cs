// MidiFilePlayer.cs - Компонент для воспроизведения MIDI файлов в VST плагине
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jacobi.Vst.Core;
using NAudio.Midi;

namespace ARPGeneratorVST
{
    public class MidiFilePlayer
    {
        private List<MidiEvent> _events;
        private int _currentEventIndex;
        private long _currentSampleTime;
        private double _sampleRate;
        private int _ticksPerQuarterNote;
        private double _microsecondsPerQuarterNote;
        private bool _isPlaying;
        private bool _loop;
        private MidiOut _midiOut;

        public bool IsPlaying => _isPlaying;
        public event Action<byte[]> OnMidiEvent;

        public MidiFilePlayer()
        {
            _events = new List<MidiEvent>();
            _sampleRate = 44100;
            _microsecondsPerQuarterNote = 500000; // 120 BPM
            _loop = true;
            _midiOut = null;
        }

        public void LoadMidiFile(string filePath)
        {
            try
            {
                var midiFile = new MidiFile(filePath);
                _ticksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote;
                _events.Clear();

                foreach (var track in midiFile.Events)
                {
                    foreach (var midiEvent in track)
                    {
                        if (midiEvent is TempoEvent tempo)
                        {
                            _microsecondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote;
                        }
                        else if (midiEvent is NoteOnEvent || midiEvent is NoteEvent || midiEvent is ControlChangeEvent)
                        {
                            _events.Add(midiEvent);
                        }
                    }
                }

                _events = _events.OrderBy(e => e.AbsoluteTime).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки MIDI файла: {ex.Message}");
            }
        }

        public void SetSampleRate(double sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public void Play()
        {
            if (_midiOut == null)
            {
                try
                {
                    if (MidiOut.NumberOfDevices > 0)
                    {
                        _midiOut = new MidiOut(0); // 0 = Microsoft GS Wavetable Synth
                    }
                }
                catch
                {
                    _midiOut = null;
                }
            }
            
            _isPlaying = true;
            _currentEventIndex = 0;
            _currentSampleTime = 0;
        }

        public void Stop()
        {
            _isPlaying = false;
            _currentEventIndex = 0;
            _currentSampleTime = 0;
            
            if (_midiOut != null)
            {
                try
                {
                    // Отправляем All Notes Off на все каналы (1-16)
                    for (int channel = 1; channel <= 16; channel++)
                    {
                        _midiOut.Send(MidiMessage.StopNote(0, 0, channel).RawData);
                    }
                    
                    _midiOut.Dispose();
                    _midiOut = null;
                }
                catch
                {
                    _midiOut = null;
                }
            }
        }
        
        ~MidiFilePlayer()
        {
            if (_midiOut != null)
            {
                _midiOut.Dispose();
                _midiOut = null;
            }
        }

        public void SetLoop(bool loop)
        {
            _loop = loop;
        }

        public List<byte[]> Process(int sampleCount)
        {
            var midiMessages = new List<byte[]>();

            if (!_isPlaying || _events.Count == 0)
                return midiMessages;

            long endSampleTime = _currentSampleTime + sampleCount;

            while (_currentEventIndex < _events.Count)
            {
                var midiEvent = _events[_currentEventIndex];
                long eventSampleTime = TicksToSamples(midiEvent.AbsoluteTime);

                if (eventSampleTime >= endSampleTime)
                    break;

                if (eventSampleTime >= _currentSampleTime)
                {
                    byte[] message = ConvertToVstMidi(midiEvent, (int)(eventSampleTime - _currentSampleTime));
                    if (message != null)
                    {
                        midiMessages.Add(message);
                        OnMidiEvent?.Invoke(message);
                        
                        // Воспроизводим через Windows MIDI
                        if (_midiOut != null)
                        {
                            SendToWindowsMidi(midiEvent);
                        }
                    }
                }

                _currentEventIndex++;
            }

            _currentSampleTime = endSampleTime;

            if (_currentEventIndex >= _events.Count)
            {
                if (_loop)
                {
                    _currentEventIndex = 0;
                    _currentSampleTime = 0;
                }
                else
                {
                    Stop();
                }
            }

            return midiMessages;
        }

        private long TicksToSamples(long ticks)
        {
            double ticksPerSecond = 1000000.0 / _microsecondsPerQuarterNote * _ticksPerQuarterNote;
            double seconds = ticks / ticksPerSecond;
            return (long)(seconds * _sampleRate);
        }

        private byte[] ConvertToVstMidi(MidiEvent midiEvent, int deltaFrames)
        {
            if (midiEvent is NoteOnEvent noteOn)
            {
                return new byte[]
                {
                    (byte)deltaFrames,
                    (byte)((deltaFrames >> 8) & 0xFF),
                    (byte)((deltaFrames >> 16) & 0xFF),
                    (byte)((deltaFrames >> 24) & 0xFF),
                    (byte)(0x90 | (noteOn.Channel - 1)),
                    (byte)noteOn.NoteNumber,
                    (byte)noteOn.Velocity,
                    0
                };
            }
            else if (midiEvent is NoteEvent noteOff && noteOff.CommandCode == MidiCommandCode.NoteOff)
            {
                return new byte[]
                {
                    (byte)deltaFrames,
                    (byte)((deltaFrames >> 8) & 0xFF),
                    (byte)((deltaFrames >> 16) & 0xFF),
                    (byte)((deltaFrames >> 24) & 0xFF),
                    (byte)(0x80 | (noteOff.Channel - 1)),
                    (byte)noteOff.NoteNumber,
                    0,
                    0
                };
            }
            else if (midiEvent is ControlChangeEvent cc)
            {
                return new byte[]
                {
                    (byte)deltaFrames,
                    (byte)((deltaFrames >> 8) & 0xFF),
                    (byte)((deltaFrames >> 16) & 0xFF),
                    (byte)((deltaFrames >> 24) & 0xFF),
                    (byte)(0xB0 | (cc.Channel - 1)),
                    (byte)cc.Controller,
                    (byte)cc.ControllerValue,
                    0
                };
            }

            return null;
        }

        private void SendToWindowsMidi(MidiEvent midiEvent)
        {
            try
            {
                if (midiEvent is NoteOnEvent noteOn)
                {
                    _midiOut.Send(MidiMessage.StartNote(noteOn.NoteNumber, noteOn.Velocity, noteOn.Channel + 1).RawData);
                }
                else if (midiEvent is NoteEvent noteOff && noteOff.CommandCode == MidiCommandCode.NoteOff)
                {
                    _midiOut.Send(MidiMessage.StopNote(noteOff.NoteNumber, 0, noteOff.Channel + 1).RawData);
                }
                else if (midiEvent is ControlChangeEvent cc)
                {
                    _midiOut.Send(MidiMessage.ChangeControl(cc.ControllerValue, (int)cc.Controller, cc.Channel + 1).RawData);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }
    }
}

