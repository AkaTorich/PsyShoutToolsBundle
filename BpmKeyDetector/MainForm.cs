using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MaterialSkin;
using MaterialSkin.Controls;

namespace BpmKeyDetector
{
    public partial class MainForm : MaterialForm
    {
        private List<AudioFileInfo> audioFiles = new List<AudioFileInfo>();
        private bool isProcessing = false;
        private const int BPM_ANALYZE_WINDOW_SECONDS = 30; // Увеличенное окно анализа для точности
        private const int BPM_MIN = 60;    // Минимальный искомый BPM
        private const int BPM_MAX = 200;   // Максимальный искомый BPM
        private const int BPM_STEPS = 280; // Количество шагов в диапазоне BPM
        private const int FFT_SIZE = 8192; // Размер окна FFT для анализа тональности
        private readonly float[] HannWindow;

        public MainForm()
        {
            InitializeComponent();

            // Настройка Material Design темы в стиле Cyberpunk/Synthwave
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            // Кастомная цветовая схема в стиле Cyberpunk
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Green400,      // Кислотно-зеленый основной цвет
                Primary.Green600,      // Темнее зеленый
                Primary.Green200,      // Светлее зеленый
                Accent.Cyan400,        // Неоновый голубой акцент
                TextShade.WHITE        // Белый текст
            );

            // Предварительное вычисление окна Ханна для FFT
            HannWindow = new float[FFT_SIZE];
            for (int i = 0; i < FFT_SIZE; i++)
            {
                HannWindow[i] = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (FFT_SIZE - 1)));
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (isProcessing)
                return;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Выберите папку с аудиофайлами";

