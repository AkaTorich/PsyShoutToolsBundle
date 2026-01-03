using System;
using System.Windows.Forms;

namespace LicensedApplication
{
    public partial class LicenseActivationForm : Form
    {
        public LicenseActivationForm()
        {
            InitializeComponent();
        }

        private void LicenseActivationForm_Load(object sender, EventArgs e)
        {
            // Инициализируем лицензию при загрузке формы
            UpdateLicenseStatus();
        }

        private void UpdateLicenseStatus()
        {
            // Получаем и отображаем текущий статус лицензии
            var status = LicenseManager.GetLicenseStatus();
            var licenseType = LicenseManager.GetLicenseType();

            switch (status)
            {
                case LicenseManager.LicenseStatus.Valid:
                    lblStatus.Text = "Лицензия активна";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    txtLicenseInfo.Text = LicenseManager.GetLicenseInfo();

                    btnActivate.Enabled = false;
                    btnDeactivate.Enabled = true;
                    // Удалена строка DialogResult = DialogResult.OK;
                    break;


                case LicenseManager.LicenseStatus.Expired:
                case LicenseManager.LicenseStatus.TrialExpired:
                    lblStatus.Text = "Лицензия истекла";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    txtLicenseInfo.Text = LicenseManager.GetLicenseInfo();

                    btnActivate.Enabled = true;
                    btnDeactivate.Enabled = true;
                    DialogResult = DialogResult.None;
                    break;

                case LicenseManager.LicenseStatus.Invalid:
                case LicenseManager.LicenseStatus.Blacklisted:
                case LicenseManager.LicenseStatus.HardwareChanged:
                    lblStatus.Text = "Лицензия недействительна: " + LicenseManager.GetLastError();
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    txtLicenseInfo.Text = string.Empty;

                    btnActivate.Enabled = true;
                    btnDeactivate.Enabled = true;
                    DialogResult = DialogResult.None;
                    break;

                case LicenseManager.LicenseStatus.NoLicense:
                    lblStatus.Text = "Лицензия не найдена";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    txtLicenseInfo.Text = string.Empty;

                    btnActivate.Enabled = true;
                    btnDeactivate.Enabled = false;
                    DialogResult = DialogResult.None;
                    break;

                default:
                    lblStatus.Text = "Статус лицензии: неизвестен";
                    lblStatus.ForeColor = System.Drawing.Color.Orange;
                    txtLicenseInfo.Text = string.Empty;

                    btnActivate.Enabled = true;
                    btnDeactivate.Enabled = false;
                    DialogResult = DialogResult.None;
                    break;
            }

            // Отображаем аппаратный ID
            txtHardwareId.Text = LicenseManager.GetHardwareId();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLicenseKey.Text) ||
                string.IsNullOrEmpty(txtUserName.Text) ||
                string.IsNullOrEmpty(txtEmail.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для активации лицензии",
                    "Ошибка активации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Активируем лицензию
            bool result = LicenseManager.ActivateLicense(
                txtLicenseKey.Text.Trim(),
                txtUserName.Text.Trim(),
                txtEmail.Text.Trim());

            if (result)
            {
                MessageBox.Show("Лицензия успешно активирована!",
                    "Активация лицензии", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateLicenseStatus();

                // Если активация прошла успешно, закрываем форму с результатом OK
                if (LicenseManager.GetLicenseStatus() == LicenseManager.LicenseStatus.Valid)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Ошибка активации лицензии: " +
                    LicenseManager.GetLastError(),
                    "Ошибка активации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите деактивировать лицензию?",
                "Подтверждение деактивации", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bool result = LicenseManager.DeactivateLicense();
                if (result)
                {
                    MessageBox.Show("Лицензия успешно деактивирована",
                        "Деактивация лицензии", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateLicenseStatus();

                    // Очищаем поля ввода
                    txtLicenseKey.Clear();
                    txtUserName.Clear();
                    txtEmail.Clear();
                }
                else
                {
                    MessageBox.Show("Ошибка деактивации лицензии: " +
                        LicenseManager.GetLastError(),
                        "Ошибка деактивации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            // Копируем аппаратный ID в буфер обмена
            if (!string.IsNullOrEmpty(txtHardwareId.Text))
            {
                Clipboard.SetText(txtHardwareId.Text);
                MessageBox.Show("Аппаратный ID скопирован в буфер обмена",
                    "Копирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Закрываем форму с результатом Cancel, если лицензия не валидна
            if (LicenseManager.GetLicenseStatus() != LicenseManager.LicenseStatus.Valid)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }

            Close();
        }
    }
}