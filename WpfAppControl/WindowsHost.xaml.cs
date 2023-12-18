using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfAppControl
{
    /// <summary>
    /// </summary>
    public partial class WindowsHost : UserControl
    {
        /// <summary>
        /// </summary>
        public WindowsHost()
        {
            InitializeComponent();
            this.IsVisibleChanged += WindowsHost_IsVisibleChanged;
            CompositionTarget.Rendering += new EventHandler(WindowsHostPositionChange);
        }

        private void WindowsHostPositionChange(object sender, EventArgs e)
        {
            if (ChildWindow != null && this.IsLoaded)
            {
                try
                {
                    var point = this.PointToScreen(Point.Zero);

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
                            var transform = containerVisual.Transform;
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

                    var top = point.Y / DPIHeler.Percentage;
                    var left = point.X / DPIHeler.Percentage;
                    var width = this.ActualWidth * scaleX;
                    var height = this.ActualHeight * scaleY;

                    if (top != ChildWindow.Top
                        || left != ChildWindow.Left
                        || width != ChildWindow.Width
                        || height != ChildWindow.Height)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            ChildWindow.Dispatcher.Invoke(new Action(() =>
                            {
                                ChildWindow.Width = width;
                                ChildWindow.Height = height;
                                ChildWindow.Top = top;
                                ChildWindow.Left = left;
                            }));
                        });
                    }
                }
                catch
                {
                    // no use
                }
            }
        }

        private void WindowsHost_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetChildVisible();
        }

        private void SetChildVisible()
        {
            if (this.ChildWindow != null)
            {
                if (this.IsVisible
                    && !this.ChildWindow.IsActive)
                {
                    if (_childWindow.Owner == null)
                    {
                        _childWindow.Owner = Window.GetWindow(this);
                    }
                    this.ChildWindow.Show();
                }
                else if (!this.IsVisible
                    && this.ChildWindow.IsVisible)
                {
                    this.ChildWindow.Hide();
                }
            }
        }

        public bool NeedSetChildViewransparency { set; get; } = true;

        private Window _childWindow;

        /// <summary>
        /// </summary>
        public Window ChildWindow
        {
            set
            {
                if (_childWindow != null)
                {
                    _childWindow.Close();
                    _childWindow = null;
                }
                _childWindow = value;
                if (_childWindow == null)
                {
                    return;
                }
                if (NeedSetChildViewransparency)
                {
                    try
                    {
                        _childWindow.AllowsTransparency = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
                _childWindow.WindowStyle = WindowStyle.None;
                _childWindow.ShowInTaskbar = false;
                _childWindow.Background = null;
                _childWindow.Closing += ChildWindow_Closing;
                _childWindow.KeyDown += ChildWindow_KeyDown;

                var ownerView = Window.GetWindow(this);
                if (ownerView != null)
                {
                    ownerView.Closing += OwnerView_Closing;
                }
                SetChildVisible();
            }
            get
            {
                return _childWindow;
            }
        }

        private void ChildWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _childWindow = null;
        }

        private void OwnerView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_childWindow != null)
            {
                try
                {
                    _childWindow.Close();
                }
                catch
                {
                }
            }
        }

        private void ChildWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.F4) && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                _childWindow.Owner.Close();
            }
        }
    }
}