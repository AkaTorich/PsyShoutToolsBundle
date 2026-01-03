// AddDreamForm.cs
using System;
using System.Windows.Forms;

namespace DreamDiary
{
    public partial class AddDreamForm : Form
    {
        public Dream UpdatedDream { get; private set; }
        private byte[] EncryptionKey;
        private byte[] EncryptionIV;

        public AddDreamForm(byte[] key, byte[] iv)
        {
            InitializeComponent();
            EncryptionKey = key;
            EncryptionIV = iv;
            dateTimePickerDreamDate.Value = DateTime.Now;
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxTitle.Text))
            {
                MessageBox.Show("Пожалуйста, введите заголовок сна.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxDescription.Text))
            {
                MessageBox.Show("Пожалуйста, введите описание сна.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdatedDream = new Dream
            {
                Title = textBoxTitle.Text.Trim(),
                Description = textBoxDescription.Text.Trim(),
                Date = dateTimePickerDreamDate.Value.Date
            };
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
