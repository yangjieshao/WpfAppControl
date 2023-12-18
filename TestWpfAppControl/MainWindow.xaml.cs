using System;
using System.IO;
using System.Windows;
using WpfAppControl;

namespace TestWpfAppControl
{
    /// <summary>
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //this.ExtendGlassFrame();
            this.ExtendGlassFrame(new Thickness(100), System.Windows.Media.Color.FromArgb(188,205,255,50));
            //this.ExtendGlassFrame(System.Windows.Media.Colors.Blue);
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            windowsHost1.ChildWindow = new Window1();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(Window_Initialized));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var view = new Window2
            {
                Owner = this
            };
            view.Show();
            if (File.Exists("TempApp1.exe"))
            {
                //appControl.Arguments = System.IO.Path.Combine(Environment.CurrentDirectory, "抽奖软件", "fbde5608-6529-4348-a211-46902dbce1d1.xml") + "  -nodebug";
                //appControl.RealProcessMainWindowTitle = "微信3D动画签到抽奖软件";
                appControl.IsCmd = false;
                appControl.ExePath = System.IO.Path.Combine(Environment.CurrentDirectory, "TempApp1.exe");
            }
            //appControl.Arguments = System.IO.Path.Combine(Environment.CurrentDirectory, "抽奖软件", "fbde5608-6529-4348-a211-46902dbce1d1.xml") + "  -nodebug";
            //appControl.RealProcessMainWindowTitle = "微信3D动画签到抽奖软件";
            //appControl.IsCmd = true;
            //appControl.ExePath = System.IO.Path.Combine(Environment.CurrentDirectory, "抽奖软件", "bin", "adl");
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            winformHost1.ChildWindow = new Form1();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ChangedButton == System.Windows.Input.MouseButton.Left
                && e.ButtonState== System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}