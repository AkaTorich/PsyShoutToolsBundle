// File: NoteTrayApp.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NoteTray
{
    public class NoteTrayApp : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private List<NoteWindow> activeNotes;
        private NoteCreationForm noteCreationForm;

        public NoteTrayApp()
        {
            // Инициализация списка активных заметок
            activeNotes = new List<NoteWindow>();

            // Настройка меню в трее
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Создать заметку", null, OnCreateNote);
            trayMenu.Items.Add("Список заметок", null, OnShowNotesList);
            trayMenu.Items.Add("-"); // Разделитель
            trayMenu.Items.Add("Выход", null, OnExit);

            // Настройка иконки в трее
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information, // Можно заменить на свою иконку
                ContextMenuStrip = trayMenu,
                Visible = true,
                Text = "NoteTray"
            };

            // Добавляем обработчик двойного клика
            trayIcon.MouseDoubleClick += OnTrayIconDoubleClick;
        }

        private void OnTrayIconDoubleClick(object sender, MouseEventArgs e)
        {
            // Показываем форму создания заметки при двойном клике на иконке трея
            if (e.Button == MouseButtons.Left)
            {
                ShowNoteCreationForm();
            }
        }

        private void OnCreateNote(object sender, EventArgs e)
        {
            ShowNoteCreationForm();
        }

        private void ShowNoteCreationForm()
        {
            if (noteCreationForm == null || noteCreationForm.IsDisposed)
            {
                noteCreationForm = new NoteCreationForm(this);
                noteCreationForm.Show();
            }
            else
            {
                // Если форма существует, но скрыта, показываем её
                noteCreationForm.Show();
                noteCreationForm.WindowState = FormWindowState.Normal;
                noteCreationForm.Activate();
                noteCreationForm.BringToFront();
            }
        }

        private void OnShowNotesList(object sender, EventArgs e)
        {
            if (activeNotes.Count == 0)
            {
                MessageBox.Show("У вас пока нет активных заметок.", "Список заметок",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var notesListForm = new Form
            {
                Text = "Активные заметки",
                Size = new Size(300, 400),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var notesList = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                Dock = DockStyle.Fill
            };

            notesList.Columns.Add("Содержимое", 270);

            foreach (var note in activeNotes)
            {
                var noteContent = note.NoteText;
                // Обрезаем текст, если он слишком длинный
                if (noteContent.Length > 30)
                    noteContent = noteContent.Substring(0, 27) + "...";

                var item = new ListViewItem(noteContent);
                // Сохраняем ссылку на объект заметки в тэге элемента списка
                item.Tag = note;
                notesList.Items.Add(item);
            }

            var showBtn = new Button
            {
                Text = "Показать",
                Dock = DockStyle.Bottom
            };

            showBtn.Click += (s, args) =>
            {
                if (notesList.SelectedItems.Count > 0)
                {
                    // Получаем заметку непосредственно из тэга выбранного элемента
                    var note = (NoteWindow)notesList.SelectedItems[0].Tag;
                    note.BringToFront();
                    notesListForm.Close();
                }
            };

            var closeBtn = new Button
            {
                Text = "Закрыть заметку",
                Dock = DockStyle.Bottom
            };

            closeBtn.Click += (s, args) =>
            {
                if (notesList.SelectedItems.Count > 0)
                {
                    int index = notesList.SelectedIndices[0];
                    // Получаем заметку непосредственно из тэга выбранного элемента
                    var note = (NoteWindow)notesList.SelectedItems[0].Tag;

                    // Удаляем заметку из активных заметок
                    activeNotes.Remove(note);
                    // Закрываем окно заметки
                    note.Close();
                    // Удаляем элемент из списка
                    notesList.Items.RemoveAt(index);

                    if (notesList.Items.Count == 0)
                        notesListForm.Close();
                }
            };

            notesListForm.Controls.Add(notesList);
            notesListForm.Controls.Add(showBtn);
            notesListForm.Controls.Add(closeBtn);

            notesListForm.Show();
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Закрываем все активные заметки перед выходом
            foreach (var note in activeNotes.ToList())
            {
                note.Close();
            }

            trayIcon.Visible = false;
            Application.Exit();
        }

        public void CreateNewNote(string noteText)
        {
            var noteWindow = new NoteWindow(noteText);
            activeNotes.Add(noteWindow);
            noteWindow.FormClosed += (s, e) => activeNotes.Remove(noteWindow);
            noteWindow.Show();
        }
    }
}