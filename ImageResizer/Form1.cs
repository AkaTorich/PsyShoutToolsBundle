using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ImageResizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Выбрать исходную папку"
        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Выберите исходную папку с изображениями";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtSource.Text = fbd.SelectedPath;
                }
            }
        }

        // Обработчик кнопки "Выбрать целевую папку"
        private void btnSelectTarget_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Выберите целевую папку для сохранения изображений";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtTarget.Text = fbd.SelectedPath;
                }
            }
        }

        // Обработчик кнопки "Начать обработку"
        private void btnStartProcessing_Click(object sender, EventArgs e)
        {
            string sourceDirectory = txtSource.Text;
            string targetDirectory = txtTarget.Text;

            if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите корректную исходную папку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите целевую папку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtWidth.Text, out int maxWidth) || maxWidth <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную ширину изображения (положительное целое число).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtHeight.Text, out int maxHeight) || maxHeight <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную высоту изображения (положительное целое число).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnStartProcessing.Enabled = false;

            System.Threading.Tasks.Task.Run(() => ProcessImages(sourceDirectory, targetDirectory, maxWidth, maxHeight))
                .ContinueWith(t =>
                {
                    this.Invoke(new Action(() =>
                    {
                        btnStartProcessing.Enabled = true;
                        if (t.Exception != null)
                        {
                            MessageBox.Show($"Произошла ошибка: {t.Exception.InnerException.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Обработка завершена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }));
                });
        }

        private void ProcessImages(string sourceDirectory, string targetDirectory, int maxWidth, int maxHeight)
        {
            // Получаем текст водяного знака из TextBox
            string watermarkText = txtWatermark.InvokeRequired
                ? (string)txtWatermark.Invoke(new Func<string>(() => txtWatermark.Text))
                : txtWatermark.Text;

            if (string.IsNullOrEmpty(watermarkText))
            {
                watermarkText = "PsyShout @ Copyrights";
            }

            AppendLog("Начинаем обработку изображений...");

            string[] supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

            var directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
            var allDirectories = new List<string>(directories);
            allDirectories.Insert(0, sourceDirectory);

            int totalImages = 0;
            int processedImages = 0;
            int skippedImages = 0;

            foreach (var dir in allDirectories)
            {
                string relativePath = GetRelativePath(sourceDirectory, dir);
                string targetSubDir = Path.Combine(targetDirectory, relativePath);

                if (!Directory.Exists(targetSubDir))
                {
                    Directory.CreateDirectory(targetSubDir);
                    AppendLog($"Создан подкаталог: {targetSubDir}");
                }

                var files = Directory.GetFiles(dir);
                foreach (var file in files)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (Array.IndexOf(supportedExtensions, extension) < 0)
                    {
                        skippedImages++;
                        AppendLog($"Пропущено (неподдерживаемый формат): {file}");
                        continue;
                    }

                    totalImages++;
                    string fileName = Path.GetFileName(file);
                    string targetFilePath = Path.Combine(targetSubDir, fileName);

                    try
                    {
                        using (Image image = Image.FromFile(file))
                        {
                            using (Image resizedImage = ResizeImage(image, maxWidth, maxHeight))
                            {
                                using (Image watermarkedImage = AddWatermark(resizedImage, watermarkText))
                                {
                                    watermarkedImage.Save(targetFilePath, GetImageFormat(extension));
                                    processedImages++;
                                    AppendLog($"Обработано: {Path.Combine(relativePath, fileName)}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        skippedImages++;
                        AppendLog($"Ошибка при обработке файла {file}: {ex.Message}");
                    }
                }
            }

            AppendLog("=== Завершено ===");
            AppendLog($"Всего изображений: {totalImages}");
            AppendLog($"Обработано: {processedImages}");
            AppendLog($"Пропущено: {skippedImages}");
        }

        /// <summary>
        /// Изменяет размер изображения до указанных размеров, сохраняя пропорции.
        /// </summary>
        private static Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        /// <summary>
        /// Добавляет текстовый водяной знак и копирайтную сетку из наклонных линий.
        /// В сетке две группы: группа 1 – линии с уравнением y = x + b, b от -Width до Height;
        /// группа 2 – линии с уравнением y = -x + c, c от 0 до (Width+Height).
        /// В каждой группе ровно 5 линий (итого 10), линии рисуются с толщиной 3 пикселя
        /// и прозрачностью, соответствующей значению 50 по шкале от 0 до 100.
        /// </summary>
        private static Image AddWatermark(Image image, string watermarkText)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Рисуем исходное изображение
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);

                // Добавляем текстовый водяной знак (с тенью)
                float fontSize = image.Width / 15f;
                Font font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                SizeF textSize = graphics.MeasureString(watermarkText, font);
                float xPosition = (image.Width - textSize.Width) / 2;
                float yPosition = (image.Height - textSize.Height) / 2;
                SolidBrush textBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
                graphics.DrawString(watermarkText, font, Brushes.Black, xPosition + 2, yPosition + 2);
                graphics.DrawString(watermarkText, font, textBrush, xPosition, yPosition);

                // Параметры защитной сетки
                int numLinesPerGroup = 5; // 5 линий для каждой группы (итого 10 линий)
                int desiredTransparency = 50; // по шкале от 0 (непрозрачные) до 100 (максимально прозрачные)
                int alpha = (int)((100 - desiredTransparency) / 100.0 * 255); // для 50 получаем примерно 127

                using (Pen gridPen = new Pen(Color.FromArgb(alpha, 255, 255, 255), 3))
                {
                    // Группа 1: линии с наклоном +45° (уравнение: y = x + b)
                    int bMin = -image.Width;
                    int bMax = image.Height;
                    double bStep = (bMax - bMin) / (double)(numLinesPerGroup - 1);
                    for (int i = 0; i < numLinesPerGroup; i++)
                    {
                        double bParam = bMin + i * bStep;
                        List<PointF> pts = GetIntersectionsGroup1(bParam, image.Width, image.Height);
                        var pair = GetMaxDistancePair(pts);
                        if (pair != null)
                        {
                            graphics.DrawLine(gridPen, pair.Value.Item1, pair.Value.Item2);
                        }
                    }

                    // Группа 2: линии с наклоном -45° (уравнение: y = -x + c)
                    int cMin = 0;
                    int cMax = image.Width + image.Height;
                    double cStep = (cMax - cMin) / (double)(numLinesPerGroup - 1);
                    for (int i = 0; i < numLinesPerGroup; i++)
                    {
                        double cParam = cMin + i * cStep;
                        List<PointF> pts = GetIntersectionsGroup2(cParam, image.Width, image.Height);
                        var pair = GetMaxDistancePair(pts);
                        if (pair != null)
                        {
                            graphics.DrawLine(gridPen, pair.Value.Item1, pair.Value.Item2);
                        }
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// Для линии y = x + b возвращает точки пересечения с прямоугольником (0,0)-(width,height).
        /// </summary>
        private static List<PointF> GetIntersectionsGroup1(double b, int width, int height)
        {
            List<PointF> pts = new List<PointF>();

            // Пересечение с левой гранью: x = 0 => (0, b)
            if (b >= 0 && b <= height)
                pts.Add(new PointF(0, (float)b));

            // Пересечение с правой гранью: x = width => (width, width + b)
            if (width + b >= 0 && width + b <= height)
                pts.Add(new PointF(width, (float)(width + b)));

            // Пересечение с верхней гранью: y = 0 => ( -b, 0 )
            if (-b >= 0 && -b <= width)
                pts.Add(new PointF((float)(-b), 0));

            // Пересечение с нижней гранью: y = height => ( height - b, height )
            if (height - b >= 0 && height - b <= width)
                pts.Add(new PointF((float)(height - b), height));

            return pts;
        }

        /// <summary>
        /// Для линии y = -x + c возвращает точки пересечения с прямоугольником (0,0)-(width,height).
        /// </summary>
        private static List<PointF> GetIntersectionsGroup2(double c, int width, int height)
        {
            List<PointF> pts = new List<PointF>();

            // Пересечение с левой гранью: x = 0 => (0, c)
            if (c >= 0 && c <= height)
                pts.Add(new PointF(0, (float)c));

            // Пересечение с правой гранью: x = width => (width, -width + c)
            if (-width + c >= 0 && -width + c <= height)
                pts.Add(new PointF(width, (float)(-width + c)));

            // Пересечение с верхней гранью: y = 0 => (c, 0)
            if (c >= 0 && c <= width)
                pts.Add(new PointF((float)c, 0));

            // Пересечение с нижней гранью: y = height => (c - height, height)
            if (c - height >= 0 && c - height <= width)
                pts.Add(new PointF((float)(c - height), height));

            return pts;
        }

        /// <summary>
        /// Из множества точек выбирает пару с максимальным расстоянием между ними.
        /// Если пересечений меньше двух – возвращает null.
        /// </summary>
        private static (PointF, PointF)? GetMaxDistancePair(List<PointF> pts)
        {
            if (pts.Count < 2)
                return null;
            double maxDist = -1;
            PointF bestA = pts[0], bestB = pts[1];
            for (int i = 0; i < pts.Count; i++)
            {
                for (int j = i + 1; j < pts.Count; j++)
                {
                    double dist = Distance(pts[i], pts[j]);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        bestA = pts[i];
                        bestB = pts[j];
                    }
                }
            }
            return (bestA, bestB);
        }

        private static double Distance(PointF a, PointF b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Возвращает формат изображения по расширению файла.
        /// </summary>
        private static ImageFormat GetImageFormat(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    return ImageFormat.Png;
            }
        }

        /// <summary>
        /// Получает относительный путь между двумя абсолютными путями.
        /// </summary>
        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }
            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Добавляет символ разделителя каталога к пути, если его нет.
        /// </summary>
        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        /// <summary>
        /// Добавляет сообщение в лог (безопасно для вызова из любых потоков).
        /// </summary>
        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AppendLog), message);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
            }
        }
    }
}
