using System;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Drawing;

namespace ThunderPriceCalculator
{
    public partial class MainForm : Form
    {
        private Timer calculationTimer;
        private bool isCalculating = false;
        private string logPath = "calculator.log";
        private readonly IFormatProvider numberFormat = CultureInfo.InvariantCulture;
        private string lastErrorMessage = string.Empty;

        // Переменные для отслеживания изменений входных параметров
        private string lastDeposit = string.Empty;
        private string lastRisk = string.Empty;
        private string lastCurrentPrice = string.Empty;
        private string lastTickPrice = string.Empty;
        private string lastRiskType = string.Empty;
        private string lastMarginType = string.Empty;
        private string lastOrderType = string.Empty;
        private string lastDesiredProfit = string.Empty;
        private string lastProfitType = string.Empty;
        private string lastDesiredLeverage = string.Empty;
        private string lastTraderType = string.Empty; // Добавлено для Taker/Maker

        private void LogToFile(string message)
        {
            try
            {
                //File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}\n");
            }
            catch
            {
                // Игнорируем ошибки записи лога
            }
        }

        // 2. ОБНОВЛЕННЫЙ МЕТОД GetDecimalPlaces
        private int GetDecimalPlaces(string priceText)
        {
            if (string.IsNullOrEmpty(priceText))
                return 2; // По умолчанию 2 знака

            // Используем нормализацию
            string normalizedPrice = NormalizeDecimalInput(priceText);

            // Находим позицию точки
            int dotIndex = normalizedPrice.IndexOf('.');

            if (dotIndex == -1)
                return 0; // Целое число

            // Считаем количество символов после точки
            return normalizedPrice.Length - dotIndex - 1;
        }

        // НОВЫЙ МЕТОД: Форматирование цены с нужным количеством знаков
        private string FormatPriceWithPrecision(decimal price, int decimalPlaces)
        {
            string format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";
            return price.ToString(format, numberFormat);
        }

        private bool HasInputParametersChanged()
        {
            string currentDeposit = txtDeposit.Text;
            string currentRisk = txtRisk.Text;
            string currentPrice = txtCurrentPrice.Text;
            string currentTickPrice = txtTickPrice.Text;
            string currentRiskType = cmbRiskType.SelectedItem?.ToString() ?? "";
            string currentMarginType = cmbMarginType.SelectedItem?.ToString() ?? "";
            string currentOrderType = cmbOrderType.SelectedItem?.ToString() ?? "";
            string currentDesiredProfit = txtDesiredProfit.Text;
            string currentProfitType = cmbProfitType.SelectedItem?.ToString() ?? "";
            string currentDesiredLeverage = txtDesiredLeverage.Text;
            string currentTraderType = cmbTraderType.SelectedItem?.ToString() ?? ""; // Добавлено

            bool hasChanged = lastDeposit != currentDeposit ||
                            lastRisk != currentRisk ||
                            lastCurrentPrice != currentPrice ||
                            lastTickPrice != currentTickPrice ||
                            lastRiskType != currentRiskType ||
                            lastMarginType != currentMarginType ||
                            lastOrderType != currentOrderType ||
                            lastDesiredProfit != currentDesiredProfit ||
                            lastProfitType != currentProfitType ||
                            lastDesiredLeverage != currentDesiredLeverage ||
                            lastTraderType != currentTraderType; // Добавлено

            if (hasChanged)
            {
                // Обновляем сохраненные значения
                lastDeposit = currentDeposit;
                lastRisk = currentRisk;
                lastCurrentPrice = currentPrice;
                lastTickPrice = currentTickPrice;
                lastRiskType = currentRiskType;
                lastMarginType = currentMarginType;
                lastOrderType = currentOrderType;
                lastDesiredProfit = currentDesiredProfit;
                lastProfitType = currentProfitType;
                lastDesiredLeverage = currentDesiredLeverage;
                lastTraderType = currentTraderType; // Добавлено
            }

            return hasChanged;
        }

