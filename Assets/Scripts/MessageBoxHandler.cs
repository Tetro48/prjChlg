using System;
using System.Runtime.InteropServices;
public static class MessageBoxHandler
{
    public static IntPtr windowPointer;
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}