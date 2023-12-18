using System;
using System.Windows;
using System.Windows.Media;

namespace WpfAppControl
{
    public static class Ex
    {
        public static void GetTransForm(this Transform transform, ref double scaleX, ref double scaleY)
        {
            if (transform != null)
            {
                if (transform.Value.Equals(Matrix.Identity))
                {
                    return;
                }
                if (transform is ScaleTransform)
                {
                    var scaleTransform = transform as ScaleTransform;
                    scaleX *= scaleTransform.ScaleX;
                    scaleY *= scaleTransform.ScaleY;
                }
                else if (transform is RotateTransform)
                {
                    // 旋转
                    var rotateTransform = transform as RotateTransform;
                    if (rotateTransform.Angle % 360 != 0)
                    {
                        throw new Exception("can not RotateTransform");
                    }
                }
                else if (transform is SkewTransform)
                {
                    // 倾斜
                    var skewTransform = transform as SkewTransform;
                    if (skewTransform.AngleX % 360 != 0
                        || skewTransform.AngleY % 360 != 0)
                    {
                        throw new Exception("can not SkewTransform");
                    }
                }
                else if (transform is TranslateTransform)
                {
                    // 移动
                    // do not need transform
                }
                else if (transform is MatrixTransform matrixTransform)
                {
                    if (Math.Abs(matrixTransform.Value.M12) > 0.00001
                        || Math.Abs(matrixTransform.Value.M21) > 0.00001)
                    {
                        throw new Exception("can not Skew and Rotate");
                    }
                    scaleX *= Math.Abs(matrixTransform.Value.M11);
                    scaleY *= Math.Abs(matrixTransform.Value.M22);
                }
                else if (transform is TransformGroup)
                {
                    // 变换组
                    foreach (var childTransform in (transform as TransformGroup).Children)
                    {
                        childTransform.GetTransForm(ref scaleX, ref scaleY);
                    }
                }
            }
        }

        public static ContainerVisual GetContainerVisual(this DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is ContainerVisual containerVisual)
                {
                    return containerVisual;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        public static UIElement GetParentUIElement(this DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is UIElement uIElement)
                {
                    return uIElement;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}