        public MainForm()
        {
            InitializeComponent();
            LogToFile("Форма инициализирована");

            // Настройка формы
            this.Text = "Thunder Price Calculator";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Установка начальных значений ComboBox
            cmbRiskType.Items.Clear();
            cmbRiskType.Items.Add("USD");
            cmbRiskType.Items.Add("Percent");
            cmbRiskType.SelectedIndex = 1;
            LogToFile($"ComboBox риска инициализирован, выбрано: {cmbRiskType.SelectedItem}");

            cmbProfitType.Items.Clear();
            cmbProfitType.Items.Add("USD");
            cmbProfitType.Items.Add("Percent");
            cmbProfitType.SelectedIndex = 1;

            // Установка типов маржи
            cmbMarginType.Items.Clear();
            cmbMarginType.Items.Add("Кросс-маржа");
            cmbMarginType.Items.Add("Изолированная");
            cmbMarginType.SelectedIndex = 1; // По умолчанию "Изолированная"
            LogToFile($"ComboBox типа маржи инициализирован, выбрано: {cmbMarginType.SelectedItem}");

            // НОВОЕ: Инициализация Taker/Maker
            cmbTraderType.Items.Clear();
            cmbTraderType.Items.Add("Taker");
            cmbTraderType.Items.Add("Maker");
            cmbTraderType.SelectedIndex = 0; // По умолчанию "Taker"
            LogToFile($"ComboBox типа трейдера инициализирован, выбрано: {cmbTraderType.SelectedItem}");

            // Добавляем подсказки
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtTickPrice, "Введите цену тика для стандартного лота (1.0)");
            toolTip.SetToolTip(lblTickPrice, "Цена тика рассчитывается для стандартного лота размером 1.0");
            toolTip.SetToolTip(txtCurrentPrice, "Введите текущую цену инструмента");
            toolTip.SetToolTip(txtDealAmount, "Сумма сделки = Текущая цена × Объем (полная стоимость позиции)");
            toolTip.SetToolTip(txtDesiredLeverage, "Введите желаемое плечо (от 1 до 125)");
            toolTip.SetToolTip(lblDesiredLeverage, "Плечо определяет необходимую маржу для сделки");
            toolTip.SetToolTip(txtMargin, "Маржа = Сумма сделки / Плечо (необходимые собственные средства)");

            // Tooltip и подписка для типа ордера
            toolTip.SetToolTip(cmbOrderType, "Выберите Buy (длинная позиция) или Sell (короткая позиция)");
            cmbOrderType.Text = "Buy";
            cmbOrderType.SelectedIndexChanged += InputField_TextChanged;
            LogToFile($"ComboBox типа ордера инициализирован, выбрано: {cmbOrderType.SelectedItem}");

            // НОВОЕ: Tooltip для Taker/Maker
            toolTip.SetToolTip(cmbTraderType, "Taker - забирает ликвидность (выше комиссия), Maker - создает ликвидность (ниже комиссия)");

            // Инициализация новых элементов для желаемой прибыли
            toolTip.SetToolTip(txtDesiredProfit, "Введите желаемую прибыль в процентах или долларах");
            toolTip.SetToolTip(cmbProfitType, "Выберите тип прибыли: проценты или доллары");
            LogToFile($"ComboBox типа прибыли инициализирован, выбрано: {cmbProfitType.SelectedItem}");

            // Подписываемся на события изменения текста
            txtDeposit.TextChanged += InputField_TextChanged;
            txtRisk.TextChanged += InputField_TextChanged;
            txtCurrentPrice.TextChanged += InputField_TextChanged;
            txtTickPrice.TextChanged += InputField_TextChanged;
            txtDesiredProfit.TextChanged += InputField_TextChanged;
            txtDesiredLeverage.TextChanged += InputField_TextChanged;
            cmbRiskType.SelectedIndexChanged += InputField_TextChanged;
            cmbRiskType.SelectionChangeCommitted += InputField_TextChanged;
            cmbProfitType.SelectedIndexChanged += InputField_TextChanged;
            cmbProfitType.SelectionChangeCommitted += InputField_TextChanged;
            cmbMarginType.SelectedIndexChanged += InputField_TextChanged;
            cmbTraderType.SelectedIndexChanged += InputField_TextChanged; // НОВОЕ
            cmbTraderType.SelectionChangeCommitted += InputField_TextChanged; // НОВОЕ

