// File: NoteCreationForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoteTray
{
    public partial class NoteCreationForm : Form
    {
        private TextBox noteTextBox;
        private Button createButton;
        private Button closeButton;
        private Button minimizeButton;
        private Panel controlPanel;
        private NoteTrayApp app;

        public NoteCreationForm(NoteTrayApp app)
        {
            this.app = app;
            InitializeComponent();

            // Настраиваем отправку формы по нажатию Enter
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && e.Control)
                {
                    CreateButton_Click(this, EventArgs.Empty);
                }
            };
        }

        private void NoteCreationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Мы больше не используем этот метод,
            // так как создали собственные кнопки закрытия
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(noteTextBox.Text))
            {
                app.CreateNewNote(noteTextBox.Text);
                noteTextBox.Clear();
                Hide();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите текст заметки.", "Пустая заметка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Обработка сообщений Windows
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;

            // Перехватываем сообщение о минимизации и скрываем форму вместо сворачивания
            if (m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_MINIMIZE)
            {
                this.Hide();
                return;
            }

            base.WndProc(ref m);
        }
    }
}
