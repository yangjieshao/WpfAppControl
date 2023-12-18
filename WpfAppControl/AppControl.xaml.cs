using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfAppControl
{
    /// <summary>
    /// </summary>
    public partial class AppControl : UserControl, IDisposable
    {
        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HWND__
        {
            /// <summary>
            /// </summary>
            public int unused;
        }

        /// <summary>
        /// Track if the application has been created
        /// </summary>
        private bool _iscreated;

        /// <summary>
        /// Handle to the application Window
        /// </summary>
        private IntPtr _appWin;

        /// <summary>
        /// 外部应用进程
        /// </summary>
        public Process AppProcess
        {
            get
            {
                return Childp;
            }
        }

        /// <summary>
        /// The name of the exe to launch
        /// </summary>
        private string _exeName = string.Empty;

        /// <summary>
        /// </summary>
        public string RootDir
        {
            set
            {
                _rootDir = value;
            }
            get
            {
                if (string.IsNullOrEmpty(_rootDir))
                {
                    _rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                return _rootDir;
            }
        }

        private string _rootDir = string.Empty;

        /// <summary>
        /// <para>The name of the exe to launch</para>
        /// <para>(width ".exe")</para>
        /// </summary>
        public string ExeName
        {
            get
            {
                return _exeName;
            }
            private set
            {
                ExitOldProcess();
                Thread.Sleep(200);
                _exeName = value;
                if (this.IsVisible)
                {
                    SetAppWinVisible();
                }
            }
        }

        private string _exePath;

        /// <summary>
        /// <para>The path of the exe to launch</para>
        /// <para>(width ".exe")</para>
        /// </summary>
        public string ExePath
        {
            set
            {
                _exePath = value;
                if (!string.IsNullOrWhiteSpace(_exePath))
                {
                    if (!_exePath.EndsWith(".exe"))
                    {
                        _exePath += ".exe";
                    }
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(_exePath);
                        ExeName = fileInfo.Name;
                    }
                    catch
                    {
                        // no use
                    }
                }
            }
            get
            {
                return _exePath;
            }
        }

        /// <summary>
        /// </summary>
        public string Arguments { set; get; }

        /// <summary>
        /// </summary>
        public string RealProcessMainWindowTitle { set; get; }

        /// <summary>
        /// </summary>
        public bool IsCmd { set; get; } = false;

        /// <summary>
        /// 同一子程序是否可以运行多个实例
        /// </summary>
        public bool CanMultiple { set; get; } = false;

        /// <summary>
        /// </summary>
        public Process Childp { get; set; }

        /// <summary>
        /// 子程序退出
        /// </summary>
        public event EventHandler AppExited;

        /// <summary>
        /// 子程序启动
        /// </summary>
        public event EventHandler AppStarted;

        private bool _isClosing = false;

        /// <summary>
        /// </summary>
        public AppControl()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(OnResize);
            this.Loaded += new RoutedEventHandler(OnLoaded);
            Application.Current.Startup += new StartupEventHandler(OnAppStartup);
        }

        private void AppControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            RemoveWindow();
        }

        /// <summary>
        /// </summary>
        ~AppControl()
        {
            this.Dispose();
        }

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            ExitOldProcess();
        }

        private void ExitOldProcess()
        {
            _isClosing = true;
            _iscreated = false;
            if (Childp != null)
            {
                Childp.Kill();
                Childp.WaitForExit();
                Childp = null;
            }
        }

        /// <summary>
        /// Create control when visibility changes
        /// </summary>
        /// <param name="e">Not used</param>
        /// <param name="s"></param>
        protected void OnLoaded(object s, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Unloaded += new RoutedEventHandler((obj, args) => { this.Dispose(); });
                Window.GetWindow(this).StateChanged += AppControl_StateChanged;
                Window.GetWindow(this).Closing += new System.ComponentModel.CancelEventHandler((obj, args) => { this.Dispose(); });
                this.IsVisibleChanged += AppControl_IsVisibleChanged;

                Window.GetWindow(this).SizeChanged += AppControl_SizeChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            SetAppWinVisible();
        }

        private void AppControl_StateChanged(object sender, EventArgs e)
        {
            if ((sender as Window).WindowState != WindowState.Minimized)
            {
                ShowWindow(_appWin, CmdShow_SHOW);
                RemoveWindow();
            }
            else
            {
                ShowWindow(_appWin, CmdShow_HIDEN);
            }
        }

        private void AppControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IntPtr.Zero.Equals(_appWin))
            {
                if (this.IsVisible)
                {
                    ShowWindow(_appWin, CmdShow_SHOW);
                }
                else
                {
                    ShowWindow(_appWin, CmdShow_HIDEN);
                }
            }
        }

        /// <summary>
        /// </summary>
        public void SetAppWinVisible()
        {
            _isClosing = false;
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            var exeFullPath = _exePath;
            if (!string.IsNullOrWhiteSpace(exeFullPath)
                && !exeFullPath.EndsWith(".exe"))
            {
                exeFullPath += ".exe";
            }
            // If control needs to be initialized/created
            if (!_iscreated
                && !string.IsNullOrWhiteSpace(this._exeName)
                && System.IO.File.Exists(exeFullPath))
            {
                // Mark that control is created
                _iscreated = true;

                // Initialize handle value to invalid
                _appWin = IntPtr.Zero;
                try
                {
                    if (!CanMultiple)
                    {
                        var processes = Process.GetProcesses();
                        var exeName = this._exeName;
                        if (exeName.EndsWith(".exe"))
                        {
                            exeName = exeName.Remove(exeName.Length - 4);
                        }
                        foreach (var processe in processes)
                        {
                            if (processe.ProcessName == exeName)
                            {
                                processe.Kill();
                                processe.WaitForExit();
                            }
                        }
                    }

                    ProcessStartInfo procInfo = null;

                    if (string.IsNullOrEmpty(Arguments))
                    {
                        procInfo = new ProcessStartInfo(exeFullPath)
                        {
                            WorkingDirectory = new System.IO.FileInfo(exeFullPath).DirectoryName,
                            UseShellExecute = false
                        };
                    }
                    else
                    {
                        if (!IsCmd)
                        {
                            procInfo = new ProcessStartInfo(exeFullPath, Arguments)
                            {
                                WorkingDirectory = new System.IO.FileInfo(exeFullPath).DirectoryName,
                                UseShellExecute = false,
                            };
                        }
                        else
                        {
                            procInfo = new ProcessStartInfo(exeFullPath, Arguments)
                            {
                                WorkingDirectory = new System.IO.FileInfo(exeFullPath).DirectoryName,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };
                        }
                    }

                    if (Childp != null)
                    {
                        try
                        {
                            Childp.Kill();
                            Childp = null;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    // Start the process
                    Childp = Process.Start(procInfo);

                    // Wait for process to be created and enter idle condition

                    // Get the main handle
                    if (!IsCmd)
                    {
                        Childp.WaitForInputIdle();
                    }
                    if (string.IsNullOrEmpty(RealProcessMainWindowTitle))
                    {
                        _appWin = Childp.MainWindowHandle;
                        while (IntPtr.Zero.Equals(_appWin))
                        {
                            //Thread.Sleep(200);
                            _appWin = Childp.MainWindowHandle;
                        }
                        Childp.EnableRaisingEvents = true;
                        Childp.Exited += Childp_Exited;
                    }
                    else
                    {
                        bool isFind = false;

                        while (!isFind)
                        {
                            var processes = Process.GetProcesses();
                            foreach (var processe in processes)
                            {
                                if (processe.MainWindowTitle == RealProcessMainWindowTitle)
                                {
                                    isFind = true;
                                    _appWin = processe.MainWindowHandle;
                                    while (IntPtr.Zero.Equals(_appWin))
                                    {
                                        //Thread.Sleep(200);
                                        _appWin = processe.MainWindowHandle;
                                    }
                                    processe.EnableRaisingEvents = true;
                                    processe.Exited += Childp_Exited;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                Thread.Sleep(20);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("Error: " + ex.Message);
                }

                var action = new Action(() =>
                {
                    SetParent();
                    // Remove border and whatnot
                    _ = SetWindowLongA(_appWin, GWL_STYLE, WS_VISIBLE);

                    int iStyle = (int)GetWindowLongA(_appWin, GWL_EXSTYLE);
                    iStyle &= ~WS_EX_APPWINDOW;
                    iStyle |= WS_EX_TOOLWINDOW;
                    _ = SetWindowLongA(_appWin, GWL_EXSTYLE, iStyle);

                    RemoveWindow();

                    AppStarted?.Invoke(this, EventArgs.Empty);
                });

                if (this.CheckAccess())
                {
                    action();
                }
                else
                {
                    if (this.Dispatcher != null
                        && !this.Dispatcher.HasShutdownStarted
                        && !this.Dispatcher.HasShutdownFinished
                        && this.Dispatcher.Thread != null
                        && this.Dispatcher.Thread.IsAlive)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            action();
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        public void CloseAppWin()
        {
            ExitOldProcess();
        }

        private void Childp_Exited(object sender, EventArgs e)
        {
            _iscreated = false;
            if (_isClosing)
            {
                try
                {
                    AppExited?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                SetAppWinVisible();
            }
        }

        /// <summary>
        /// Update display of the executable
        /// </summary>
        /// <param name="e">Not used</param>
        /// <param name="s"></param>
        protected void OnResize(object s, SizeChangedEventArgs e)
        {
            this.InvalidateVisual();
            if (this._appWin != IntPtr.Zero)
            {
                RemoveWindow();
            }
        }

        /// <summary>
        /// </summary>
        public void SetParent()
        {
            // Put it into this form
            var helper = new WindowInteropHelper(Window.GetWindow(this.AppContainer));
            SetParent(_appWin, helper.Handle);
        }

        /// <summary>
        /// Move the window to overlay it on this window
        /// </summary>
        public void RemoveWindow()
        {
            if (_iscreated)
            {
                var view = Window.GetWindow(this);
                if (view != null
                    && view.Content is FrameworkElement windowRootGrid)
                {
                    var windowsStartPoint = view.PointToScreen(Point.Zero);
                    var rootGridPoint = windowRootGrid.PointToScreen(Point.Zero);
                    var startPoint = windowRootGrid.PointFromScreen(this.PointToScreen(Point.Zero));

                    var scaleX = 1.0d;
                    var scaleY = 1.0d;
                    DependencyObject childObject = this;
                    ContainerVisual containerVisual;
                    do
                    {
                        containerVisual = childObject.GetContainerVisual();
                        if (containerVisual != null)
                        {
                            childObject = containerVisual;
                            Transform transform = containerVisual.Transform;
                            transform.GetTransForm(ref scaleX, ref scaleY);
                        }
                    }
                    while (containerVisual != null);

                    UIElement uIElement = this;
                    while (uIElement != null)
                    {
                        var parent = uIElement.GetParentUIElement();
                        if (parent != null)
                        {
                            var generalTransform = uIElement.TransformToVisual(parent);
                            if (generalTransform is Transform transform1)
                            {
                                transform1.GetTransForm(ref scaleX, ref scaleY);
                            }
                        }
                        uIElement = parent;
                    }

                    var x = (int)((startPoint.X + rootGridPoint.X - windowsStartPoint.X) * DPIHeler.Percentage);
                    var y = (int)((startPoint.Y + rootGridPoint.Y - windowsStartPoint.Y) * DPIHeler.Percentage);
                    var cx = (int)(this.ActualWidth * DPIHeler.Percentage * scaleX);
                    var cy = (int)(this.ActualHeight * DPIHeler.Percentage * scaleY);
                    // Debug.WriteLine($"RemoveWindow {DateTime.Now} X:{x} Y:{y} CX:{cx} CY:{cy}");
                    // Move the window to overlay it on this window
                    MoveWindow(_appWin, x, y, cx, cy, true);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            _isClosing = true;
            try
            {
                if (Childp != null)
                {
                    Childp.Kill();
                    Childp = null;
                }
                _appWin = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region user32.dll

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll", EntryPoint = nameof(GetWindowThreadProcessId), SetLastError = true,
             CharSet = CharSet.Unicode, ExactSpelling = true,
             CallingConvention = CallingConvention.StdCall)]
        private static extern long GetWindowThreadProcessId(long hWnd, long lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = nameof(GetWindowLongA), SetLastError = true)]
        private static extern long GetWindowLongA(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = nameof(SetWindowLongA), SetLastError = true)]
        private static extern int SetWindowLongA([System.Runtime.InteropServices.InAttribute()] System.IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        /// <summary>
        /// </summary>
        public enum GetWindowCmd : uint
        {
            /// <summary>
            /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
            /// </summary>
            GW_HWNDFIRST = 0,

            /// <summary>
            /// 返回的句柄标识了在z序最低端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
            /// </summary>
            GW_HWNDLAST = 1,

            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
            /// </summary>
            GW_HWNDNEXT = 2,

            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
            /// </summary>
            GW_HWNDPREV = 3,

            /// <summary>
            /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
            /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
            /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
            /// </summary>
            GW_OWNER = 4,

            /// <summary>
            /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
            /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
            /// </summary>
            GW_CHILD = 5,

            /// <summary>
            /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
            /// 如果无使能窗口，则获得的句柄与指定窗口相同。
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }

        private const int GWL_STYLE = (-16);
        private const int GWL_EXSTYLE = (-20);
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const uint CmdShow_SHOW = 1;
        private const uint CmdShow_HIDEN = 0;

        #endregion user32.dll
    }
}