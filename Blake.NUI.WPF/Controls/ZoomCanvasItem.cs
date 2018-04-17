using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Blake.NUI.WPF.Controls
{
    public class ZoomCanvasItem : ContentControl
    {
        /// <summary>
        /// The Center attached property's name.
        /// </summary>
        public const string CenterPropertyName = "Center";

        /// <summary>
        /// Gets the value of the Center attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the Center property of the specified object.</returns>
        public static Point GetCenter(DependencyObject obj)
        {
            return (Point)obj.GetValue(CenterProperty);
        }

        /// <summary>
        /// Sets the value of the Center attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the Center value of the specified object.</param>
        public static void SetCenter(DependencyObject obj, Point value)
        {
            obj.SetValue(CenterProperty, value);
        }

        /// <summary>
        /// Identifies the Center attached property.
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.RegisterAttached(
            CenterPropertyName,
            typeof(Point),
            typeof(ZoomCanvasItem),
            new UIPropertyMetadata(new Point(double.NaN, double.NaN)));
    }
}
