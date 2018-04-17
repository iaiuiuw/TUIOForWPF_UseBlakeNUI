using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Blake.NUI.WPF.Utility
{
    public static class VisualUtility
    {
        public static T FindVisualParent<T>(DependencyObject obj) where T : UIElement
        {
            if (obj is T)
            {
                return obj as T;
            }

            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            if (parent == null)
                return null;
            return FindVisualParent<T>(parent);
        }

        public static DependencyObject GetChildByType(DependencyObject element, Type type)
        {
            if (element == null)
                return null;

            if (type.IsAssignableFrom(element.GetType()))
                return element;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                DependencyObject ret = GetChildByType(VisualTreeHelper.GetChild(element, i), type);
                if (ret != null)
                    return ret;
            }
            return null;
        }

        public static BitmapSource CreateBitmapSourceFromVisual(Double width, Double height, Visual visualToRender, Boolean undoTransformation)
        {
            if (visualToRender == null) 
            { 
                return null; 
            } 
            RenderTargetBitmap bmp = new RenderTargetBitmap((Int32)Math.Ceiling(width), 
                                                            (Int32)Math.Ceiling(height), 
                                                            96, 
                                                            96, 
                                                            PixelFormats.Pbgra32); 
            if (undoTransformation) 
            { 
                DrawingVisual dv = new DrawingVisual(); 
                using (DrawingContext dc = dv.RenderOpen()) 
                { 
                    VisualBrush vb = new VisualBrush(visualToRender); 
                    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height))); 
                } 
                bmp.Render(dv); 
            }
            else 
            { 
                bmp.Render(visualToRender); 
            } 
            return bmp;
        }
    }
}
