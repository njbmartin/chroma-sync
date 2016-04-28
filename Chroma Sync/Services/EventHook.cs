using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultrabox.ChromaSync.Services
{
    class EventHook
    {

        static WinEventDelegate dele = null;

        // Enums
        public enum EventContants : uint
        {
            EVENT_SYSTEM_SOUND = 0x1,
            EVENT_SYSTEM_ALERT = 0x2,
            EVENT_SYSTEM_FOREGROUND = 0x3,
            EVENT_SYSTEM_MENUSTART = 0x4,
            EVENT_SYSTEM_MENUEND = 0x5,
            EVENT_SYSTEM_MENUPOPUPSTART = 0x6,
            EVENT_SYSTEM_MENUPOPUPEND = 0x7,
            EVENT_SYSTEM_CAPTURESTART = 0x8,
            EVENT_SYSTEM_CAPTUREEND = 0x9,
            EVENT_SYSTEM_MOVESIZESTART = 0xa,
            EVENT_SYSTEM_MOVESIZEEND = 0xb,
            EVENT_SYSTEM_CONTEXTHELPSTART = 0xc,
            EVENT_SYSTEM_CONTEXTHELPEND = 0xd,
            EVENT_SYSTEM_DRAGDROPSTART = 0xe,
            EVENT_SYSTEM_DRAGDROPEND = 0xf,
            EVENT_SYSTEM_DIALOGSTART = 0x10,
            EVENT_SYSTEM_DIALOGEND = 0x11,
            EVENT_SYSTEM_SCROLLINGSTART = 0x12,
            EVENT_SYSTEM_SCROLLINGEND = 0x13,
            EVENT_SYSTEM_SWITCHSTART = 0x14,
            EVENT_SYSTEM_SWITCHEND = 0x15,
            EVENT_SYSTEM_MINIMIZESTART = 0x16,
            EVENT_SYSTEM_MINIMIZEEND = 0x17
        }



        [Flags]
        internal enum SetWinEventHookParameter
        {
            WINEVENT_OUTOFCONTEXT = 0,
            WINEVENT_SKIPOWNTHREAD = 1,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_INCONTEXT = 4
        }


        public static void SetHook()
        {
            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook((uint)EventContants.EVENT_SYSTEM_FOREGROUND, (uint)EventContants.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, (uint)(SetWinEventHookParameter.WINEVENT_OUTOFCONTEXT | SetWinEventHookParameter.WINEVENT_SKIPOWNPROCESS));
        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Console.WriteLine(GetActiveWindowTitle());
        }
    }
}
