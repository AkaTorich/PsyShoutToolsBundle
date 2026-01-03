// File: NoteCreationForm.Designer.cs
using System.Drawing;
using System.Windows.Forms;
using System;

namespace NoteTray
{
    partial class NoteCreationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // Настройка формы
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 230);

            // Панель управления с кнопками
            this.controlPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.SkyBlue
            };

            // Кнопка закрытия
            this.closeButton = new Button
            {
                Text = "✕",
                Size = new Size(30, 25),
                Location = new Point(5, 3),
                FlatStyle = FlatStyle.Flat
            };
            this.closeButton.Click += (s, e) => { this.Hide(); };

            // Кнопка минимизации (скрытия в трей)
            this.minimizeButton = new Button
            {
                Text = "—",
                Size = new Size(30, 25),
                Location = new Point(40, 3),
                FlatStyle = FlatStyle.Flat
            };
            this.minimizeButton.Click += (s, e) => { this.Hide(); }; // Скрываем форму в трей

            // Заголовок формы на панели
            Label titleLabel = new Label
            {
                Text = "Создать заметку",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(80, 5),
                AutoSize = true
            };

            // Добавляем элементы на панель
            this.controlPanel.Controls.Add(this.closeButton);
            this.controlPanel.Controls.Add(this.minimizeButton);
            this.controlPanel.Controls.Add(titleLabel);

            // Обработчик для перетаскивания окна
            this.controlPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
                }
            };

            // Поле для ввода текста
            this.noteTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(370, 130),
                Location = new Point(15, 40),
                Font = new Font("Segoe UI", 9.75F)
            };

            // Кнопка создания заметки
            this.createButton = new Button
            {
                Text = "Создать",
                Size = new Size(100, 30),
                Location = new Point(285, 180),
                BackColor = Color.LightSkyBlue
            };
            this.createButton.Click += new EventHandler(this.CreateButton_Click);

            // Добавляем все элементы на форму
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.noteTextBox);
            this.Controls.Add(this.createButton);

            // Добавляем рамку вокруг окна
            this.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(
                    new Pen(Color.DodgerBlue, 2),
                    new Rectangle(0, 0, Width - 1, Height - 1)
                );
            };
        }

        #endregion
    }
}
