using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Aspose.Pdf;
using Aspose.Pdf.Optimization;

namespace HtmlToPdfConverter
{
    public class PdfConverter
    {
        private IBrowser browser;
        private readonly string logPath;

        public bool EnableCompression { get; set; } = false;
        public double CompressionFactor { get; set; } = 0.25; // 0.25 рекоменд., 0.5 умеренная, 1.0 максимальная
        
        public PdfConverter()
        {
            logPath = Path.Combine(Application.StartupPath, "userapp.txt");
        }

        /// <summary>
        /// Инициализация браузера Chromium
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // Закрываем старый браузер если он есть
                if (browser != null && !browser.IsClosed)
                {
                    try
                    {
                        await browser.CloseAsync();
                        browser.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Предупреждение при закрытии старого браузера: {ex.Message}");
                    }
                }

                LogMessage("Начинаем инициализацию нового браузера...");
                
                // Загружаем браузер Chromium если он не установлен
                using (var browserFetcher = new BrowserFetcher())
                {
                    await browserFetcher.DownloadAsync();
                    LogMessage("Браузер Chromium загружен");
                }

                // Запускаем новый браузер
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { 
                        "--no-sandbox", 
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu",
                        "--disable-web-security",
                        "--disable-extensions",
                        "--no-first-run",
                        "--disable-default-apps",
                        "--disable-background-timer-throttling",
                        "--disable-renderer-backgrounding",
                        "--disable-backgrounding-occluded-windows",
                        "--allow-file-access-from-files",
                        "--enable-local-file-accesses",
                        "--enable-features=MhtmlFullAdoption"
                    },
                    DefaultViewport = new ViewPortOptions
                    {
                        Width = 1200,
                        Height = 800
                    }
                });

                LogMessage("Новый браузер успешно инициализирован");
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при инициализации браузера: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Конвертация множества HTML/MHTML файлов в один PDF
        /// </summary>
        /// <param name="inputFiles">Список путей к входным файлам</param>
        /// <param name="outputPath">Путь к выходному PDF файлу</param>
        /// <param name="progressCallback">Callback для отображения прогресса</param>
        public async Task ConvertToPdfAsync(List<string> inputFiles, string outputPath, Action<int, string> progressCallback = null)
        {
            if (inputFiles == null || inputFiles.Count == 0)
            {
                throw new ArgumentException("Список файлов не может быть пустым");
            }

            try
            {
                LogMessage($"Начинаем конвертацию {inputFiles.Count} файлов в PDF");
                
                var allPages = new List<byte[]>();
                int processedFiles = 0;

                foreach (var inputFile in inputFiles)
                {
                    var ext = Path.GetExtension(inputFile).ToLowerInvariant();

                    // Если это уже готовый PDF — просто добавляем его как есть
                    if (ext == ".pdf")
                    {
                        progressCallback?.Invoke((processedFiles * 100) / inputFiles.Count, $"Добавляем PDF: {Path.GetFileName(inputFile)}");
                        LogMessage($"Добавляем готовый PDF без рендеринга: {inputFile}");
                        try
                        {
                            var bytes = File.ReadAllBytes(inputFile);
                            allPages.Add(bytes);
                            processedFiles++;
                            LogMessage($"Файл {inputFile} успешно добавлен как PDF");
                            if (processedFiles < inputFiles.Count) await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Ошибка при чтении PDF {inputFile}: {ex.Message}");
                        }
                        continue;
                    }

                    // Для HTML/MHTML — рендерим браузером; при необходимости инициализируем его
                    if (browser == null || browser.IsClosed)
                    {
                        await InitializeAsync();
                    }

                    var attempts = 0;
                    var done = false;
                    while (!done && attempts < 3)
                    {
                        attempts++;
                        try
                        {
                            progressCallback?.Invoke((processedFiles * 100) / inputFiles.Count, $"Обрабатываем: {Path.GetFileName(inputFile)}");
                            LogMessage($"Обрабатываем файл: {inputFile} (попытка {attempts})");

                            using (var page = await browser.NewPageAsync())
                            {
                                var fileUri = new Uri(Path.GetFullPath(inputFile)).ToString();
                                await page.GoToAsync(fileUri, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2 } });

                                // Разблокируем контент для печати
                                try { await page.EmulateMediaTypeAsync(PuppeteerSharp.Media.MediaType.Screen); } catch {}
                                // Убираем классы-hidden
                                try { await page.EvaluateFunctionAsync(@"() => {
                                    document.documentElement && (document.documentElement.style.overflow = 'auto');
                                    const rmTokens = ['hidden-print','print-hidden','no-print','k-print-hide'];
                                    document.querySelectorAll('[class]')
                                      .forEach(el => { rmTokens.forEach(t => el.classList.remove(t)); el.className = el.className.replace(/\\bhidden-print\\S*\\b/g,'').trim(); });
                                }"); } catch {}
                                // Инжектим анти-@media print
                                try { await page.AddStyleTagAsync(new AddTagOptions { Content = "@media print { .hidden-print, .print-hidden, .no-print, .k-print-hide { display:block!important; visibility:visible!important; } .reader-content, #reader, .text-container, .text-wrapper, body, html { visibility:visible!important; display:block!important; position:static!important; transform:none!important; } *{-webkit-print-color-adjust:exact!important; print-color-adjust:exact!important; break-inside:auto!important; page-break-inside:auto!important; break-after:auto!important; break-before:auto!important;} }" }); } catch {}

                                await page.WaitForTimeoutAsync(500);
                                try { await page.EvaluateExpressionHandleAsync("document.fonts.ready"); } catch {}
                                try { await page.WaitForSelectorAsync("#text-container, .text-container, body", new WaitForSelectorOptions { Timeout = 5000, Visible = true }); } catch {}
                                try { await page.WaitForFunctionAsync("() => document.readyState === 'complete'", new WaitForFunctionOptions { Timeout = 5000 }); } catch { LogMessage($"Предупреждение: не удалось дождаться полной загрузки {Path.GetFileName(inputFile)}"); }

                                // Диагностика: лог длины текста
                                int textLen = 0; try { textLen = await page.EvaluateFunctionAsync<int>("() => document.body ? document.body.innerText.length : 0"); } catch {}
                                LogMessage($"Длина текста страницы: {textLen}");

                                // Попробуем распечатать чистый контент без оболочки ридера
                                try
                                {
                                    var baseUri = await page.EvaluateFunctionAsync<string>("() => document.baseURI");
                                    var htmlSegment = await page.EvaluateFunctionAsync<string>(@"() => {
                                        const pick=(s)=>document.querySelector(s);
                                        const el = pick('#text-container') || pick('.text-container') || pick('#reader .text-container') || pick('#reader .reader-content') || pick('#app') || document.body;
                                        return el ? el.innerHTML : document.body.innerHTML;
                                    }");
                                    var printable = "<!DOCTYPE html><html><head><meta charset='utf-8'><base href='" + baseUri + "'><style>@page{size:A4;margin:10mm} html,body{height:auto!important;max-height:none!important;overflow:visible!important} body{font-family:Arial,'Segoe UI','DejaVu Sans',sans-serif;font-size:16px;line-height:1.4} img{max-width:100%;page-break-inside:avoid} h1,h2,h3,h4{page-break-after:avoid} pre,code,blockquote,table{page-break-inside:avoid} *{-webkit-print-color-adjust:exact;print-color-adjust:exact}</style></head><body>" + htmlSegment + "</body></html>";
                                    await page.SetContentAsync(printable, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                                    try { await page.WaitForFunctionAsync("() => Array.from(document.images||[]).every(i => i.complete)", new WaitForFunctionOptions { Timeout = 5000 }); } catch {}
                                }
                                catch { }

                                // Нормализуем высоту/переполнение для корректной разбивки на страницы
                                try { await page.AddStyleTagAsync(new AddTagOptions { Content = "@media print { html, body { height:auto!important; max-height:none!important; overflow:visible!important; } img, blockquote, pre, code, table { page-break-inside:avoid!important; } h1,h2,h3,h4 { page-break-after:avoid!important; } }" }); } catch {}

                                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                                {
                                    Width = "210mm",
                                    Height = "297mm",
                                    PrintBackground = true,
                                    Scale = 0.9m,
                                    PreferCSSPageSize = false,
                                    MarginOptions = new MarginOptions { Top = "1cm", Right = "1cm", Bottom = "1cm", Left = "1cm" },
                                    DisplayHeaderFooter = false,
                                    PageRanges = "1-9999"
                                });

                                // Диагностика: сколько страниц создалось у файла
                                try { using (var r = new PdfReader(new MemoryStream(pdfBytes))) { LogMessage($"Страниц в PDF для {Path.GetFileName(inputFile)}: {r.NumberOfPages}"); } } catch {}

                                allPages.Add(pdfBytes);
                            }

                            processedFiles++;
                            LogMessage($"Файл {inputFile} успешно обработан");
                            if (processedFiles < inputFiles.Count) await Task.Delay(300);
                            done = true;
                        }
                        catch (Exception ex)
                        {
                            if (IsClosedException(ex))
                            {
                                LogMessage($"Браузер/страница закрылись неожиданно: {ex.Message}. Переинициализируем и повторим");
                                await InitializeAsync();
                                await Task.Delay(500);
                                continue; // retry
                            }

                            LogMessage($"Ошибка при обработке файла {inputFile}: {ex.Message}");
                            done = true; // не критичная ошибка — переходим к следующему файлу
                        }
                    }
                }

                // Объединяем все PDF страницы в один файл
                progressCallback?.Invoke(95, "Объединяем страницы в один PDF...");
                MergePdfPages(allPages, outputPath);
                
                progressCallback?.Invoke(100, "Конвертация завершена!");
                LogMessage($"Конвертация завершена. Результат сохранен в: {outputPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при конвертации: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Извлечение HTML контента из MHTML файла (БОЛЬШЕ НЕ ИСПОЛЬЗУЕТСЯ)
        /// </summary>
        private string ExtractHtmlFromMhtml(string mhtmlContent)
        {
            return mhtmlContent;
        }

        /// <summary>
        /// Объединение нескольких PDF страниц в один файл
        /// </summary>
        private void MergePdfPages(List<byte[]> pdfPages, string outputPath)
        {
            try
            {
                if (pdfPages == null || pdfPages.Count == 0)
                    throw new ArgumentException("Нет страниц для объединения");

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

                if (pdfPages.Count == 1)
                {
                    var finalBytes = pdfPages[0];
                    if (EnableCompression)
                    {
                        // Aspose-оптимизация изображений/ресурсов
                        finalBytes = OptimizePdfAsposeBytes(finalBytes);
                        // Дополнительно потоковая компрессия
                        finalBytes = CompressPdf(finalBytes);
                    }
                    File.WriteAllBytes(outputPath, finalBytes);
                    if (EnableCompression)
                    {
                        TryAsposeCompress(outputPath);
                        TryGhostscriptCompress(outputPath);
                    }
                    LogMessage($"PDF файл успешно создан: {outputPath} (1 страница)");
                    return;
                }

                using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var document = new iTextSharp.text.Document())
                using (var copy = new PdfSmartCopy(document, outputStream))
                {
                    document.Open();

                    for (int i = 0; i < pdfPages.Count; i++)
                    {
                        try
                        {
                            var bytes = pdfPages[i];
                            if (EnableCompression)
                            {
                                // Оптимизируем входной PDF через Aspose
                                bytes = OptimizePdfAsposeBytes(bytes);
                                // Завершаем потоковой компрессией
                                bytes = CompressPdf(bytes);
                            }
                            using (var reader = new PdfReader(bytes))
                            {
                                copy.AddDocument(reader);
                                copy.FreeReader(reader);
                            }
                            LogMessage($"Добавлена страница {i + 1} из {pdfPages.Count}");
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Ошибка при добавлении страницы {i + 1}: {ex.Message}");
                        }
                    }

                    document.Close();
                }

                if (EnableCompression)
                {
                    TryAsposeCompress(outputPath);
                    TryGhostscriptCompress(outputPath);
                }
                LogMessage($"PDF файл успешно создан: {outputPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при объединении PDF: {ex.Message}");
                throw;
            }
        }

        private byte[] CompressPdf(byte[] input)
        {
            using (var src = new PdfReader(input))
            using (var ms = new MemoryStream())
            {
                using (var document = new iTextSharp.text.Document())
                {
                    var writer = PdfWriter.GetInstance(document, ms);
                    writer.SetFullCompression();
                    // Настраиваем уровень сжатия потоков
                    writer.CompressionLevel = PdfStream.BEST_COMPRESSION;
                    document.Open();

                    var cb = writer.DirectContent;
                    var contentByte = cb;

                    for (int pageNum = 1; pageNum <= src.NumberOfPages; pageNum++)
                    {
                        document.NewPage();
                        var imported = writer.GetImportedPage(src, pageNum);
                        contentByte.AddTemplate(imported, 0, 0);
                    }

                    document.Close();
                }

                // Пробуем уменьшить качество встроенных изображений согласно коэффициенту
                // Примечание: iTextSharp 5.x не предоставляет простого API для рекомпрессии изображений на лету,
                // используем уровень потоков; коэффициент может быть использован в будущем для доп. даунскейла.
                return ms.ToArray();
            }
        }

        private byte[] OptimizePdfImagesBytes(byte[] input)
        {
            try
            {
                using (var reader = new PdfReader(input))
                using (var ms = new MemoryStream())
                {
                    using (var stamper = new PdfStamper(reader, ms, PdfWriter.VERSION_1_5))
                    {
                        stamper.Writer.SetFullCompression();
                        int pageCount = reader.NumberOfPages;
                        int maxDim;
                        long minRecompressSize = 20 * 1024;
                        int jpegQ;
                        if (CompressionFactor >= 0.75) { maxDim = 1280; jpegQ = 35; }
                        else if (CompressionFactor >= 0.375) { maxDim = 1920; jpegQ = 50; }
                        else { maxDim = 2560; jpegQ = 65; }

                        for (int i = 1; i <= pageCount; i++)
                        {
                            var page = reader.GetPageN(i);
                            var resources = page.GetAsDict(PdfName.RESOURCES);
                            if (resources == null) continue;
                            var xobj = resources.GetAsDict(PdfName.XOBJECT);
                            if (xobj == null) continue;

                            foreach (var name in xobj.Keys)
                            {
                                try
                                {
                                    var obj = xobj.GetDirectObject(name);
                                    if (!(obj is PRStream)) continue;
                                    var stream = (PRStream)obj;
                                    var subtype = stream.GetAsName(PdfName.SUBTYPE);
                                    if (!PdfName.IMAGE.Equals(subtype)) continue;
                                    if (stream.GetAsName(PdfName.SMASK) != null || stream.GetAsName(PdfName.MASK) != null) continue;

                                    var imgObj = new iTextSharp.text.pdf.parser.PdfImageObject(stream);
                                    using (var img = imgObj.GetDrawingImage())
                                    {
                                        if (img == null) continue;
                                        int w = img.Width, h = img.Height;
                                        int maxSide = Math.Max(w, h);
                                        var raw = PdfReader.GetStreamBytesRaw(stream);
                                        if (raw != null && raw.LongLength < minRecompressSize) continue;

                                        double scale = 1.0;
                                        if (maxSide > maxDim) scale = (double)maxDim / maxSide;
                                        int nw = Math.Max(1, (int)Math.Round(w * scale));
                                        int nh = Math.Max(1, (int)Math.Round(h * scale));

                                        using (var resized = (scale < 0.999) ? new Bitmap(nw, nh, PixelFormat.Format24bppRgb) : new Bitmap(img))
                                        {
                                            if (scale < 0.999)
                                            {
                                                using (var g = Graphics.FromImage(resized))
                                                {
                                                    g.SmoothingMode = SmoothingMode.HighQuality;
                                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, nw, nh));
                                                }
                                            }

                                            using (var msImg = new MemoryStream())
                                            {
                                                var codec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.MimeType == "image/jpeg");
                                                var ep = new EncoderParameters(1);
                                                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQ);
                                                resized.Save(msImg, codec, ep);
                                                var newData = msImg.ToArray();

                                                if (raw == null || newData.LongLength < raw.LongLength * 0.98)
                                                {
                                                    stream.Put(PdfName.FILTER, PdfName.DCTDECODE);
                                                    stream.Put(PdfName.TYPE, PdfName.XOBJECT);
                                                    stream.Put(PdfName.SUBTYPE, PdfName.IMAGE);
                                                    stream.Put(PdfName.WIDTH, new PdfNumber(resized.Width));
                                                    stream.Put(PdfName.HEIGHT, new PdfNumber(resized.Height));
                                                    stream.Put(PdfName.BITSPERCOMPONENT, new PdfNumber(8));
                                                    stream.Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                                    stream.SetData(newData);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    var result = ms.ToArray();
                    if (result.Length < input.Length) return result;
                    return input;
                }
            }
            catch { return input; }
        }

        private byte[] OptimizePdfAsposeBytes(byte[] input)
        {
            try
            {
                using (var inMs = new MemoryStream(input))
                using (var doc = new Aspose.Pdf.Document(inMs))
                using (var outMs = new MemoryStream())
                {
                    var (jpegQ, dpi) = GetAsposeQuality();
                    var opt = new OptimizationOptions
                    {
                        AllowReusePageContent = true,
                        LinkDuplcateStreams = true,
                        RemoveUnusedStreams = true,
                        RemoveUnusedObjects = true,
                        UnembedFonts = false
                    };
                    opt.ImageCompressionOptions.CompressImages = true;
                    opt.ImageCompressionOptions.ImageQuality = jpegQ;
                    opt.ImageCompressionOptions.ResizeImages = true;
                    opt.ImageCompressionOptions.MaxResolution = dpi;

                    doc.OptimizeResources(opt);
                    doc.Save(outMs);
                    var result = outMs.ToArray();
                    if (result.Length < input.Length * 0.995) return result; // берем только при выигрыше
                    return input;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Aspose оптимизация (bytes) пропущена: {ex.Message}");
                return input;
            }
        }

        private void TryAsposeCompress(string outputPath)
        {
            try
            {
                var tempOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(outputPath), System.IO.Path.GetFileNameWithoutExtension(outputPath) + ".aspose.tmp.pdf");
                if (File.Exists(tempOut)) { try { File.Delete(tempOut); } catch { } }

                using (var doc = new Aspose.Pdf.Document(outputPath))
                {
                    var (jpegQ, dpi) = GetAsposeQuality();
                    var opt = new OptimizationOptions
                    {
                        AllowReusePageContent = true,
                        LinkDuplcateStreams = true,
                        RemoveUnusedStreams = true,
                        RemoveUnusedObjects = true,
                        UnembedFonts = false
                    };
                    opt.ImageCompressionOptions.CompressImages = true;
                    opt.ImageCompressionOptions.ImageQuality = jpegQ;
                    opt.ImageCompressionOptions.ResizeImages = true;
                    opt.ImageCompressionOptions.MaxResolution = dpi;

                    doc.OptimizeResources(opt);
                    doc.Save(tempOut);
                }

                if (File.Exists(tempOut))
                {
                    var orig = new FileInfo(outputPath).Length;
                    var optSize = new FileInfo(tempOut).Length;
                    if (optSize < orig)
                    {
                        File.Copy(tempOut, outputPath, true);
                        LogMessage($"Aspose сжатие применено: {orig} -> {optSize} байт");
                    }
                    else
                    {
                        LogMessage("Aspose не дал выигрыша");
                    }
                    try { File.Delete(tempOut); } catch { }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Aspose оптимизация (file) пропущена: {ex.Message}");
            }
        }

        private (int jpegQ, int dpi) GetAsposeQuality()
        {
            // Маппинг уровней по CompressionFactor
            if (CompressionFactor >= 0.75) return (35, 96);      // Максимальная
            if (CompressionFactor >= 0.375) return (50, 144);    // Умеренная
            return (70, 200);                                    // Рекомендуемая
        }

        private void TryGhostscriptCompress(string outputPath)
        {
            if (!EnableCompression) return;

            string gsExe = FindGhostscript();
            if (string.IsNullOrEmpty(gsExe))
            {
                LogMessage("Ghostscript не найден. Используется встроенная компрессия iTextSharp.");
                return;
            }

            try
            {
                // Настройки по фактору: 1.0 (макс) -> очень агрессивно, 0.5 умеренно, 0.25 рекомендовано
                int dpi;
                int jpegQ;
                if (CompressionFactor >= 0.75)
                {
                    dpi = 72; jpegQ = 35; // максимально сильное сжатие
                }
                else if (CompressionFactor >= 0.375)
                {
                    dpi = 100; jpegQ = 50; // умеренное
                }
                else
                {
                    dpi = 150; jpegQ = 65; // рекомендуемое
                }

                string tempOut = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath) + ".gs.tmp.pdf");
                if (File.Exists(tempOut))
                {
                    try { File.Delete(tempOut); } catch { }
                }

                // Агрессивные параметры перекодирования изображений и потоков
                var args = string.Join(" ", new[] {
                    "-sDEVICE=pdfwrite",
                    "-dCompatibilityLevel=1.5",
                    "-dDetectDuplicateImages=true",
                    "-dCompressFonts=true",
                    "-dSubsetFonts=true",
                    "-dEncodeColorImages=true",
                    "-dEncodeGrayImages=true",
                    "-dEncodeMonoImages=true",
                    "-dAutoFilterColorImages=false",
                    "-dAutoFilterGrayImages=false",
                    "-sColorImageFilter=/DCTEncode",
                    "-sGrayImageFilter=/DCTEncode",
                    $"-dJPEGQ={jpegQ}",
                    "-dDownsampleColorImages=true",
                    "-dDownsampleGrayImages=true",
                    "-dDownsampleMonoImages=true",
                    "-dColorImageDownsampleType=/Bicubic",
                    "-dGrayImageDownsampleType=/Bicubic",
                    "-dMonoImageDownsampleType=/Bicubic",
                    $"-dColorImageResolution={dpi}",
                    $"-dGrayImageResolution={dpi}",
                    $"-dMonoImageResolution={Math.Max(300, dpi)}",
                    "-dUseFlateCompression=true",
                    "-dConvertCMYKImagesToRGB=true",
                    "-dOverprint=0",
                    "-dNOPAUSE -dBATCH -dQUIET",
                    $"-sOutputFile=\"{tempOut}\"",
                    $"\"{outputPath}\""
                });

                var startInfo = new ProcessStartInfo
                {
                    FileName = gsExe,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                LogMessage($"Запуск Ghostscript для сжатия: {gsExe} {args}");
                using (var p = new Process { StartInfo = startInfo })
                {
                    p.Start();
                    var stdOut = p.StandardOutput.ReadToEnd();
                    var stdErr = p.StandardError.ReadToEnd();
                    p.WaitForExit(600000); // до 10 минут на большие файлы
                    LogMessage($"Ghostscript завершен. Код: {p.ExitCode}. Out: {stdOut.Trim()} Err: {stdErr.Trim()}");
                    if (p.ExitCode == 0 && File.Exists(tempOut))
                    {
                        var origSize = new FileInfo(outputPath).Length;
                        var newSize = new FileInfo(tempOut).Length;
                        if (newSize < origSize)
                        {
                            File.Copy(tempOut, outputPath, true);
                            LogMessage($"Ghostscript применен. Размер уменьшен: {origSize} -> {newSize} байт");
                        }
                        else
                        {
                            LogMessage($"Ghostscript не дал выигрыша. Оставляем исходный файл ({origSize} байт)");
                        }
                    }
                    else
                    {
                        LogMessage("Ghostscript не смог сжать PDF. Используем исходный результат.");
                    }
                }

                try { if (File.Exists(tempOut)) File.Delete(tempOut); } catch { }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка Ghostscript-компрессии: {ex.Message}");
            }
        }

        private string FindGhostscript()
        {
            // 1) Попытка найти в типовых путях Program Files
            var candidates = new List<string>();
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            foreach (var baseDir in new[] { Path.Combine(pf, "gs"), Path.Combine(pfx86, "gs") })
            {
                try
                {
                    if (!Directory.Exists(baseDir)) continue;
                    foreach (var dir in Directory.GetDirectories(baseDir, "gs*") )
                    {
                        var bin = Path.Combine(dir, "bin");
                        var x64 = Path.Combine(bin, "gswin64c.exe");
                        var x86 = Path.Combine(bin, "gswin32c.exe");
                        if (File.Exists(x64)) candidates.Add(x64);
                        if (File.Exists(x86)) candidates.Add(x86);
                    }
                }
                catch { }
            }

            // 2) Если не нашли — вернем пусто
            return candidates.FirstOrDefault();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    browser.Dispose();
                    LogMessage("Браузер закрыт");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при закрытии браузера: {ex.Message}");
            }
        }

        /// <summary>
        /// Запись сообщения в лог
        /// </summary>
        private void LogMessage(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n";
                File.AppendAllText(logPath, logEntry, Encoding.UTF8);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        private static bool IsClosedException(Exception ex)
        {
            var msg = ex?.Message ?? string.Empty;
            return msg.IndexOf("Already closed", StringComparison.OrdinalIgnoreCase) >= 0
                || msg.IndexOf("Target closed", StringComparison.OrdinalIgnoreCase) >= 0
                || msg.IndexOf("Connection closed", StringComparison.OrdinalIgnoreCase) >= 0
                || (ex is PuppeteerException && msg.IndexOf("closed", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}