            using (folderBrowser)
            {
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderBrowser.SelectedPath;
                    LoadAudioFiles(folderPath);
                }
            }
        }

        private void LoadAudioFiles(string folderPath)
        {
            // Поддерживаемые расширения файлов для NAudio
            string[] supportedExtensions = { ".mp3", ".wav", ".aiff", ".aif", ".m4a", ".flac" };

            try
            {
                var files = Directory.GetFiles(folderPath)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();

                audioFiles.Clear();
                listViewFiles.Items.Clear();

                foreach (var file in files)
                {
                    var fileInfo = new AudioFileInfo();
                    fileInfo.FilePath = file;
                    fileInfo.FileName = Path.GetFileName(file);
                    fileInfo.BPM = 0;
                    fileInfo.Key = "Неизвестно";
                    fileInfo.Status = "Ожидает анализа";

                    audioFiles.Add(fileInfo);

                    var item = new ListViewItem(fileInfo.FileName);
                    item.SubItems.Add("--");
                    item.SubItems.Add("--");
                    item.SubItems.Add("Ожидает анализа");
                    listViewFiles.Items.Add(item);
                }

                lblStatus.Text = $"Загружено {audioFiles.Count} аудиофайлов.";
                btnAnalyzeFiles.Enabled = audioFiles.Count > 0;
                btnRenameFiles.Enabled = false;
                btnFastRename.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файлов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnalyzeFiles_Click(object sender, EventArgs e)
        {
            if (isProcessing || audioFiles.Count == 0)
                return;

            isProcessing = true;
            btnSelectFolder.Enabled = false;
            btnAnalyzeFiles.Enabled = false;
            btnRenameFiles.Enabled = false;
            btnFastRename.Enabled = false;
            progressBar.Value = 0;
            progressBar.Maximum = audioFiles.Count;

            // Сбросить статус у всех файлов
            for (int i = 0; i < listViewFiles.Items.Count; i++)
            {
                listViewFiles.Items[i].SubItems[3].Text = "Ожидает анализа";
            }

            // Запуск анализа в фоновом потоке
            bgWorker.RunWorkerAsync();
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker == null)
                return;

            for (int i = 0; i < audioFiles.Count; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var fileInfo = audioFiles[i];
                try
                {
                    worker.ReportProgress(i, $"Анализируется: {fileInfo.FileName}");

                    // Анализируем BPM и тональность
                    AnalyzeAudioFile(fileInfo, worker, i);

                    fileInfo.Status = "Анализ завершен";
                    worker.ReportProgress(i, new ProgressInfo { Index = i, FileInfo = fileInfo, Status = "Анализ завершен" });
                }
                catch (Exception ex)
                {
                    fileInfo.Status = $"Ошибка: {ex.Message}";
                    worker.ReportProgress(i, new ProgressInfo { Index = i, FileInfo = fileInfo, Status = fileInfo.Status });
                }
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage + 1;

            if (e.UserState is string)
            {
                string message = (string)e.UserState;
                lblStatus.Text = message;
            }
            else if (e.UserState is ProgressInfo)
            {
                ProgressInfo progressInfo = (ProgressInfo)e.UserState;
                int index = progressInfo.Index;
                if (index >= 0 && index < listViewFiles.Items.Count)
                {
                    var item = listViewFiles.Items[index];
                    item.SubItems[1].Text = progressInfo.FileInfo.BPM.ToString("0.0");
                    item.SubItems[2].Text = progressInfo.FileInfo.Key;
                    item.SubItems[3].Text = progressInfo.Status;
                }
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isProcessing = false;
            btnSelectFolder.Enabled = true;
            btnAnalyzeFiles.Enabled = true;
            btnRenameFiles.Enabled = audioFiles.Count > 0;
            btnFastRename.Enabled = audioFiles.Count > 0;

            if (e.Cancelled)
            {
                lblStatus.Text = "Анализ отменен.";
            }
            else if (e.Error != null)
            {
                lblStatus.Text = $"Ошибка при анализе: {e.Error.Message}";
                MessageBox.Show($"Произошла ошибка: {e.Error.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                lblStatus.Text = "Анализ завершен. Можно переименовать файлы.";
            }
        }

        // Анализ аудиофайла
        private void AnalyzeAudioFile(AudioFileInfo fileInfo, BackgroundWorker worker, int fileIndex)
        {
            using (var audioFile = new AudioFileReader(fileInfo.FilePath))
            {
                // Установим статус для текущего этапа
                worker.ReportProgress(fileIndex, new ProgressInfo
                {
                    Index = fileIndex,
                    FileInfo = fileInfo,
                    Status = "Извлечение данных из файла..."
                });

                // Проверяем, есть ли BPM и Key в имени или метаданных
                float bpmFromName = GetBpmFromFileName(fileInfo.FileName);
                string keyFromName = GetKeyFromFileName(fileInfo.FileName);

                if (bpmFromName > 0)
                {
                    fileInfo.BPM = bpmFromName;
                    worker.ReportProgress(fileIndex, new ProgressInfo
                    {
                        Index = fileIndex,
                        FileInfo = fileInfo,
                        Status = "BPM получен из имени файла"
                    });
                }
                else
                {
                    // Определяем BPM через спектральный анализ
                    worker.ReportProgress(fileIndex, new ProgressInfo
                    {
                        Index = fileIndex,
                        FileInfo = fileInfo,
                        Status = "Анализ BPM..."
                    });

                    fileInfo.BPM = DetectBPM(audioFile);
                }

                if (keyFromName != "Неизвестно")
                {
                    fileInfo.Key = keyFromName;
                    worker.ReportProgress(fileIndex, new ProgressInfo
                    {
                        Index = fileIndex,
                        FileInfo = fileInfo,
                        Status = "Тональность получена из имени файла"
                    });
                }
                else
                {
                    // Определяем тональность через улучшенный спектральный анализ
                    worker.ReportProgress(fileIndex, new ProgressInfo
                    {
                        Index = fileIndex,
                        FileInfo = fileInfo,
                        Status = "Анализ тональности..."
                    });

                    try
                    {
                        fileInfo.Key = DetectKeyEnhanced(audioFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при определении тональности: {ex.Message}");

                        // Если возникла ошибка, используем запасной метод
                        fileInfo.Key = DetectKeyUsingImprovedSpectralAnalysis(audioFile);
                    }
                }
            }
        }

        #region BPM Detection

        // Определение BPM с использованием энергетического спектра
        private float DetectBPM(AudioFileReader audioFile)
        {
            try
            {
                // Необходимые параметры
                var sampleRate = audioFile.WaveFormat.SampleRate;
                var channels = audioFile.WaveFormat.Channels;
                var totalDuration = audioFile.TotalTime.TotalSeconds;

                // Сбрасываем позицию файла
                audioFile.Position = 0;

                Console.WriteLine($"Начало анализа BPM: длительность трека {totalDuration}с");

                // Алгоритм использует относительно короткий фрагмент (120 сек максимум)
                // для эффективности, но берет его из середины трека
                int analyzeSeconds = (int)Math.Min(120, totalDuration);

                // Рассчитываем начальную позицию, чтобы взять фрагмент из середины трека
                int startPos = Math.Max(0, (int)((totalDuration - analyzeSeconds) / 2));

                // Устанавливаем позицию в середину трека
                audioFile.CurrentTime = TimeSpan.FromSeconds(startPos);

                // Читаем фрагмент аудио
                int numSamples = analyzeSeconds * sampleRate;
                float[] audioSamples = new float[numSamples];
                int samplesRead = audioFile.Read(audioSamples, 0, numSamples);

                // Если прочитано недостаточно данных, пробуем еще раз с начала файла
                if (samplesRead < numSamples / 2)
                {
                    audioFile.Position = 0;
                    samplesRead = audioFile.Read(audioSamples, 0, numSamples);
                }

                // Конвертируем в моно
                float[] monoSamples = new float[samplesRead / channels];
                for (int i = 0, j = 0; i < samplesRead; i += channels, j++)
                {
                    float sum = 0;
                    for (int ch = 0; ch < channels; ch++)
                    {
                        if (i + ch < samplesRead)
                            sum += audioSamples[i + ch];
                    }
                    if (j < monoSamples.Length)
                        monoSamples[j] = sum / channels;
                }

                // Вычисляем BPM через повторяющиеся удары
                float bpm = ComputeBpmByEnergySpectrum(monoSamples, sampleRate);

                // Округляем BPM до целого числа
                bpm = (float)Math.Round(bpm);

                Console.WriteLine($"Обнаружен BPM: {bpm}");
                return bpm;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при определении BPM: {ex.Message}");
                return 128.0f; // Значение по умолчанию
            }
        }

        // Метод определения BPM через энергетический спектр
        private float ComputeBpmByEnergySpectrum(float[] samples, int sampleRate)
        {
            Console.WriteLine("Начало расчета BPM через энергетический спектр");

            try
            {
                // Шаг 1: Конвертируем аудио в огибающую энергии в низкой дискретизации (для ускорения)
                int energyRate = 100; // 100 Гц - достаточно для ритм-детекции
                float[] energyEnvelope = CalculateEnergyEnvelope(samples, sampleRate, energyRate);

                // Шаг 2: Вычисляем спектр огибающей (для поиска периодичности)
                int fftSize = NextPowerOfTwo(energyEnvelope.Length);
                Console.WriteLine($"Размер FFT для спектра: {fftSize}");

                NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[fftSize];
                for (int i = 0; i < fftSize; i++)
                {
                    if (i < energyEnvelope.Length)
                    {
                        fftBuffer[i].X = energyEnvelope[i];
                    }
                    else
                    {
                        fftBuffer[i].X = 0;
                    }
                    fftBuffer[i].Y = 0;
                }

                // Применяем FFT
                NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2.0), fftBuffer);

                // Шаг 3: Анализируем спектр для обнаружения пиков, соответствующих BPM
                // BPM = 60 * частота
                float[] magnitudes = new float[fftSize / 2];
                for (int i = 0; i < fftSize / 2; i++)
                {
                    magnitudes[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
                }

                // Шаг 4: Преобразуем в BPM-спектр и ищем пики
                int minBpmBin = (int)(Math.Floor(BPM_MIN * fftSize / (60.0 * energyRate)));
                int maxBpmBin = (int)(Math.Ceiling(BPM_MAX * fftSize / (60.0 * energyRate)));

                // Ограничиваем bins диапазоном
                minBpmBin = Math.Max(1, minBpmBin); // начинаем с 1, пропуская DC компонент
                maxBpmBin = Math.Min(fftSize / 2 - 1, maxBpmBin);

                Console.WriteLine($"Поиск пиков в диапазоне BPM bins: {minBpmBin}-{maxBpmBin}");

                // Ищем пики в спектре
                List<BpmPeak> peaks = new List<BpmPeak>();
                for (int i = minBpmBin + 1; i < maxBpmBin - 1; i++)
                {
                    if (magnitudes[i] > magnitudes[i - 1] && magnitudes[i] > magnitudes[i + 1])
                    {
                        float bpm = i * 60 * energyRate / (float)fftSize;
                        if (bpm >= BPM_MIN && bpm <= BPM_MAX)
                        {
                            BpmPeak peak = new BpmPeak();
                            peak.Bpm = bpm;
                            peak.Energy = magnitudes[i];
                            peaks.Add(peak);
                        }
                    }
                }

                // Если нет пиков, возвращаем значение по умолчанию
                if (peaks.Count == 0)
                {
                    Console.WriteLine("Не найдено пиков в BPM спектре. Возвращаем 128 BPM");
                    return 128.0f;
                }

                // Сортируем пики по энергии
                peaks.Sort((a, b) => b.Energy.CompareTo(a.Energy));

                // Проверка на кратность
                float primaryBpm = peaks[0].Bpm;
                Console.WriteLine($"Первичный кандидат BPM: {primaryBpm}, энергия: {peaks[0].Energy}");

                if (peaks.Count > 1)
                {
                    // Проверяем, есть ли близкие пики с удвоенной/уполовиненной частотой
                    foreach (var peak in peaks.Skip(1).Take(5))
                    {
                        Console.WriteLine($"Доп. кандидат BPM: {peak.Bpm}, энергия: {peak.Energy}");

                        // Если BPM слишком низкий, и есть пик с удвоенным BPM
                        if (primaryBpm < 90 &&
                            Math.Abs(peak.Bpm - primaryBpm * 2) < 5 &&
                            peak.Energy > peaks[0].Energy * 0.4)
                        {
                            Console.WriteLine($"Коррекция: Низкий BPM {primaryBpm} заменен на {peak.Bpm} (x2)");
                            return peak.Bpm;
                        }

                        // Если BPM слишком высокий, и есть пик с уполовиненным BPM
                        if (primaryBpm > 160 &&
                            Math.Abs(peak.Bpm - primaryBpm / 2) < 5 &&
                            peak.Energy > peaks[0].Energy * 0.2)
                        {
                            Console.WriteLine($"Коррекция: Высокий BPM {primaryBpm} заменен на {peak.Bpm} (x0.5)");
                            return peak.Bpm;
                        }
                    }
                }

                // Коррекция для очень высоких/низких значений
                if (primaryBpm > 185) return primaryBpm / 2;
                if (primaryBpm < 65) return primaryBpm * 2;

                return primaryBpm;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ComputeBpmByEnergySpectrum: {ex.Message}");
                return 128.0f;
            }
        }

        // Вычисление огибающей энергии
        private float[] CalculateEnergyEnvelope(float[] samples, int sampleRate, int targetRate)
        {
            // Определяем размер блока для понижения дискретизации
            int blockSize = sampleRate / targetRate;
            int numBlocks = samples.Length / blockSize;
            float[] result = new float[numBlocks];

            Parallel.For(0, numBlocks, i =>
            {
                float sum = 0;
                int startIdx = i * blockSize;
                int endIdx = Math.Min(startIdx + blockSize, samples.Length);

                for (int j = startIdx; j < endIdx; j++)
                {
                    // Используем квадрат амплитуды для выделения энергии
                    sum += samples[j] * samples[j];
                }

                // Извлекаем квадратный корень для получения RMS энергии
                result[i] = (float)Math.Sqrt(sum / blockSize);
            });

            // Сглаживающий фильтр для удаления высоких частот
            return LowPassFilter(result, 10, targetRate);
        }

        // Вспомогательный метод для расчета следующей степени 2
        private int NextPowerOfTwo(int value)
        {
            int power = 1;
            while (power < value)
            {
                power *= 2;
            }
            return power;
        }

        // Фильтр низких частот для выделения басов и ударных
        private float[] LowPassFilter(float[] samples, float cutoffFreq, int sampleRate)
        {
            float[] output = new float[samples.Length];
            double RC = 1.0 / (cutoffFreq * 2 * Math.PI);
            double dt = 1.0 / sampleRate;
            double alpha = dt / (RC + dt);

            output[0] = samples[0];
            for (int i = 1; i < samples.Length; i++)
            {
                output[i] = (float)(output[i - 1] + alpha * (samples[i] - output[i - 1]));
            }

            return output;
        }

        // Фильтр высоких частот
        private float[] ApplyHighPassFilter(float[] samples, float cutoffFrequency, int sampleRate)
        {
            float[] output = new float[samples.Length];

            // Простой фильтр первого порядка
            double RC = 1.0 / (cutoffFrequency * 2 * Math.PI);
            double dt = 1.0 / sampleRate;
            double alpha = RC / (RC + dt);

            // Применяем фильтр
            output[0] = samples[0];
            for (int i = 1; i < samples.Length; i++)
            {
                output[i] = (float)(alpha * (output[i - 1] + samples[i] - samples[i - 1]));
            }

            return output;
        }

        // Вспомогательный класс для хранения информации о пиках BPM
        private class BpmPeak
        {
            public float Bpm { get; set; }
            public float Energy { get; set; }
        }

        #endregion

        #region Enhanced Key Detection

        /// <summary>
        /// Улучшенное определение тональности с учетом формата файла
        /// </summary>
        private string DetectKeyEnhanced(AudioFileReader audioFile)
        {
            // Используем подход с голосованием для более стабильного определения
            Dictionary<string, int> keyVotes = new Dictionary<string, int>();

            try
            {
                // Анализируем несколько сегментов файла
                double duration = audioFile.TotalTime.TotalSeconds;

                // Позиции для анализа (относительно продолжительности)
                double[] positions = { 0.2, 0.4, 0.6, 0.8 };

                // Определяем формат файла для адаптации параметров
                string extension = Path.GetExtension(audioFile.FileName).ToLower();
                float filterStrength = 0.6f; // Значение по умолчанию

                // Настраиваем параметры в зависимости от формата
                switch (extension)
                {
                    case ".mp3":
                        // Для MP3 используем более агрессивную фильтрацию
                        filterStrength = 0.8f;
                        break;
                    case ".flac":
                    case ".wav":
                    case ".aiff":
                    case ".aif":
                        // Для несжатых форматов нужна меньшая фильтрация
                        filterStrength = 0.4f;
                        break;
                    default:
                        // Для других форматов используем средние параметры
                        filterStrength = 0.6f;
                        break;
                }

                // Анализируем разные участки трека
                foreach (double position in positions)
                {
                    string key = AnalyzeSegment(audioFile, position * duration, filterStrength);
                    if (key != "Неизвестно")
                    {
                        if (!keyVotes.ContainsKey(key)) keyVotes[key] = 0;
                        keyVotes[key]++;
                    }
                }

                // Анализируем басовые ноты, особенно важно для электронной музыки
                string bassKey = AnalyzeBassNotes(audioFile);
                if (bassKey != "Неизвестно")
                {
                    // Даем больший вес басовой линии (2 голоса)
                    if (!keyVotes.ContainsKey(bassKey)) keyVotes[bassKey] = 0;
                    keyVotes[bassKey] += 2;
                }

                // Выбираем тональность с наибольшим числом голосов
                if (keyVotes.Count > 0)
                {
                    var bestKey = keyVotes.OrderByDescending(kv => kv.Value).First();
                    Console.WriteLine($"Тональность определена как {bestKey.Key} (голосов: {bestKey.Value})");
                    return bestKey.Key;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расширенном определении тональности: {ex.Message}");
            }

            // Если не удалось определить тональность или произошла ошибка,
            // возвращаемся к основному методу
            return DetectKeyUsingImprovedSpectralAnalysis(audioFile);
        }

        /// <summary>
        /// Анализ сегмента аудиофайла для определения тональности
        /// </summary>
        private string AnalyzeSegment(AudioFileReader audioFile, double startPositionSeconds, float filterStrength)
        {
            try
            {
                // Позиционируем в указанную точку
                audioFile.CurrentTime = TimeSpan.FromSeconds(startPositionSeconds);

                // Читаем 10 секунд аудио или до конца файла
                int analyzeSeconds = 10;
                int sampleRate = audioFile.WaveFormat.SampleRate;
                int channels = audioFile.WaveFormat.Channels;
                int bufferSize = analyzeSeconds * sampleRate * channels;

                float[] buffer = new float[bufferSize];
                int samplesRead = audioFile.Read(buffer, 0, bufferSize);

                if (samplesRead < sampleRate * channels * 3) // Минимум 3 секунды
                    return "Неизвестно";

                // Конвертируем в моно
                float[] monoData = new float[samplesRead / channels];
                for (int i = 0, j = 0; i < samplesRead; i += channels, j++)
                {
                    float sum = 0;
                    for (int ch = 0; ch < channels && i + ch < samplesRead; ch++)
                        sum += buffer[i + ch];

                    if (j < monoData.Length)
                        monoData[j] = sum / channels;
                }

                // Предобработка сигнала
                monoData = PreprocessAudio(monoData, sampleRate, filterStrength);

                // Анализируем сегмент
                return DetectKeyUsingImprovedSpectralAnalysis(monoData, sampleRate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при анализе сегмента: {ex.Message}");
                return "Неизвестно";
            }
        }

        /// <summary>
        /// Улучшенная предобработка аудио
        /// </summary>
        private float[] PreprocessAudio(float[] samples, int sampleRate, float filterStrength)
        {
            // 1. Применить фильтр низких частот
            float cutoffFrequency = 2000 + (1.0f - filterStrength) * 6000; // От 2000 до 8000 Гц
            float[] filtered = LowPassFilter(samples, cutoffFrequency, sampleRate);

            // 2. Нормализация громкости
            float[] normalized = NormalizeAmplitude(filtered);

            // 3. Усиление гармоник (эмпирическая формула)
            float powerFactor = 0.5f + filterStrength * 0.5f; // От 0.5 до 1.0
            for (int i = 0; i < normalized.Length; i++)
            {
                normalized[i] = Math.Sign(normalized[i]) * (float)Math.Pow(Math.Abs(normalized[i]), powerFactor);
            }

            return normalized;
        }

        /// <summary>
        /// Нормализация амплитуды аудиосигнала
        /// </summary>
        private float[] NormalizeAmplitude(float[] samples)
        {
            float[] normalized = new float[samples.Length];

            // Находим максимальную амплитуду
            float maxAmp = 0.00001f; // Избегаем деления на ноль
            for (int i = 0; i < samples.Length; i++)
            {
                float abs = Math.Abs(samples[i]);
                if (abs > maxAmp) maxAmp = abs;
            }

            // Нормализуем
            for (int i = 0; i < samples.Length; i++)
            {
                normalized[i] = samples[i] / maxAmp;
            }

            return normalized;
        }

        /// <summary>
        /// Анализ басовых нот для определения тональности
        /// </summary>
        private string AnalyzeBassNotes(AudioFileReader audioFile)
        {
            try
            {
                // Сбрасываем позицию файла
                audioFile.Position = 0;
                int sampleRate = audioFile.WaveFormat.SampleRate;
                int channels = audioFile.WaveFormat.Channels;

                // Читаем 20 секунд или меньше, если файл короче
                int analyzeSeconds = 20;
                int bufferSize = analyzeSeconds * sampleRate * channels;
                float[] buffer = new float[bufferSize];
                int samplesRead = audioFile.Read(buffer, 0, bufferSize);

                if (samplesRead < sampleRate * channels * 5) // Минимум 5 секунд
                    return "Неизвестно";

                // Конвертируем в моно
                float[] monoData = new float[samplesRead / channels];
                for (int i = 0, j = 0; i < samplesRead; i += channels, j++)
                {
                    float sum = 0;
                    for (int ch = 0; ch < channels && i + ch < samplesRead; ch++)
                        sum += buffer[i + ch];

                    if (j < monoData.Length)
                        monoData[j] = sum / channels;
                }

                // Применяем фильтр очень низких частот (20-200 Гц) для выделения баса
                float[] bassLine = LowPassFilter(monoData, 200, sampleRate);
                bassLine = ApplyHighPassFilter(bassLine, 20, sampleRate);

                // Анализируем басовую линию с большим FFT для лучшего разрешения низких частот
                int fftSize = 32768; // Большое окно для точности низких частот

                // Хроматический профиль для басовых нот
                double[] bassChromaProfile = new double[12];

                // Анализируем окна с перекрытием
                int hopSize = fftSize / 4; // 75% перекрытие
                int numWindows = (bassLine.Length - fftSize) / hopSize + 1;

                if (numWindows <= 0) numWindows = 1;

                for (int w = 0; w < numWindows; w++)
                {
                    int startSample = w * hopSize;
                    if (startSample + fftSize > bassLine.Length) break;

                    // Копируем данные для анализа
                    float[] windowData = new float[fftSize];
                    Array.Copy(bassLine, startSample, windowData, 0, fftSize);

                    // Применяем окно Ханна
                    for (int i = 0; i < fftSize; i++)
                    {
                        windowData[i] *= (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / (fftSize - 1))));
                    }

                    // Выполняем FFT
                    NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[fftSize];
                    for (int i = 0; i < fftSize; i++)
                    {
                        fftBuffer[i].X = windowData[i];
                        fftBuffer[i].Y = 0;
                    }

                    NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2.0), fftBuffer);

                    // Анализируем только низкие частоты (20-300 Гц)
                    int minBin = (int)(20 * fftSize / sampleRate);
                    int maxBin = (int)(300 * fftSize / sampleRate);

                    // Базовая частота ноты A0 (самая низкая на фортепиано)
                    double baseFreq = 27.5;

                    // Обрабатываем бины в интересующем нас диапазоне
                    for (int i = minBin; i <= maxBin; i++)
                    {
                        double frequency = i * sampleRate / (double)fftSize;

                        // Только если частота в диапазоне басовых нот
                        if (frequency >= 20 && frequency <= 300)
                        {
                            // Магнитуда
                            double magnitude = Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);

                            // Преобразуем частоту в ноту
                            double noteNum = 12 * Math.Log(frequency / baseFreq, 2.0);

                            // Получаем хроматический индекс (0-11)
                            int chromatoNum = (int)Math.Round(noteNum) % 12;
                            if (chromatoNum < 0) chromatoNum += 12;

                            // Добавляем энергию к хроматическому профилю
                            bassChromaProfile[chromatoNum] += magnitude;
                        }
                    }
                }

                // Нормализуем хроматический профиль
                double maxVal = bassChromaProfile.Max();
                if (maxVal > 0)
                {
                    for (int i = 0; i < 12; i++)
                        bassChromaProfile[i] /= maxVal;
                }

                // Найдем ноту с максимальной энергией (наиболее вероятный бас)
                int maxNoteIndex = 0;
                double maxEnergy = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (bassChromaProfile[i] > maxEnergy)
                    {
                        maxEnergy = bassChromaProfile[i];
                        maxNoteIndex = i;
                    }
                }

                // Определяем, мажор или минор
                // (простой эвристический подход - проверяем относительную энергию терций)
                int majorThirdIndex = (maxNoteIndex + 4) % 12;
                int minorThirdIndex = (maxNoteIndex + 3) % 12;

                bool isMajor = bassChromaProfile[majorThirdIndex] > bassChromaProfile[minorThirdIndex];

                // Названия нот
                string[] noteNames = {
            "A", "A#", "B", "C", "C#", "D",
            "D#", "E", "F", "F#", "G", "G#"
        };

                return noteNames[maxNoteIndex] + (isMajor ? " maj" : " min");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при анализе басовых нот: {ex.Message}");
                return "Неизвестно";
            }
        }

        private string DetectKeyUsingImprovedSpectralAnalysis(AudioFileReader audioFile)
        {
            try
            {
                // Сбрасываем позицию файла
                audioFile.Position = 0;
                int sampleRate = audioFile.WaveFormat.SampleRate;
                int channels = audioFile.WaveFormat.Channels;

                // Читаем из середины файла для лучшего результата
                double duration = audioFile.TotalTime.TotalSeconds;
                int analyzeSeconds = Math.Min(30, (int)duration);
                double startPosition = Math.Max(0, (duration - analyzeSeconds) / 2);

                audioFile.CurrentTime = TimeSpan.FromSeconds(startPosition);

                // Читаем фрагмент
                int bufferSize = analyzeSeconds * sampleRate * channels;
                float[] buffer = new float[bufferSize];
                int samplesRead = audioFile.Read(buffer, 0, bufferSize);

                if (samplesRead == 0)
                    return "Неизвестно";

                // Конвертируем в моно
                float[] monoData = new float[samplesRead / channels];
                for (int i = 0, j = 0; i < samplesRead; i += channels, j++)
                {
                    float sum = 0;
                    for (int ch = 0; ch < channels && i + ch < samplesRead; ch++)
                        sum += buffer[i + ch];

                    if (j < monoData.Length)
                        monoData[j] = sum / channels;
                }

                // Анализируем с нашим улучшенным методом
                return DetectKeyUsingImprovedSpectralAnalysis(monoData, sampleRate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в спектральном анализе аудиофайла: {ex.Message}");
                return "Неизвестно";
            }
        }

        // Улучшенный спектральный анализ
        private string DetectKeyUsingImprovedSpectralAnalysis(float[] samples, int sampleRate)
        {
            try
            {
                Console.WriteLine("Выполняем улучшенный анализ тональности через спектр...");

                // Увеличенный размер FFT для лучшего разрешения
                int fftSize = 32768;

                // Разбиваем на окна с перекрытием для лучшего результата
                int windowSize = Math.Min(fftSize, samples.Length);
                int hopSize = windowSize / 2; // 50% перекрытие
                int numWindows = (samples.Length - windowSize) / hopSize + 1;

                if (numWindows <= 0) numWindows = 1;

                // Создаем массив для хроматического профиля
                double[] chromaProfile = new double[12];

                // Частоты нот для нескольких октав
                double baseFreq = 27.5; // A0

                // Названия нот
                string[] noteNames = {
                    "A", "A#", "B", "C", "C#", "D",
                    "D#", "E", "F", "F#", "G", "G#"
                };

                Console.WriteLine($"Анализируем {numWindows} окон данных...");

                // Обрабатываем окна с перекрытием
                for (int w = 0; w < numWindows; w++)
                {
                    int startSample = w * hopSize;
                    if (startSample + windowSize > samples.Length) break;

                    // Копируем данные для анализа
                    float[] windowData = new float[fftSize];
                    for (int i = 0; i < windowSize; i++)
                    {
                        if (startSample + i < samples.Length)
                            windowData[i] = samples[startSample + i];
                    }

                    // Применяем окно Ханна для уменьшения утечки спектра
                    for (int i = 0; i < windowSize; i++)
                    {
                        windowData[i] *= (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / (windowSize - 1))));
                    }

                    // Заполняем массив для FFT
                    NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[fftSize];
                    for (int i = 0; i < fftSize; i++)
                    {
                        fftBuffer[i].X = (i < windowSize) ? windowData[i] : 0;
                        fftBuffer[i].Y = 0;
                    }

                    // Выполняем FFT
                    NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2.0), fftBuffer);

                    // Вычисляем хроматический профиль - более точный метод
                    // Частотный диапазон: 27.5 Гц (A0) до 4186.0 Гц (C8)
                    for (int i = 1; i < fftSize / 2; i++)
                    {
                        double frequency = i * sampleRate / (double)fftSize;

                        if (frequency < 27.5 || frequency > 4186.0)
                            continue;

                        // Магнитуда
                        double magnitude = Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);

                        // Вычисляем логарифм по основанию 2
                        double noteNum = 12 * Math.Log(frequency / baseFreq, 2.0);
                        int chromatoNum = (int)Math.Round(noteNum) % 12;

                        // Взвешиваем вклад в зависимости от близости к точной частоте ноты
                        double fractPart = Math.Abs(noteNum - Math.Round(noteNum));
                        double weight = 1.0 - fractPart; // Чем ближе к центру ноты, тем выше вес
                        weight = Math.Max(0, weight * weight); // Квадратичное взвешивание

                        if (chromatoNum >= 0 && chromatoNum < 12)
                            chromaProfile[chromatoNum] += magnitude * weight;
                    }
                }

                // Нормализуем хроматический профиль
                double maxVal = chromaProfile.Max();
                if (maxVal > 0)
                {
                    for (int i = 0; i < 12; i++)
                        chromaProfile[i] /= maxVal;
                }

                // Более точные шаблоны для мажора и минора
                // Эти значения основаны на теоретических и эмпирических данных
                double[] majorTemplate = { 1.0, 0.0, 0.5, 0.0, 0.8, 0.0, 0.4, 0.8, 0.0, 0.6, 0.0, 0.4 };
                double[] minorTemplate = { 1.0, 0.0, 0.4, 0.8, 0.0, 0.5, 0.0, 0.8, 0.4, 0.0, 0.6, 0.0 };

                // Улучшенная корреляция с шаблонами
                double bestCorr = -1;
                int bestKey = 0;
                bool isMajor = true;

                Console.WriteLine("Хроматический профиль:");
                for (int i = 0; i < 12; i++)
                {
                    Console.WriteLine($"  {noteNames[i]}: {chromaProfile[i]:F4}");
                }

                // Перебираем все возможные тональности
                for (int key = 0; key < 12; key++)
                {
                    // Проверка корреляции с мажорным шаблоном
                    double majorCorr = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        int idx = (i + key) % 12;
                        majorCorr += chromaProfile[idx] * majorTemplate[i];
                    }
                    majorCorr /= 12; // Нормализуем

                    // Проверка корреляции с минорным шаблоном
                    double minorCorr = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        int idx = (i + key) % 12;
                        minorCorr += chromaProfile[idx] * minorTemplate[i];
                    }
                    minorCorr /= 12; // Нормализуем

                    Console.WriteLine($"Корреляция для {noteNames[key]}: мажор={majorCorr:F4}, минор={minorCorr:F4}");

                    // Обновляем лучшую тональность
                    if (majorCorr > bestCorr)
                    {
                        bestCorr = majorCorr;
                        bestKey = key;
                        isMajor = true;
                    }

                    if (minorCorr > bestCorr)
                    {
                        bestCorr = minorCorr;
                        bestKey = key;
                        isMajor = false;
                    }
                }

                // Формируем название тональности
                string keyName = noteNames[bestKey] + (isMajor ? " maj" : " min");
                Console.WriteLine($"Определена тональность: {keyName} (корреляция: {bestCorr:F4})");

                return keyName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в улучшенном спектральном анализе: {ex.Message}");
                return "Неизвестно";
            }
        }

        #endregion

        #region File Analysis and Renaming

        // Пытаемся извлечь BPM из имени файла
        private float GetBpmFromFileName(string fileName)
        {
            try
            {
                string pattern = @"(\d+\.?\d*)(?:\s*(?:BPM|bpm))";
                var match = Regex.Match(fileName, pattern);

                if (match.Success && float.TryParse(match.Groups[1].Value, out float bpm))
                {
                    if (bpm >= BPM_MIN && bpm <= BPM_MAX)
                        return bpm;
                }

                // Ищем также в формате [BPM123]
                pattern = @"\[(?:BPM|bpm)\s*(\d+\.?\d*)\]";
                match = Regex.Match(fileName, pattern);

                if (match.Success && float.TryParse(match.Groups[1].Value, out bpm))
                {
                    if (bpm >= BPM_MIN && bpm <= BPM_MAX)
                        return bpm;
                }

                // Еще один вариант: 123BPM
                pattern = @"(\d+\.?\d*)(?:BPM|bpm)";
                match = Regex.Match(fileName, pattern);

                if (match.Success && float.TryParse(match.Groups[1].Value, out bpm))
                {
                    if (bpm >= BPM_MIN && bpm <= BPM_MAX)
                        return bpm;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении BPM из имени файла: {ex.Message}");
            }

            return 0;
        }

        // Переписываем метод GetKeyFromFileName для извлечения только корневой ноты
        private string GetKeyFromFileName(string fileName)
        {
            try
            {
                // Регулярное выражение для поиска формата Key X (где X может быть C, C#, G, и т.д.)
                string pattern = @"[Kk]ey\s*([A-G][b#]?\s*(?:maj|min|major|minor|M|m)?)";
                var match = Regex.Match(fileName, pattern);

                if (match.Success && match.Groups.Count > 1)
                {
                    string keyString = match.Groups[1].Value.Trim();

                    // Проверяем, содержит ли строка информацию о режиме (мажор/минор)
                    if (!keyString.Contains("maj") && !keyString.Contains("min") &&
                        !keyString.Contains("M") && !keyString.Contains("m"))
                    {
                        // Если режим не указан, считаем мажорным по умолчанию
                        keyString += " maj";
                    }

                    return keyString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении ключа из имени файла: {ex.Message}");
            }

            return "Неизвестно";
        }

        #endregion

        #region File Renaming

        private void btnRenameFiles_Click(object sender, EventArgs e)
        {
            if (isProcessing || audioFiles.Count == 0)
                return;

            try
            {
                int renamedCount = 0;
                int errorCount = 0;

                foreach (var fileInfo in audioFiles)
                {
                    // Пропускаем файлы без результатов анализа
                    if (fileInfo.BPM <= 0 || fileInfo.Key == "Неизвестно")
                        continue;

                    string directory = Path.GetDirectoryName(fileInfo.FilePath);
                    if (directory == null)
                        continue;

                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FilePath);
                    string extension = Path.GetExtension(fileInfo.FilePath);

                    // Проверяем, есть ли уже информация о BPM и Key в имени файла
                    if (!fileNameWithoutExt.Contains(" [") || !fileNameWithoutExt.Contains("BPM") || !fileNameWithoutExt.Contains("Key"))
                    {
                        // Формируем новое имя файла
                        string newFileName = $"{fileNameWithoutExt} [BPM {fileInfo.BPM:0.0} - Key {fileInfo.Key}]{extension}";
                        string newFilePath = Path.Combine(directory, newFileName);

                        try
                        {
                            // Переименовываем файл
                            File.Move(fileInfo.FilePath, newFilePath);
                            fileInfo.FilePath = newFilePath;
                            fileInfo.FileName = newFileName;
                            fileInfo.Status = "Файл переименован";
                            renamedCount++;
                        }
                        catch (Exception ex)
                        {
                            fileInfo.Status = $"Ошибка при переименовании: {ex.Message}";
                            errorCount++;
                        }
                    }
                    else
                    {
                        fileInfo.Status = "Файл уже содержит информацию о BPM и Key";
                    }
                }

                // Обновляем статус в интерфейсе
                RefreshListView();

                string message = $"Файлы переименованы: {renamedCount}";
                if (errorCount > 0)
                    message += $", с ошибками: {errorCount}";

                lblStatus.Text = message;
                MessageBox.Show(message, "Результат переименования", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переименовании файлов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshListView()
        {
            listViewFiles.BeginUpdate();
            listViewFiles.Items.Clear();

            foreach (var fileInfo in audioFiles)
            {
                var item = new ListViewItem(fileInfo.FileName);
                item.SubItems.Add(fileInfo.BPM.ToString("0.0"));
                item.SubItems.Add(fileInfo.Key);
                item.SubItems.Add(fileInfo.Status);
                listViewFiles.Items.Add(item);
            }

            listViewFiles.EndUpdate();
        }

        private void btnFastRename_Click(object sender, EventArgs e)
        {
            if (isProcessing || audioFiles.Count == 0)
                return;

            try
            {
                string folderPath = Path.GetDirectoryName(audioFiles[0].FilePath);
                if (folderPath == null)
                    return;

                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.Description = "Выберите папку для переименованных файлов";
                folderBrowser.SelectedPath = folderPath;

                using (folderBrowser)
                {
                    if (folderBrowser.ShowDialog() == DialogResult.OK)
                    {
                        string targetFolder = folderBrowser.SelectedPath;
                        int renamedCount = 0;
                        int errorCount = 0;

                        foreach (var fileInfo in audioFiles)
                        {
                            // Пропускаем файлы без результатов анализа
                            if (fileInfo.BPM <= 0 || fileInfo.Key == "Неизвестно")
                                continue;

                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FilePath);
                            string extension = Path.GetExtension(fileInfo.FilePath);

                            // Проверяем, есть ли уже информация о BPM и Key в имени файла
                            if (!fileNameWithoutExt.Contains("[BPM") || !fileNameWithoutExt.Contains("Key"))
                            {
                                // Формируем новое имя файла
                                string newFileName = $"{fileNameWithoutExt} [BPM {fileInfo.BPM:0.0} - Key {fileInfo.Key}]{extension}";
                                string newFilePath = Path.Combine(targetFolder, newFileName);

                                try
                                {
                                    // Копируем файл с новым именем
                                    File.Copy(fileInfo.FilePath, newFilePath, false);
                                    fileInfo.Status = "Файл скопирован с новым именем";
                                    renamedCount++;
                                }
                                catch (Exception ex)
                                {
                                    fileInfo.Status = $"Ошибка при копировании: {ex.Message}";
                                    errorCount++;
                                }
                            }
                            else
                            {
                                fileInfo.Status = "Файл уже содержит информацию о BPM и Key";
                            }
                        }

                        // Обновляем статус в интерфейсе
                        RefreshListView();

                        string message = $"Файлы скопированы с новыми именами: {renamedCount}";
                        if (errorCount > 0)
                            message += $", с ошибками: {errorCount}";

                        lblStatus.Text = message;
                        MessageBox.Show(message, "Результат копирования", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с файлами: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ListView Custom Drawing (Cyberpunk/Synthwave Style)

        private void listViewFiles_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            // Кислотно-зеленый градиент для заголовка
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                e.Bounds,
                Color.FromArgb(102, 255, 102), // Кислотно-зеленый
                Color.FromArgb(0, 230, 118),    // Темнее зеленый
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Обводка
            using (var pen = new Pen(Color.FromArgb(0, 255, 200), 1))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }

            // Текст заголовка
            TextRenderer.DrawText(
                e.Graphics,
                e.Header?.Text ?? "",
                new Font("Segoe UI", 9, FontStyle.Bold),
                e.Bounds,
                Color.FromArgb(20, 20, 20),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void listViewFiles_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void listViewFiles_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null || e.SubItem == null)
                return;

            // Цвет фона - чередование строк
            Color backColor = e.ItemIndex % 2 == 0
                ? Color.FromArgb(30, 30, 30)
                : Color.FromArgb(35, 35, 35);

            // Подсветка выделенной строки
            if (e.Item.Selected)
            {
                backColor = Color.FromArgb(0, 150, 136); // Неоновый циан для выбранного элемента
            }

            // Заполнение фона
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Цвет текста в зависимости от содержимого
            Color textColor = Color.FromArgb(0, 255, 200); // Неоново-зеленый по умолчанию

            // Специальная раскраска для статуса
            if (e.ColumnIndex == 3) // Колонка статуса
            {
                string? status = e.SubItem.Text;
                if (status != null)
                {
                    if (status.Contains("Ошибка"))
                        textColor = Color.FromArgb(255, 82, 82); // Красный для ошибок
                    else if (status.Contains("завершен"))
                        textColor = Color.FromArgb(0, 255, 200); // Зеленый для успеха
                    else if (status.Contains("Анализ"))
                        textColor = Color.FromArgb(0, 229, 255); // Голубой для процесса
                    else
                        textColor = Color.FromArgb(255, 171, 0); // Оранжевый для ожидания
                }
            }
            else if (e.ColumnIndex == 1) // BPM колонка
            {
                textColor = Color.FromArgb(255, 64, 255); // Неоново-розовый для BPM
            }
            else if (e.ColumnIndex == 2) // Key колонка
            {
                textColor = Color.FromArgb(0, 229, 255); // Неоново-голубой для тональности
            }

            // Отрисовка текста
            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem.Text,
                listViewFiles.Font,
                e.Bounds,
                textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            // Тонкая линия разделителя
            using (var pen = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        #endregion
    }

    // Классы для хранения данных
    public class AudioFileInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public float BPM { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }

        public AudioFileInfo()
        {
            FilePath = string.Empty;
            FileName = string.Empty;
            BPM = 0;
            Key = "Неизвестно";
            Status = string.Empty;
        }
    }

    public class ProgressInfo
    {
        public int Index { get; set; }
        public AudioFileInfo FileInfo { get; set; }
        public string Status { get; set; }

        public ProgressInfo()
        {
            Index = 0;
            FileInfo = new AudioFileInfo();
            Status = string.Empty;
        }
    }
}