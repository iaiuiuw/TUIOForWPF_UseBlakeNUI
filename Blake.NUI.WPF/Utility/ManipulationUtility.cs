using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Blake.NUI.WPF.Utility
{
    public static class ManipulationUtility
    {
        public static void CaptureTouchDeviceToManipulationEnabledParent(DependencyObject element, TouchDevice touchDevice)
        {
            if (element == null)
                return;

            UIElement parent = VisualTreeHelper.GetParent(element) as UIElement;
            if (parent == null)
            {
                touchDevice.Capture(null);
                return;
            }

            if (parent.IsManipulationEnabled)
            {
                touchDevice.Capture(parent as IInputElement);
                return;
            }

            CaptureTouchDeviceToManipulationEnabledParent(parent, touchDevice);
        }

    }
}
