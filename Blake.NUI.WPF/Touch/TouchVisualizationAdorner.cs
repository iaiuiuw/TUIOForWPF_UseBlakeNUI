using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Blake.NUI.WPF.Touch
{
    public class TouchVisualizationAdorner : Adorner
    {
        #region Class members

        VisualCollection visualChildren;

        Dictionary<TouchDevice, Ellipse> visualizations = new Dictionary<TouchDevice, Ellipse>();
        UIElement adorningElement;
        Canvas visualizationCanvas;

        #endregion

        #region Static methods

        public static void AddTouchVisualizations(FrameworkElement adorningElement)
        {
            adorningElement = GetContentIfWindow(adorningElement);

            if (!adorningElement.IsLoaded)
            {
                AddTouchVisualizationsOnLoading(adorningElement);
            }
            else
            {
                CreateAndAddAdorner(adorningElement);
            }
        }

        private static FrameworkElement GetContentIfWindow(FrameworkElement adorningElement)
        {
            Window window = adorningElement as Window;
            if (window != null)
            {
                adorningElement = window.Content as FrameworkElement;
            }

            if (adorningElement == null)
            {
                throw new ArgumentNullException("adorningElement");
            }

            return adorningElement;
        }

        private static void AddTouchVisualizationsOnLoading(FrameworkElement adorningElement)
        {
            adorningElement.Loaded += (s, e) =>
                {
                    AddTouchVisualizations(adorningElement);
                };
        }

        private static void CreateAndAddAdorner(FrameworkElement adorningElement)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(adorningElement);
            if (layer == null)
            {
                throw new NullReferenceException("Adorner layer is null");
            }

            TouchVisualizationAdorner visualizationAdorner = new TouchVisualizationAdorner(adorningElement);
            layer.Add(visualizationAdorner);
        }

        #endregion

        #region Constructors

        public TouchVisualizationAdorner(UIElement adorningElement)
            : base(adorningElement)
        {
            visualChildren = new VisualCollection(this);

            this.adorningElement = adorningElement;
            if (this.adorningElement == null)
                throw new ArgumentNullException("adorningElement");

            this.visualizationCanvas = new Canvas();
            this.IsHitTestVisible = false;

            visualChildren.Add(this.visualizationCanvas);

            this.adorningElement.AddHandler(UIElement.TouchEnterEvent, new EventHandler<TouchEventArgs>(TouchEvent), true);
            this.adorningElement.AddHandler(UIElement.PreviewTouchMoveEvent, new EventHandler<TouchEventArgs>(TouchEvent), true);
            this.adorningElement.AddHandler(UIElement.TouchLeaveEvent, new EventHandler<TouchEventArgs>(TouchEvent), true);
        }

        #endregion

        #region Overridden methods

        protected override int VisualChildrenCount
        {
            get
            {
                return visualChildren.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.visualizationCanvas.Measure(constraint);
            return this.visualizationCanvas.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.visualizationCanvas.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion

        #region Touch Events

        private void TouchEvent(object sender, TouchEventArgs e)
        {
            VisualizeTouches(e);
        }

        #endregion

        #region Visualize Touches

        private void VisualizeTouches(TouchEventArgs e)
        {
            Ellipse ellipse;
            bool visualizationExists = visualizations.TryGetValue(e.TouchDevice, out ellipse);

            TouchPoint touch = e.GetTouchPoint(visualizationCanvas);

            if (visualizationExists &&
                (touch.Action == TouchAction.Up ||
                 e.TouchDevice.Target == null))
            {
                if (visualizationCanvas.Children.Contains(ellipse))
                {
                    visualizationCanvas.Children.Remove(ellipse);
                }
                visualizations.Remove(e.TouchDevice);
            }
            else
            {
                if (!visualizationExists)
                {
                    ellipse = new Ellipse();
                    ellipse.Fill = new RadialGradientBrush(Colors.Gray, Colors.Transparent);

                    visualizationCanvas.Children.Add(ellipse);
                    visualizations.Add(e.TouchDevice, ellipse);
                }

                ellipse.Width = touch.Size.Width * 2;
                ellipse.Height = touch.Size.Height * 2;

                Canvas.SetLeft(ellipse, touch.Position.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, touch.Position.Y - ellipse.Height / 2);
            }
        }

        #endregion
    }
}