            // Устанавливаем фильтр на ввод только чисел и точки
            txtDeposit.KeyPress += NumericOnly_KeyPress;
            txtRisk.KeyPress += NumericOnly_KeyPress;
            txtCurrentPrice.KeyPress += NumericOnly_KeyPress;
            txtTickPrice.KeyPress += NumericOnly_KeyPress;
            txtDesiredProfit.KeyPress += NumericOnly_KeyPress;
            txtDesiredLeverage.KeyPress += NumericOnly_KeyPress;

            // Настройка валидации вставки для всех числовых полей
            SetupTextBoxPasteValidation(txtDeposit);
            SetupTextBoxPasteValidation(txtRisk);
            SetupTextBoxPasteValidation(txtCurrentPrice);
            SetupTextBoxPasteValidation(txtTickPrice);
            SetupTextBoxPasteValidation(txtDesiredProfit);
            SetupTextBoxPasteValidation(txtDesiredLeverage);

            // Инициализация и настройка таймера
            calculationTimer = new Timer();
            calculationTimer.Interval = 100; // интервал 100 мс
            calculationTimer.Tick += CalculationTimer_Tick;
            calculationTimer.Start();
            LogToFile("Таймер запущен");

            // Запускаем первоначальный расчет
            Calculate();
        }

        private void InputField_TextChanged(object sender, EventArgs e)
        {
            if (!isCalculating)
            {
                isCalculating = true;
                Calculate();
                isCalculating = false;
            }
        }

        private void CalculationTimer_Tick(object sender, EventArgs e)
        {
            if (!isCalculating)
            {
                isCalculating = true;
                Calculate();
                isCalculating = false;
            }
        }

        // НОВЫЙ МЕТОД: Получение комиссии в зависимости от типа трейдера
        private decimal GetTradingFee(string traderType, string instrumentType = "futures")
        {
            // Комиссии Bybit для разных типов инструментов
            switch (instrumentType.ToLower())
            {
                case "spot":
                    return traderType == "Taker" ? 0.001M : 0.001M; // Спот: 0.1% для обоих
                case "futures":
                case "perpetual":
                    return traderType == "Taker" ? 0.001M : 0.00036M; // Фьючерсы: Taker 0.1%, Maker 0.036%
                case "options":
                    return 0.0003M; // Опционы: 0.03% для обоих
                default:
                    return traderType == "Taker" ? 0.001M : 0.00036M; // По умолчанию фьючерсы
            }
        }

