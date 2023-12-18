using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfAppControl
{
    /// <summary>
    /// </summary>
    public static class GlassHelper
    {
        /// <summary>
        /// </summary>
        public static bool ExtendGlassFrame(this Window window)
        {
            return ExtendGlassFrame(window, Colors.Transparent);
        }

        /// <summary>
        /// </summary>
        public static bool ExtendGlassFrame(this Window window, Color transparentColor)
        {
            return ExtendGlassFrame(window, new Thickness(-1), transparentColor);
        }

        /// <summary>
        /// </summary>
        public static bool ExtendGlassFrame(this Window window, Thickness margin)
        {
            return ExtendGlassFrame(window, margin, Colors.Transparent);
        }

        /// <summary>
        /// </summary>
        public static bool ExtendGlassFrame(this Window window, Thickness margin, Color transparentColor)
        {
            bool result = false;
            if (DwmIsCompositionEnabled())
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    // Set the background to transparent from both the WPF and Win32 perspectives
                    window.Background = Brushes.Transparent;
                    HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = transparentColor;

                    var margins = new MARGINS(margin);
                    DwmExtendFrameIntoClientArea(hwnd, ref margins);
                    result = true;
                }
            }
            return result;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();
    }

    /// <summary>
    /// </summary>
    internal struct MARGINS
    {
        public MARGINS(Thickness t)
        {
            Left = (int)t.Left;
            Right = (int)t.Right;
            Top = (int)t.Top;
            Bottom = (int)t.Bottom;
        }

        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }
}