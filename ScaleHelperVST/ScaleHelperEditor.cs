// ScaleHelperEditor.cs - Простое решение с сохранением состояния
using System;
using System.Drawing;
using System.Windows.Forms;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace ScaleHelperVST
{
    internal sealed class ScaleHelperEditor : IVstPluginEditor
    {
        private ScaleHelperForm _form;
        private ScaleHelperVstPlugin _plugin;
        private IntPtr _parentHwnd;

        // Добавляем статические поля для сохранения состояния
        private static int _lastCategoryIndex = 0;
        private static int _lastScaleIndex = 0;
        private static int _lastTonicIndex = 0;
        private static string _lastNotesText = "";
        private static bool _stateInitialized = false;

        public ScaleHelperEditor(ScaleHelperVstPlugin plugin)
        {
            _plugin = plugin;
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, 788, 366); }
        }

        public VstKnobMode KnobMode { get; set; } = VstKnobMode.CircularMode;

        public void Open(IntPtr hWnd)
        {
            try
            {
                _parentHwnd = hWnd;

                // Создаем форму
                _form = new ScaleHelperForm();
                _form.TopLevel = false;
                _form.FormBorderStyle = FormBorderStyle.None;
                _form.Size = new Size(788, 366);
                _form.Location = new Point(0, 0);

                // Восстанавливаем состояние формы после ее полной инициализации
                _form.Load += (sender, e) => RestoreFormState();

                // Подписываемся на изменения для сохранения состояния
                _form.StateChanged += OnFormStateChanged;

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
                    // Сохраняем состояние перед закрытием
                    SaveFormState();

                    // Отписываемся от событий
                    _form.StateChanged -= OnFormStateChanged;

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

        private void SaveFormState()
        {
            if (_form != null && !_form.IsDisposed)
            {
                _lastCategoryIndex = _form.GetCategoryIndex();
                _lastScaleIndex = _form.GetScaleIndex();
                _lastTonicIndex = _form.GetTonicIndex();
                _lastNotesText = _form.GetNotesText();
                _stateInitialized = true;
            }
        }

        private void RestoreFormState()
        {
            if (_form != null && !_form.IsDisposed && _stateInitialized)
            {
                // Отключаем события временно, чтобы избежать циклических вызовов
                _form.StateChanged -= OnFormStateChanged;

                try
                {
                    _form.SetCategoryIndex(_lastCategoryIndex);
                    // Небольшая задержка для загрузки гамм
                    Application.DoEvents();
                    _form.SetScaleIndex(_lastScaleIndex);
                    _form.SetTonicIndex(_lastTonicIndex);
                    _form.SetNotesText(_lastNotesText);
                }
                finally
                {
                    // Включаем события обратно
                    _form.StateChanged += OnFormStateChanged;
                }
            }
        }

        private void OnFormStateChanged(object sender, EventArgs e)
        {
            // Автоматически сохраняем состояние при изменениях
            SaveFormState();
        }

        public bool KeyDown(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        public bool KeyUp(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        public void ProcessIdle()
        {
            // Обновляем форму если она существует
            if (_form != null && !_form.IsDisposed)
            {
                Application.DoEvents();
            }
        }

        // Win32 API для установки родителя окна
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}