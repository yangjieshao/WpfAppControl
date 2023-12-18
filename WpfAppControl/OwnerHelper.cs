using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfAppControl
{
    /// <summary>
    /// </summary>
    public static class OwnerHelper
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// sets the owner of a System.Windows.Forms.Form to a System.Windows.Window
        /// </summary>
        /// <param name="form"></param>
        /// <param name="owner"></param>
        public static void SetOwner(System.Windows.Window owner, System.Windows.Forms.Form form)
        {
            var helper = new WindowInteropHelper(owner);
            _ = SetWindowLong(new HandleRef(form, form.Handle), -8, helper.Handle.ToInt32());
        }

        /// <summary>
        /// </summary>
        /// <param name="parentForm"></param>
        /// <param name="window"></param>
        public static bool SetOwner(System.Windows.Forms.Form parentForm, System.Windows.Window window)
        {
            var ret = new WindowInteropHelper(window)
            {
                Owner = parentForm.Handle
            };
            return ret != null;
        }
    }
}