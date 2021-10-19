using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Priorities
{
    class Program
    {

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
        [DllImport("user32.dll")]
        private static extern IntPtr UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int EVENT_SYSTEM_FOREGROUND = 3;

        private static int previousId;
        private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private static void WindowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);
            try
            {
                Process.GetProcessById((int)processId).PriorityClass = ProcessPriorityClass.RealTime;
                if ((int)processId != previousId)
                {
                    try
                    {
                        Process.GetProcessById(previousId).PriorityClass = ProcessPriorityClass.Idle;
                    }
                    catch (Exception e) { }
                }
                previousId = (int)processId;

            }
            catch (Exception e) { }
        }
        static void Main(string[] args)
        {
            IntPtr hook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                WindowEventCallback,
                0,
                0,
                WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

            Array.ForEach(Process.GetProcesses(), process => {
                try
                {
                    process.PriorityClass = ProcessPriorityClass.Idle;
                }catch(Exception e){}
            });
            EventLoop.Run();
            UnhookWinEvent(hook);
        }
    }

    public static class EventLoop
    {
        public static void Run()
        {
            MSG msg;

            while (true)
            {
                if (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    if (msg.Message == WM_QUIT)
                        break;

                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                Thread.Sleep(100);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr Hwnd;
            public uint Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public System.Drawing.Point Point;
        }

        const uint PM_NOREMOVE = 0;
        const uint PM_REMOVE = 1;

        const uint WM_QUIT = 0x0012;

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);
    }
}
