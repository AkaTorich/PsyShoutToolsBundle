// DreamDetailsForm.cs
using System;
using System.Windows.Forms;

namespace DreamDiary
{
    public partial class DreamDetailsForm : Form
    {
        public Dream UpdatedDream { get; private set; }
        private byte[] EncryptionKey;
        private byte[] EncryptionIV;

        public DreamDetailsForm(Dream dream, byte[] key, byte[] iv)
        {
            InitializeComponent();
            EncryptionKey = key;
            EncryptionIV = iv;
            LoadDreamDetails(dream);
        }

        private void LoadDreamDetails(Dream dream)
        {
            textBoxTitle.Text = dream.Title;
            textBoxDescription.Text = dream.Description;
            labelDate.Text = dream.Date.ToShortDateString();

            // Создаем копию сна для обновления
            UpdatedDream = new Dream
            {
                ID = dream.ID,
                Title = dream.Title,
                Description = dream.Description,
                Date = dream.Date
            };
        }

        private void ButtonSave_Click(object sender, EventArgs e)
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

            // Обновляем свойства сна
            UpdatedDream.Title = textBoxTitle.Text.Trim();
            UpdatedDream.Description = textBoxDescription.Text.Trim();
            // Дата остаётся неизменной

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть без сохранения изменений?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;  // Не сохраняем изменения
                this.Close();
            }
        }
    }
}
