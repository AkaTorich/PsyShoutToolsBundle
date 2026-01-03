using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace PsyTrancePlayer.Test
{
    public class AudioTest
    {
        public static void TestAudio(string audioFilePath)
        {
            Console.WriteLine("=== Audio Diagnostic Test ===");
            Console.WriteLine($"Date: {DateTime.Now}");
            Console.WriteLine();

            // Test 1: Check audio devices
            Console.WriteLine("Test 1: Checking audio output devices...");
            int deviceCount = WaveOut.DeviceCount;
            Console.WriteLine($"  Found {deviceCount} audio output device(s)");

            if (deviceCount == 0)
            {
                Console.WriteLine("  ERROR: No audio output devices found!");
                Console.WriteLine("  Please check your system audio settings.");
                return;
            }

            for (int i = 0; i < deviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                Console.WriteLine($"  Device {i}: {caps.ProductName} (Channels: {caps.Channels})");
            }
            Console.WriteLine();

            // Test 2: Check file
            Console.WriteLine($"Test 2: Checking audio file...");
            Console.WriteLine($"  Path: {audioFilePath}");

            if (!File.Exists(audioFilePath))
            {
                Console.WriteLine("  ERROR: File not found!");
                return;
            }
            Console.WriteLine("  File exists: OK");

            var fileInfo = new FileInfo(audioFilePath);
            Console.WriteLine($"  File size: {fileInfo.Length / 1024 / 1024.0:F2} MB");
            Console.WriteLine($"  Extension: {Path.GetExtension(audioFilePath)}");
            Console.WriteLine();

            // Test 3: Try to load audio file
            Console.WriteLine("Test 3: Loading audio file...");
            AudioFileReader? reader = null;
            try
            {
                reader = new AudioFileReader(audioFilePath);
                Console.WriteLine("  AudioFileReader created: OK");
                Console.WriteLine($"  Duration: {reader.TotalTime}");
                Console.WriteLine($"  Sample Rate: {reader.WaveFormat.SampleRate} Hz");
                Console.WriteLine($"  Channels: {reader.WaveFormat.Channels}");
                Console.WriteLine($"  Bits Per Sample: {reader.WaveFormat.BitsPerSample}");
                Console.WriteLine($"  Encoding: {reader.WaveFormat.Encoding}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: Failed to load audio file!");
                Console.WriteLine($"  Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                return;
            }
            Console.WriteLine();

            // Test 4: Try to initialize WaveOut
            Console.WriteLine("Test 4: Initializing WaveOut...");
            WaveOutEvent? waveOut = null;
            try
            {
                waveOut = new WaveOutEvent();
                Console.WriteLine("  WaveOutEvent created: OK");

                waveOut.Init(reader);
                Console.WriteLine("  WaveOut initialized: OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: Failed to initialize WaveOut!");
                Console.WriteLine($"  Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                reader?.Dispose();
                return;
            }
            Console.WriteLine();

            // Test 5: Try to play
            Console.WriteLine("Test 5: Testing playback...");
            try
            {
                Console.WriteLine("  Starting playback...");
                waveOut.Play();
                Console.WriteLine($"  Playback state: {waveOut.PlaybackState}");

                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Console.WriteLine("  SUCCESS: Audio is playing!");
                    Console.WriteLine("  Playing for 3 seconds...");
                    Thread.Sleep(3000);

                    Console.WriteLine("  Stopping playback...");
                    waveOut.Stop();
                    Console.WriteLine("  Playback stopped.");
                }
                else
                {
                    Console.WriteLine($"  ERROR: Playback state is {waveOut.PlaybackState}, expected Playing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: Playback failed!");
                Console.WriteLine($"  Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
            }
            finally
            {
                waveOut?.Dispose();
                reader?.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("=== Test Complete ===");
        }
    }
}
