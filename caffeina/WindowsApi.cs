using System.Runtime.InteropServices;

namespace Caffeina
{
    /// <summary>
    /// Windows API declarations for power management and system control
    /// </summary>
    public static class WindowsApi
    {
        #region Power Management

        /// <summary>
        /// Execution state flags for SetThreadExecutionState
        /// </summary>
        [Flags]
        public enum ExecutionState : uint
        {
            /// <summary>
            /// Away mode should be used only by media-recording and media-distribution applications
            /// </summary>
            ES_AWAYMODE_REQUIRED = 0x00000040,
            
            /// <summary>
            /// Prevents the system from entering sleep or turning off the display while the thread is running
            /// </summary>
            ES_CONTINUOUS = 0x80000000,
            
            /// <summary>
            /// Forces the display to be on by resetting the display idle timer
            /// </summary>
            ES_DISPLAY_REQUIRED = 0x00000002,
            
            /// <summary>
            /// Forces the system to be in the working state by resetting the system idle timer
            /// </summary>
            ES_SYSTEM_REQUIRED = 0x00000001,
            
            /// <summary>
            /// This value is not supported. If ES_USER_PRESENT is combined with other esFlags values, the call will fail and none of the specified states will be set
            /// </summary>
            ES_USER_PRESENT = 0x00000004
        }

        /// <summary>
        /// Enables an application to inform the system that it is in use, thereby preventing the system from entering sleep or turning off the display while the application is running
        /// </summary>
        /// <param name="esFlags">The thread's execution requirements</param>
        /// <returns>If the function succeeds, the return value is the previous thread execution state</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        #endregion

        #region Mouse and Keyboard Input

        /// <summary>
        /// Input types for SendInput
        /// </summary>
        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;

        /// <summary>
        /// Mouse event flags
        /// </summary>
        [Flags]
        public enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }

        /// <summary>
        /// Keyboard event flags
        /// </summary>
        [Flags]
        public enum KeyEventFlags : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008
        }

        /// <summary>
        /// Virtual key codes
        /// </summary>
        public enum VirtualKeyCode : ushort
        {
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12,
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3
        }

        /// <summary>
        /// Mouse input structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Keyboard input structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public VirtualKeyCode wVk;
            public ushort wScan;
            public KeyEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Hardware input structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        /// <summary>
        /// Input union structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        /// <summary>
        /// Input structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks
        /// </summary>
        /// <param name="nInputs">The number of structures in the pInputs array</param>
        /// <param name="pInputs">An array of INPUT structures</param>
        /// <param name="cbSize">The size, in bytes, of an INPUT structure</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        #endregion

        #region System Parameters

        /// <summary>
        /// System parameters info actions
        /// </summary>
        public enum SPI : uint
        {
            SPI_GETSCREENSAVEACTIVE = 0x0010,
            SPI_SETSCREENSAVEACTIVE = 0x0011,
            SPI_GETSCREENSAVETIMEOUT = 0x000E,
            SPI_SETSCREENSAVETIMEOUT = 0x000F,
            SPI_GETLOWPOWERTIMEOUT = 0x004F,
            SPI_GETPOWEROFFTIMEOUT = 0x0050,
            SPI_SETLOWPOWERTIMEOUT = 0x0051,
            SPI_SETPOWEROFFTIMEOUT = 0x0052,
            SPI_GETLOWPOWERACTIVE = 0x0053,
            SPI_GETPOWEROFFACTIVE = 0x0054,
            SPI_SETLOWPOWERACTIVE = 0x0055,
            SPI_SETPOWEROFFACTIVE = 0x0056
        }

        /// <summary>
        /// System parameters info flags
        /// </summary>
        [Flags]
        public enum SPIF : uint
        {
            None = 0x00,
            SPIF_UPDATEINIFILE = 0x01,
            SPIF_SENDCHANGE = 0x02,
            SPIF_SENDWININICHANGE = 0x02
        }

        /// <summary>
        /// Retrieves or sets the value of one of the system-wide parameters
        /// </summary>
        /// <param name="uiAction">The system-wide parameter to be retrieved or set</param>
        /// <param name="uiParam">A parameter whose usage and format depends on the system parameter being queried or set</param>
        /// <param name="pvParam">A parameter whose usage and format depends on the system parameter being queried or set</param>
        /// <param name="fWinIni">If a system parameter is being set, specifies whether the user profile is to be updated</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref uint pvParam, SPIF fWinIni);

        /// <summary>
        /// Retrieves or sets the value of one of the system-wide parameters (bool version)
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref bool pvParam, SPIF fWinIni);

        #endregion

        #region Cursor Position

        /// <summary>
        /// Point structure for cursor position
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates
        /// </summary>
        /// <param name="lpPoint">A pointer to a POINT structure that receives the screen coordinates of the cursor</param>
        /// <returns>Returns nonzero if successful or zero otherwise</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point lpPoint);

        /// <summary>
        /// Moves the cursor to the specified screen coordinates
        /// </summary>
        /// <param name="x">The new x-coordinate of the cursor, in screen coordinates</param>
        /// <param name="y">The new y-coordinate of the cursor, in screen coordinates</param>
        /// <returns>Returns nonzero if successful or zero otherwise</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        #endregion
    }
}
