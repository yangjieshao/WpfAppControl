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
    public partial class WindowFormHost : UserControl
    {
        private System.Windows.Forms.Form _childWindow;

        /// <summary>
        /// </summary>
        public WindowFormHost()
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

                    var top = Convert.ToInt32(point.Y);
                    var left = Convert.ToInt32(point.X);
                    var width = Convert.ToInt32(this.ActualWidth * DPIHeler.Percentage * scaleX);
                    var height = Convert.ToInt32(this.ActualHeight * DPIHeler.Percentage * scaleY);

                    if (top != ChildWindow.Top
                        || left != ChildWindow.Left
                        || width != ChildWindow.Width
                        || height != ChildWindow.Height)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            ChildWindow.Invoke(new Action(() =>
                            {
                                ChildWindow.Width = width;
                                ChildWindow.Height = height;
                                ChildWindow.Top = top;
                                ChildWindow.Left = left;
                            }));
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private void WindowsHost_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetChildVisible();
        }

        /// <summary>
        /// </summary>
        public System.Windows.Forms.Form ChildWindow
        {
            set
            {
                if (_childWindow != null)
                {
                    _childWindow.Closed -= ChildWindow_Closed;
                    _childWindow.KeyDown -= ChildWindow_KeyDown;
                    _childWindow.FormClosing -= ChildWindow_FormClosing;
                    _childWindow.Close();
                    _childWindow = null;
                }
                _childWindow = value;
                _childWindow.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                _childWindow.ShowInTaskbar = false;
                _childWindow.Closed += ChildWindow_Closed;
                _childWindow.KeyDown += ChildWindow_KeyDown;
                _childWindow.FormClosing += ChildWindow_FormClosing;
                SetChildVisible();
            }
            get
            {
                return _childWindow;
            }
        }

        private void ChildWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (e.CloseReason == System.Windows.Forms.CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void SetChildVisible()
        {
            if (this.ChildWindow != null)
            {
                if (this.IsVisible)
                {
                    if (_childWindow.Owner == null)
                    {
                        OwnerHelper.SetOwner(Window.GetWindow(this), _childWindow);
                    }
                    this.ChildWindow.Show();
                }
                else if (!this.IsVisible)
                {
                    this.ChildWindow.Hide();
                }
            }
        }

        private void ChildWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                _childWindow.Owner.Close();
            }
        }

        private void ChildWindow_Closed(object sender, EventArgs e)
        {
            _childWindow = null;
        }
    }
}