        // 3. ПОЛНЫЙ ОБНОВЛЕННЫЙ МЕТОД Calculate
        private void Calculate()
        {
            try
            {
                // Проверяем, изменились ли входные параметры
                bool parametersChanged = HasInputParametersChanged();

                if (parametersChanged)
                {
                    LogToFile("\n=== ТОЧНЫЙ РАСЧЕТ НА ОСНОВЕ РИСКА ===");
                    LogToFile($"Депозит: {txtDeposit.Text}");
                    LogToFile($"Риск: {txtRisk.Text}");
                    LogToFile($"Текущая цена: {txtCurrentPrice.Text}");
                    LogToFile($"Цена тика: {txtTickPrice.Text}");
                    LogToFile($"Желаемая прибыль: {txtDesiredProfit.Text} ({cmbProfitType.SelectedItem})");
                    LogToFile($"Желаемое плечо: {txtDesiredLeverage.Text}");
                    LogToFile($"Тип риска: {cmbRiskType.SelectedItem}");
                    LogToFile($"Тип трейдера: {cmbTraderType.SelectedItem}");
                }

                string orderTypeStr = cmbOrderType.SelectedItem as string ?? "Buy";
                bool isBuy = orderTypeStr == "Buy";

                // Получаем тип трейдера
                string traderType = cmbTraderType.SelectedItem as string ?? "Taker";

                if (string.IsNullOrWhiteSpace(txtDeposit.Text) ||
                    string.IsNullOrWhiteSpace(txtRisk.Text) ||
                    string.IsNullOrWhiteSpace(txtCurrentPrice.Text) ||
                    string.IsNullOrWhiteSpace(txtTickPrice.Text) ||
                    string.IsNullOrWhiteSpace(txtDesiredProfit.Text) ||
                    string.IsNullOrWhiteSpace(txtDesiredLeverage.Text))
                {
                    if (parametersChanged) LogToFile("Одно из полей пустое");
                    return;
                }

                bool success = true;
                decimal deposit = 0, risk = 0, currentPrice = 0, tickPrice = 0, desiredProfit = 0, desiredLeverage = 0;

                // ИСПРАВЛЕНО: Используем нормализацию для всех полей
                success &= decimal.TryParse(NormalizeDecimalInput(txtDeposit.Text), NumberStyles.Any, numberFormat, out deposit);
                success &= decimal.TryParse(NormalizeDecimalInput(txtRisk.Text), NumberStyles.Any, numberFormat, out risk);
                success &= decimal.TryParse(NormalizeDecimalInput(txtCurrentPrice.Text), NumberStyles.Any, numberFormat, out currentPrice);
                success &= decimal.TryParse(NormalizeDecimalInput(txtTickPrice.Text), NumberStyles.Any, numberFormat, out tickPrice);
                success &= decimal.TryParse(NormalizeDecimalInput(txtDesiredProfit.Text), NumberStyles.Any, numberFormat, out desiredProfit);
                success &= decimal.TryParse(NormalizeDecimalInput(txtDesiredLeverage.Text), NumberStyles.Any, numberFormat, out desiredLeverage);

                if (!success)
                {
                    if (parametersChanged) LogToFile("Ошибка преобразования значений");
                    return;
                }

                // Определяем точность цены на основе введенной цены котировки
                int priceDecimalPlaces = GetDecimalPlaces(txtCurrentPrice.Text);
                if (parametersChanged) LogToFile($"Точность цены: {priceDecimalPlaces} знаков после точки");

                // Проверки валидности
                if (deposit <= 0 || risk <= 0 || tickPrice <= 0 || currentPrice <= 0 || desiredProfit <= 0 || desiredLeverage <= 0)
                {
                    if (parametersChanged) LogToFile("Обнаружены нулевые или отрицательные значения");
                    return;
                }

                if (desiredLeverage < 1 || desiredLeverage > 125)
                {
                    string error = "Плечо должно быть в диапазоне от 1 до 125";
                    if (lastErrorMessage != error)
                    {
                        UpdateNotification(error, true);
                        lastErrorMessage = error;
                    }
                    return;
                }

                // === ТОЧНЫЙ РАСЧЕТ ===

                // 1. Размер риска в USD
                decimal riskSizeUSD;
                if (cmbRiskType.SelectedItem.ToString() == "Percent")
                {
                    if (risk > 100)
                    {
                        string error = "Риск не может превышать 100%";
                        if (lastErrorMessage != error)
                        {
                            UpdateNotification(error, true);
                            lastErrorMessage = error;
                        }
                        return;
                    }
                    riskSizeUSD = (risk / 100) * deposit;
                }
                else
                {
                    riskSizeUSD = risk;
                    if (riskSizeUSD > deposit)
                    {
                        string error = "Риск не может превышать размер депозита";
                        if (lastErrorMessage != error)
                        {
                            UpdateNotification(error, true);
                            lastErrorMessage = error;
                        }
                        return;
                    }
                }

                if (parametersChanged) LogToFile($"Размер риска в USD: {riskSizeUSD}");

                // 2. ОПРЕДЕЛЯЕМ РАЗУМНЫЙ МИНИМАЛЬНЫЙ ОБЪЕМ ДЛЯ СТОП-ЛОССА
                decimal minVolume = currentPrice > 10000M ? 0.001M : 0.01M;
                int volumePrecision = minVolume == 0.001M ? 3 : 2;

                // 3. РАСЧИТЫВАЕМ МАКСИМАЛЬНОЕ РАССТОЯНИЕ ДО СТОП-ЛОССА
                // Чтобы риск не превышал заданный при минимальном объеме
                decimal maxStopDistance = riskSizeUSD / minVolume;

                // 4. ОПРЕДЕЛЯЕМ РАЗУМНОЕ ПРОЦЕНТНОЕ РАССТОЯНИЕ ДЛЯ СТОП-ЛОССА
                decimal reasonableStopPercent;
                if (tickPrice >= 0.1M)        // BTC и дорогие активы
                    reasonableStopPercent = 1.0M;   // 1%
                else if (tickPrice >= 0.01M)  // Обычные криптовалюты  
                    reasonableStopPercent = 1.5M;   // 1.5%
                else if (tickPrice >= 0.001M) // ETH и средние активы
                    reasonableStopPercent = 2.0M;   // 2%
                else if (tickPrice >= 0.0001M) // Альткоины (ваш случай)
                    reasonableStopPercent = 0.5M;   // 0.5% - БЛИЖЕ для мелких активов!
                else
                    reasonableStopPercent = 1.0M;   // По умолчанию 1%

                decimal reasonableStopDistance = currentPrice * (reasonableStopPercent / 100);

                // 5. ВЫБИРАЕМ МЕНЬШЕЕ ИЗ ДВУХ РАССТОЯНИЙ
                decimal finalStopDistance = Math.Min(reasonableStopDistance, maxStopDistance);

                // 6. РАССЧИТЫВАЕМ ТОЧНУЮ ЦЕНУ СТОП-ЛОССА
                decimal stopPrice = isBuy ?
                    currentPrice - finalStopDistance :
                    currentPrice + finalStopDistance;

                // 7. РАССЧИТЫВАЕМ ТОЧНЫЙ ОБЪЕМ ПОД ЭТОТ СТОП-ЛОСС
                decimal volume = riskSizeUSD / finalStopDistance;
                volume = Math.Round(volume, volumePrecision);

                bool volumeAdjusted = false;
                if (volume < minVolume)
                {
                    volume = minVolume;
                    volumeAdjusted = true;
                }

                if (parametersChanged)
                {
                    LogToFile($"=== ТОЧНАЯ ФОРМУЛА ===");
                    LogToFile($"Разумный стоп %: {reasonableStopPercent}%");
                    LogToFile($"Разумное расстояние: {reasonableStopDistance}");
                    LogToFile($"Максимальное расстояние: {maxStopDistance}");
                    LogToFile($"Выбранное расстояние: {finalStopDistance}");
                    LogToFile($"Цена стоп-лосса: {stopPrice}");
                    LogToFile($"Объем = {riskSizeUSD} / {finalStopDistance} = {volume}");
                }

                // 8. Рассчитываем тейк-профит
                decimal profitDistance;
                decimal takeProfitPrice;

                string profitTypeStr = cmbProfitType.SelectedItem?.ToString() ?? "Percent";

                if (profitTypeStr == "Percent")
                {
                    profitDistance = currentPrice * (desiredProfit / 100);
                    takeProfitPrice = isBuy ?
                        currentPrice + profitDistance :
                        currentPrice - profitDistance;
                }
                else
                {
                    profitDistance = desiredProfit / volume;
                    takeProfitPrice = isBuy ?
                        currentPrice + profitDistance :
                        currentPrice - profitDistance;
                }

                // 9. Соотношение риск/прибыль
                decimal riskRewardRatio = profitDistance / finalStopDistance;

                // 10. Проверяем логику ордеров
                if (isBuy)
                {
                    if (stopPrice >= currentPrice || takeProfitPrice <= currentPrice)
                    {
                        UpdateNotification("ОШИБКА: Неправильная логика Buy ордера!", true);
                        return;
                    }
                }
                else
                {
                    if (takeProfitPrice >= currentPrice || stopPrice <= currentPrice)
                    {
                        UpdateNotification("ОШИБКА: Неправильная логика Sell ордера!", true);
                        return;
                    }
                }

                // 11. ИСПРАВЛЕННЫЕ ФИНАНСОВЫЕ РАСЧЕТЫ
                decimal totalPositionValue = currentPrice * volume;

                // ИСПРАВЛЕНО: Сумма сделки = полная стоимость позиции
                decimal dealAmount = totalPositionValue;

                // ИСПРАВЛЕНО: Маржа = сумма сделки / плечо
                decimal margin = totalPositionValue / desiredLeverage;

                // Расчет комиссий
                decimal tradingFee = GetTradingFee(traderType, "futures"); // Используем фьючерсы по умолчанию
                decimal entryFee = totalPositionValue * tradingFee; // Комиссия за вход
                decimal exitFee = totalPositionValue * tradingFee;  // Комиссия за выход
                decimal totalFees = entryFee + exitFee;

                decimal actualRisk = finalStopDistance * volume + totalFees; // Добавляем комиссии к риску
                decimal potentialProfit = profitTypeStr == "USD" ? desiredProfit : profitDistance * volume;
                decimal netProfit = potentialProfit - totalFees; // Чистая прибыль после комиссий
                decimal balanceAtProfit = deposit + netProfit;
                decimal balanceAtLoss = deposit - actualRisk;

                // 12. Проценты для отображения
                decimal stopLossPercDisp = (finalStopDistance / currentPrice) * 100;
                decimal takeProfitPercDisp = (profitDistance / currentPrice) * 100;
                decimal actualRiskPerc = (actualRisk / deposit) * 100;

                string stopPrefix = isBuy ? "-" : "+";
                string tpPrefix = isBuy ? "+" : "-";

                if (parametersChanged)
                {
                    LogToFile($"=== ФИНАЛЬНЫЕ РЕЗУЛЬТАТЫ ===");
                    LogToFile($"Объем: {volume} лотов");
                    LogToFile($"Сумма сделки: ${dealAmount:F2} (полная стоимость позиции)");
                    LogToFile($"Маржа: ${margin:F2} (необходимые средства)");
                    LogToFile($"Стоп-лосс: {stopPrice} ({stopPrefix}{stopLossPercDisp:F2}%)");
                    LogToFile($"Тейк-профит: {takeProfitPrice} ({tpPrefix}{takeProfitPercDisp:F2}%)");
                    LogToFile($"Фактический риск: ${actualRisk:F2} (ожидалось ${riskSizeUSD:F2})");
                    LogToFile($"Комиссии ({traderType}): ${totalFees:F2} (вход: ${entryFee:F2}, выход: ${exitFee:F2})");
                    LogToFile($"Чистая прибыль: ${netProfit:F2}");
                    LogToFile($"Соотношение: 1:{riskRewardRatio:F2}");
                }

                // 13. Обновляем поля с правильной точностью цен
                txtTakeProfit.Text = FormatPriceWithPrecision(takeProfitPrice, priceDecimalPlaces);
                txtStopLoss.Text = FormatPriceWithPrecision(stopPrice, priceDecimalPlaces);
                txtVolume.Text = volume.ToString(volumePrecision == 3 ? "F3" : "F2", numberFormat);
                txtDealAmount.Text = dealAmount.ToString("F2", numberFormat);
                txtMargin.Text = margin.ToString("F2", numberFormat);

                // 14. Уведомление с правильной точностью цен
                string notification = $"Цель: {FormatPriceWithPrecision(takeProfitPrice, priceDecimalPlaces)} ({tpPrefix}{takeProfitPercDisp:F1}%) Баланс: {balanceAtProfit.ToString("F2", numberFormat)}\n" +
                                    $"ПР/УБ: {(netProfit - (actualRisk - totalFees)).ToString("F2", numberFormat)}, Объём: {volume.ToString(volumePrecision == 3 ? "F3" : "F2", numberFormat)}\n" +
                                    $"Соотношение риск/прибыль: 1:{riskRewardRatio.ToString("F2", numberFormat)}\n" +
                                    $"Стоп: {FormatPriceWithPrecision(stopPrice, priceDecimalPlaces)} ({stopPrefix}{stopLossPercDisp:F1}%) Баланс: {balanceAtLoss.ToString("F2", numberFormat)}\n" +
                                    $"Комиссии ({traderType}): ${totalFees.ToString("F2", numberFormat)} ({(tradingFee * 100).ToString("F3", numberFormat)}% × 2)\n" +
                                    $"Сумма: ${dealAmount.ToString("F2", numberFormat)}, Маржа: ${margin.ToString("F2", numberFormat)}";

                // 15. Предупреждения
                bool hasWarning = false;

                if (volumeAdjusted)
                {
                    notification += $"\n⚠️ Объём скорректирован до минимума: {minVolume.ToString(volumePrecision == 3 ? "F3" : "F2", numberFormat)}";
                    hasWarning = true;
                }

                if (Math.Abs((actualRisk - totalFees) - riskSizeUSD) > 0.01M)
                {
                    notification += $"\n💡 Точный риск: ${actualRisk.ToString("F2", numberFormat)} (цель: ${riskSizeUSD.ToString("F2", numberFormat)})";
                }

                if (riskRewardRatio < 1.0M)
                {
                    notification += $"\n⚠️ Низкое соотношение: 1:{riskRewardRatio.ToString("F2", numberFormat)}";
                    hasWarning = true;
                }

                if (desiredLeverage > 50)
                {
                    notification += $"\n⚠️ ОПАСНО: Плечо {desiredLeverage}x";
                    hasWarning = true;
                }

                // ИСПРАВЛЕНО: Проверяем маржу, а не сумму сделки
                if (margin > deposit)
                {
                    notification += $"\n⚠️ КРИТИЧНО: Недостаточно средств для маржи!";
                    hasWarning = true;
                }

                // Предупреждение о высоких комиссиях
                decimal feePercent = (totalFees / totalPositionValue) * 100;
                if (feePercent > 0.5M)
                {
                    notification += $"\n⚠️ Высокие комиссии: {feePercent.ToString("F2", numberFormat)}% от позиции";
                    hasWarning = true;
                }

                UpdateNotification(notification, hasWarning);
                lastErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка расчета: {ex.Message}";
                UpdateNotification(errorMessage, true);
                LogToFile($"{errorMessage}\n{ex.StackTrace}");
            }
        }

