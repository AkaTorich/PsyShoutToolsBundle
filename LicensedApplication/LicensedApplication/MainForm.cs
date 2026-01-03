using System;
using System.Windows.Forms;

namespace LicensedApplication
{
    public partial class MainForm : Form
    {
        // Таймер для периодической проверки лицензии
        private Timer licenseCheckTimer;

        public MainForm()
        {
            InitializeComponent();

            // Инициализация таймера для проверки лицензии
            licenseCheckTimer = new Timer();
            licenseCheckTimer.Interval = 60000; // Проверка каждую минуту
            licenseCheckTimer.Tick += LicenseCheckTimer_Tick;
            licenseCheckTimer.Start();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Проверяем лицензию при загрузке и показываем форму активации если нужно
            if (!LicenseManager.IsLicenseValid())
            {
                using (var activationForm = new LicenseActivationForm())
                {
                    if (activationForm.ShowDialog() != DialogResult.OK)
                    {
                        // Если пользователь отменил активацию или не смог активировать,
                        // закрываем приложение
                        this.BeginInvoke(new Action(() => { this.Close(); }));
                        return;
                    }
                }
            }

            // Отображаем информацию о лицензии
            UpdateLicenseInfo();

            // Применяем ограничения лицензии к интерфейсу
            ApplyLicenseRestrictions();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Останавливаем таймер проверки лицензии
            licenseCheckTimer.Stop();
        }

        /// <summary>
        /// Обновляет отображение информации о лицензии
        /// </summary>
        private void UpdateLicenseInfo()
        {
            try
            {
                // Получаем статус лицензии
                var licenseStatus = LicenseManager.GetLicenseStatus();
                var licenseType = LicenseManager.GetLicenseType();
                int daysLeft = LicenseManager.GetRemainingDays();

                // Отображаем информацию о лицензии
                lblLicenseType.Text = "Тип лицензии: " + LicenseManager.GetLicenseTypeName(licenseType);
                lblDaysLeft.Text = $"Осталось дней: {daysLeft}";

                // Изменяем цвет индикатора в зависимости от оставшихся дней
                if (daysLeft <= 5)
                {
                    lblDaysLeft.ForeColor = System.Drawing.Color.Red;
                }
                else if (daysLeft <= 15)
                {
                    lblDaysLeft.ForeColor = System.Drawing.Color.Orange;
                }
                else
                {
                    lblDaysLeft.ForeColor = System.Drawing.Color.Green;
                }

                // Отображаем подробную информацию о лицензии
                rtbLicenseInfo.Text = LicenseManager.GetLicenseInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении информации о лицензии: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Применяет ограничения лицензии к интерфейсу приложения
        /// </summary>
        private void ApplyLicenseRestrictions()
        {
            try
            {
                // Проверяем доступность различных функций в зависимости от лицензии

                // Пример: доступ к функции экспорта
                btnExport.Enabled = LicenseManager.IsFeatureEnabled("export");
                if (!btnExport.Enabled)
                {
                    btnExport.ToolTipText = "Функция доступна только в полной версии";
                }

                // Пример: доступ к пунктам меню
                menuItemAdvancedFeatures.Enabled = LicenseManager.IsFeatureEnabled("advanced");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении ограничений лицензии: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик таймера для периодической проверки лицензии
        /// </summary>
        private void LicenseCheckTimer_Tick(object sender, EventArgs e)
        {
            // Проверяем, не изменился ли статус лицензии
            var currentStatus = LicenseManager.GetLicenseStatus();

            // Если лицензия стала недействительной, показываем предупреждение
            if (currentStatus != LicenseManager.LicenseStatus.Valid)
            {
                licenseCheckTimer.Stop();

                MessageBox.Show("Лицензия стала недействительной или истек её срок действия. " +
                    "Приложение будет закрыто.", "Ошибка лицензии",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Показываем форму активации
                using (var activationForm = new LicenseActivationForm())
                {
                    if (activationForm.ShowDialog() == DialogResult.OK)
                    {
                        // Если активация прошла успешно, обновляем информацию и продолжаем работу
                        UpdateLicenseInfo();
                        ApplyLicenseRestrictions();
                        licenseCheckTimer.Start();
                    }
                    else
                    {
                        // Иначе закрываем приложение
                        Application.Exit();
                    }
                }
            }

            // Также проверяем на наличие отладчика
            if (!AntiDebug.IsSecure())
            {
                licenseCheckTimer.Stop();
                Application.Exit();
            }
        }

        /// <summary>
        /// Пример защищенной функции
        /// </summary>
        private void ExecuteSecureFunction()
        {
            // Перед выполнением защищенного кода проверяем наличие отладчика
            if (!AntiDebug.IsSecure())
            {
                MessageBox.Show("Обнаружена попытка отладки приложения.",
                    "Ошибка безопасности", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем лицензию перед выполнением функции
            if (!LicenseManager.IsLicenseValid())
            {
                var status = LicenseManager.GetLicenseStatus();
                string message = "Для выполнения этой функции требуется действующая лицензия.";

                if (status == LicenseManager.LicenseStatus.Expired ||
                    status == LicenseManager.LicenseStatus.TrialExpired)
                {
                    message = "Срок действия лицензии истек. Пожалуйста, обновите лицензию.";
                }

                MessageBox.Show(message, "Ошибка лицензии", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Если всё в порядке, выполняем защищенный код
            try
            {
                // Здесь ваш защищенный код
                // ...

                MessageBox.Show("Функция успешно выполнена.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении функции: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnLicense_Click(object sender, EventArgs e)
        {
            // Показываем форму активации при нажатии на кнопку "Лицензия"
            using (var activationForm = new LicenseActivationForm())
            {
                activationForm.ShowDialog();

                // После закрытия формы активации обновляем информацию
                UpdateLicenseInfo();
                ApplyLicenseRestrictions();
            }
        }

        private void btnSecureFunction_Click(object sender, EventArgs e)
        {
            // Пример вызова защищенной функции
            ExecuteSecureFunction();
        }
    }
}