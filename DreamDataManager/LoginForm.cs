// LoginForm.cs
using System;
using System.Windows.Forms;

namespace DreamDiary
{
    public partial class LoginForm : Form
    {
        public byte[] EncryptionKey { get; private set; }
        public byte[] EncryptionIV { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string enteredPassword = textBoxPassword.Text;

            if (PasswordManager.ValidatePassword(enteredPassword))
            {
                // Генерируем ключ и IV на основе пароля
                EncryptionHelper.GenerateKeyAndIV(enteredPassword, out byte[] key, out byte[] iv);
                EncryptionKey = key;
                EncryptionIV = iv;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль. Попробуйте ещё раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxPassword.Clear();
                textBoxPassword.Focus();
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