        private void UpdateResults(decimal stopLossPrice, decimal takeProfitPrice, decimal volume,
            decimal dealAmount, decimal margin, decimal leverage)
        {
            try
            {
                // НОВОЕ: Определяем точность для цен на основе введенной цены
                int priceDecimalPlaces = GetDecimalPlaces(txtCurrentPrice.Text);

                // Определяем точность для объема
                decimal minVolume = takeProfitPrice > 10000M ? 0.001M : 0.01M;
                int volumePrecision = minVolume == 0.001M ? 3 : 2;

                txtTakeProfit.Text = FormatPriceWithPrecision(takeProfitPrice, priceDecimalPlaces);
                txtStopLoss.Text = FormatPriceWithPrecision(stopLossPrice, priceDecimalPlaces);
                txtVolume.Text = volume.ToString(volumePrecision == 3 ? "F3" : "F2", numberFormat);
                txtDealAmount.Text = dealAmount.ToString("F2", numberFormat);
                txtMargin.Text = margin.ToString("F2", numberFormat);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка обновления результатов: {ex.Message}";
                UpdateNotification(errorMessage, true);
                LogToFile($"{errorMessage}\n{ex.StackTrace}");
            }
        }

        private void UpdateNotification(string message, bool isWarning = false)
        {
            txtNotification.Text = message;
            txtNotification.ForeColor = isWarning ? System.Drawing.Color.Red : System.Drawing.Color.White;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (calculationTimer != null)
            {
                calculationTimer.Stop();
                calculationTimer.Dispose();
            }
            LogToFile("Форма закрывается");
            base.OnFormClosing(e);
        }

