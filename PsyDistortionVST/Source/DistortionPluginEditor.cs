using System;
using System.Drawing;
using System.Windows.Forms;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace VSTDistortion
{
    public partial class DistortionPluginEditor : IVstPluginEditor
    {
        private DistortionPlugin _plugin;
        private DistortionForm _form;
        private IntPtr _parentHwnd;

        public DistortionPluginEditor(DistortionPlugin plugin)
        {
            _plugin = plugin;
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, 384, 410); }
        }

        public VstKnobMode KnobMode { get; set; } = VstKnobMode.CircularMode;

        public void Open(IntPtr hWnd)
        {
            try
            {
                _parentHwnd = hWnd;

                // Создаем форму
                _form = new DistortionForm(_plugin);
                _form.TopLevel = false;
                _form.FormBorderStyle = FormBorderStyle.None;
                _form.Size = new Size(384, 410);
                _form.Location = new Point(0, 0);

                // Пытаемся получить родительский контрол
                Control parentControl = Control.FromHandle(hWnd);
                if (parentControl != null)
                {
                    // Если удалось получить Control
                    parentControl.Controls.Add(_form);
                    _form.Dock = DockStyle.Fill;
                    _form.Show();
                }
                else
                {
                    // Альтернативный способ - используем Win32 API
                    _form.Show();
                    SetParent(_form.Handle, hWnd);
                }

                // Принудительно обновляем отображение
                _form.Invalidate();
                _form.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при открытии редактора: {ex.Message}");
            }
        }

        public void Close()
        {
            try
            {
                if (_form != null)
                {
                    _form.Hide();

                    // Убираем из родительского контрола
                    if (_form.Parent != null)
                    {
                        _form.Parent.Controls.Remove(_form);
                    }

                    _form.Close();
                    _form.Dispose();
                    _form = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии редактора: {ex.Message}");
            }
        }

        public void ProcessIdle()
        {
            if (_form != null)
            {
                Application.DoEvents();
            }
        }
        
        // ИСПРАВЛЕНИЕ: метод для обновления формы из плагина
        public void UpdateFromPlugin()
        {
            if (_form != null)
            {
                _form.UpdateFromPlugin();
            }
        }
        
        // ИСПРАВЛЕНИЕ: Метод для установки фокуса на текстовое поле
        public void FocusTextBox()
        {
            if (_form != null)
            {
                _form.FocusPresetNameTextBox();
            }
        }

        public bool KeyDown(byte keyCode, VstVirtualKey virtualKey, VstModifierKeys modifiers)
        {
            // ИСПРАВЛЕНИЕ: Простая обработка клавиатурного ввода через keyCode
            if (_form != null && _form.Visible)
            {
                // Проверяем, активно ли поле ввода имени пресета
                if (_form.ActiveControl is System.Windows.Forms.TextBox textBox)
                {
                    try
                    {
                        // Обрабатываем Backspace (код 8)
                        if (keyCode == 8)
                        {
                            if (textBox.Text.Length > 0 && textBox.SelectionStart > 0)
                            {
                                int pos = textBox.SelectionStart;
                                textBox.Text = textBox.Text.Remove(pos - 1, 1);
                                textBox.SelectionStart = pos - 1;
                            }
                            return true;
                        }
                        // Обрабатываем Delete (код 127)
                        else if (keyCode == 127)
                        {
                            if (textBox.SelectionStart < textBox.Text.Length)
                            {
                                int pos = textBox.SelectionStart;
                                textBox.Text = textBox.Text.Remove(pos, 1);
                                textBox.SelectionStart = pos;
                            }
                            return true;
                        }
                        // Обрабатываем обычные символы (коды 32-126: пробел, буквы, цифры, знаки)
                        else if (keyCode >= 32 && keyCode <= 126)
                        {
                            char keyChar = (char)keyCode;
                            int pos = textBox.SelectionStart;
                            textBox.Text = textBox.Text.Insert(pos, keyChar.ToString());
                            textBox.SelectionStart = pos + 1;
                            return true;
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки
                    }
                }
            }
            return false; // Не обработано
        }

        public bool KeyUp(byte keyCode, VstVirtualKey virtualKey, VstModifierKeys modifiers)
        {
            // Просто возвращаем false - не обрабатываем KeyUp события
            return false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}