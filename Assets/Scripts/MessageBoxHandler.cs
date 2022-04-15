using System;
using System.Runtime.InteropServices;
public static class MessageBoxHandler
{
    public static IntPtr windowPointer;
    /// <summary>
    /// This summons a message box. It freezes main thread until it's interacted with.
    /// Read Microsoft's documentation on this first before using it.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}