        private void NumericOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+Z, Delete, Home, End и т.д.)
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешаем ввод цифр
            if (char.IsDigit(e.KeyChar))
                return;

            // Разрешаем точку и запятую как разделители
            if (e.KeyChar == '.' || e.KeyChar == ',')
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    // Проверяем, что разделитель еще не введен (учитываем и точку, и запятую)
                    if (!textBox.Text.Contains(".") && !textBox.Text.Contains(","))
                        return;
                }
            }

            // Запрещаем все остальные символы
            e.Handled = true;
        }
        // ДОПОЛНИТЕЛЬНО: Добавь валидацию вставляемого текста при вставке
        // Этот метод нужно добавить в конструктор MainForm для каждого TextBox
        private void SetupTextBoxPasteValidation(TextBox textBox)
        {
            textBox.KeyDown += (sender, e) =>
            {
                // Обрабатываем Ctrl+V
                if (e.Control && e.KeyCode == Keys.V)
                {
                    // Получаем текст из буфера обмена
                    string clipboardText = Clipboard.GetText();

                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        // Проверяем, что вставляемый текст является корректным числом
                        if (IsValidNumericInput(clipboardText))
                        {
                            // Если число корректное, разрешаем вставку
                            return;
                        }
                        else
                        {
                            // Если число некорректное, блокируем вставку
                            e.Handled = true;
                            e.SuppressKeyPress = true;

                            // Показываем уведомление пользователю
                            UpdateNotification("⚠️ Вставляемый текст не является корректным числом", true);
                        }
                    }
                }
            };
        }

        private bool IsValidNumericInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Нормализуем ввод (заменяем запятую на точку)
            string normalizedInput = NormalizeDecimalInput(input.Trim());

            // Проверяем, что это корректное число
            return decimal.TryParse(normalizedInput, NumberStyles.Any, numberFormat, out _);
        }

        // Добавь этот метод в класс MainForm
        private void ResultField_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    // Копируем текст в буфер обмена
                    Clipboard.SetText(textBox.Text);

                    // Опционально: показываем уведомление пользователю
                    string fieldName = GetFieldDisplayName(textBox);
                    UpdateNotification($"✅ Скопировано: {fieldName} = {textBox.Text}", false);

                    // Выделяем весь текст в поле
                    textBox.SelectAll();

                    // Логируем действие
                    LogToFile($"Скопировано значение {fieldName}: {textBox.Text}");
                }
            }
            catch (Exception ex)
            {
                UpdateNotification($"Ошибка копирования: {ex.Message}", true);
                LogToFile($"Ошибка копирования: {ex.Message}");
            }
        }

        // 1. НОВЫЙ ВСПОМОГАТЕЛЬНЫЙ МЕТОД - добавь в класс MainForm
        private string NormalizeDecimalInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Заменяем запятую на точку для корректного парсинга
            return input.Replace(',', '.');
        }

        // Вспомогательный метод для получения понятного имени поля
        private string GetFieldDisplayName(TextBox textBox)
        {
            if (textBox == txtTakeProfit) return "Тейк-профит";
            if (textBox == txtStopLoss) return "Стоп-лосс";
            if (textBox == txtVolume) return "Объем";
            if (textBox == txtDealAmount) return "Сумма сделки";
            if (textBox == txtMargin) return "Маржа";
            if (textBox == txtDeposit) return "Депозит";
            if (textBox == txtRisk) return "Риск";
            if (textBox == txtCurrentPrice) return "Текущая цена";
            if (textBox == txtTickPrice) return "Цена тика";
            if (textBox == txtDesiredProfit) return "Желаемая прибыль";
            if (textBox == txtDesiredLeverage) return "Плечо";

            return "Значение";
        }
    }
}