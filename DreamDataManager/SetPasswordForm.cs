// SetPasswordForm.cs
using System;
using System.Windows.Forms;

namespace DreamDiary
{
    public partial class SetPasswordForm : Form
    {
        public string NewPassword { get; private set; }

        public SetPasswordForm()
        {
            InitializeComponent();
        }

        private void ButtonSetPassword_Click(object sender, EventArgs e)
        {
            string password = textBoxPassword.Text;
            string confirmPassword = textBoxConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пароль не может быть пустым.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают. Пожалуйста, попробуйте ещё раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxPassword.Clear();
                textBoxConfirmPassword.Clear();
                textBoxPassword.Focus();
                return;
            }

            NewPassword = password;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
