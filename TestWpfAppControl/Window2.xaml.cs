using System;
using System.IO;
using System.Windows;

namespace TestWpfAppControl
{
    /// <summary>
    /// </summary>
    public partial class Window2 : Window
    {
        private bool _hadFirstShow = false;
        public Window2()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (!_hadFirstShow
                && IsVisible)
            {
                _hadFirstShow = true;
                OnShown();
            }
        }

        /// <summary>
        /// 第一次渲染
        /// </summary>
        private void OnShown()
        {
            if (File.Exists("TempApp1.exe"))
            {
                appControl.CanMultiple = true;
                appControl.ExePath = "TempApp1";
                appControl2.CanMultiple = true;
                appControl2.ExePath = "TempApp1";
            }
        }
    }
}
