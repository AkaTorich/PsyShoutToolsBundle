using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace TaskReminderApp.Services
{
    public static class ClipboardHelper
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_A = 0x41;
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const byte VK_X = 0x58;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static string? GetSelectedText()
        {
            try
            {
                string? originalClipboard = null;
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                    }
                }
                catch { }

                Clipboard.Clear();
                Thread.Sleep(50);

                // Копируем выделенный текст
                SendCtrlKey(VK_C);
                Thread.Sleep(100);

                string? selectedText = null;
                if (Clipboard.ContainsText())
                {
                    selectedText = Clipboard.GetText();
                }

                // НЕ восстанавливаем старый буфер сейчас - это сделаем после замены текста
                return selectedText;
            }
            catch
            {
                return null;
            }
        }

        public static string? GetAllTextWithSelection()
        {
            try
            {
                Clipboard.Clear();
                Thread.Sleep(50);

                // Сначала выделяем весь текст (Ctrl+A)
                SendCtrlKey(VK_A);
                Thread.Sleep(100);

                // Затем копируем его (Ctrl+C)
                SendCtrlKey(VK_C);
                Thread.Sleep(100);

                string? selectedText = null;
                if (Clipboard.ContainsText())
                {
                    selectedText = Clipboard.GetText();
                }

                return selectedText;
            }
            catch
            {
                return null;
            }
        }

        public static void ReplaceSelectedText(string text)
        {
            try
            {
                // Вырезаем выделенный текст (Ctrl+X) - это удалит его и оставит курсор на месте
                SendCtrlKey(VK_X);
                Thread.Sleep(50);

                // Вставляем сконвертированный текст
                Clipboard.SetText(text);
                Thread.Sleep(50);

                SendCtrlKey(VK_V);
                Thread.Sleep(50);
            }
            catch { }
        }

        public static void SetSelectedText(string text)
        {
            try
            {
                // Просто вставляем текст поверх выделенного
                Clipboard.SetText(text);
                Thread.Sleep(50);

                SendCtrlKey(VK_V);
                Thread.Sleep(50);
            }
            catch { }
        }

        private static void SendCtrlKey(byte key)
        {
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            keybd_event(key, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(10);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
