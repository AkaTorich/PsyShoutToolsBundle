// File: NoteWindow.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoteTray
{
    public partial class NoteWindow : Form
    {
        public string NoteText { get; private set; }
        private Panel controlPanel;
        private Button pinTopMostButton;
        private Button pinDesktopButton;
        private Button closeButton;
        private TextBox textBox;
        private bool isPinnedToDesktop = false;

        public NoteWindow(string noteText)
        {
            NoteText = noteText;
            InitializeComponent();

            // Заполняем текстовое поле
            textBox.Text = noteText;

            // Добавляем поддержку работы с системным треем
            ShowInTaskbar = false;

            // Добавляем рамку вокруг окна
            Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(
                    new Pen(Color.Orange, 2),
                    new Rectangle(0, 0, Width - 1, Height - 1)
                );
            };
        }

        private void ControlPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PinTopMostButton_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            pinTopMostButton.BackColor = TopMost ? Color.LightGreen : SystemColors.Control;
        }

        private void PinDesktopButton_Click(object sender, EventArgs e)
        {
            isPinnedToDesktop = !isPinnedToDesktop;
            pinDesktopButton.BackColor = isPinnedToDesktop ? Color.LightGreen : SystemColors.Control;

            if (isPinnedToDesktop)
            {
                // Отправляем окно на задний план (на рабочий стол)
                NativeMethods.SetWindowPos(
                    Handle,
                    NativeMethods.HWND_BOTTOM,
                    0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE
                );
            }
            else
            {
                // Возвращаем нормальное поведение
                BringToFront();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BringToFront();
        }
    }
}