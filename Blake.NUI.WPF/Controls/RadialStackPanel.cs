using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InfoStrat.VE.NUI2.Utilities
{

    public class RadialStackPanel : Canvas
    {
        #region Class Members

        bool innerRadiusSet;
        bool outerRadiusSet;

        #endregion

        #region Angle DP

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(RadialStackPanel), new UIPropertyMetadata(0.0));
        
        #endregion

        #region Inner Radius DP

        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set
            {
                innerRadiusSet = true;
                SetValue(InnerRadiusProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.RegisterAttached("InnerRadius", typeof(double), typeof(RadialStackPanel),
            new FrameworkPropertyMetadata(0.0,
                                          FrameworkPropertyMetadataOptions.Inherits |
                                          FrameworkPropertyMetadataOptions.AffectsArrange,
                                          new PropertyChangedCallback(OnInnerRadiusPropertyChanged)));

        public static void SetInnerRadius(DependencyObject element, double value)
        {
            element.SetValue(InnerRadiusProperty, value);
        }

        public static double GetInnerRadius(DependencyObject element)
        {
            return (double)element.GetValue(InnerRadiusProperty);
        }

        static void OnInnerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialStackPanel panel = d as RadialStackPanel;

            if (panel == null)
                return;

            if (e.NewValue == null)
            {
                panel.innerRadiusSet = false;
                return;
            }

            panel.innerRadiusSet = true;
        }

        #endregion

        #region Outer Radius DP
        
        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set
            {
                outerRadiusSet = true;
                SetValue(OuterRadiusProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for OuterRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.RegisterAttached("OuterRadius", typeof(double), typeof(RadialStackPanel),
            new FrameworkPropertyMetadata(0.0, 
                                          FrameworkPropertyMetadataOptions.Inherits | 
                                          FrameworkPropertyMetadataOptions.AffectsArrange,
                                          new PropertyChangedCallback(OnOuterRadiusPropertyChanged)));

        public static void SetOuterRadius(DependencyObject element, double value)
        {
            element.SetValue(OuterRadiusProperty, value);
        }

        public static double GetOuterRadius(DependencyObject element)
        {
            return (double)element.GetValue(OuterRadiusProperty);
        }

        static void OnOuterRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialStackPanel panel = d as RadialStackPanel;

            if (panel == null)
                return;

            if (e.NewValue == null)
            {
                panel.outerRadiusSet = false;
                return;
            }

            panel.outerRadiusSet = true;
        }

        #endregion

        #region TranslateRadial DP

        public bool TranslateRadial
        {
            get { return (bool)GetValue(TranslateRadialProperty); }
            set { SetValue(TranslateRadialProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TranslateRadial.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TranslateRadialProperty =
            DependencyProperty.Register("TranslateRadial", typeof(bool), typeof(RadialStackPanel), new UIPropertyMetadata(true));
        
        #endregion

        #region Orientation DP
        
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(RadialStackPanel), new UIPropertyMetadata(Orientation.Horizontal));
        
        #endregion

        #region HorizontalContentAlignment DP
        
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalContentAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(RadialStackPanel), 
            new UIPropertyMetadata(HorizontalAlignment.Center));
        
        #endregion

        #region VerticalContentAlignment DP

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalContentAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(RadialStackPanel),
            new UIPropertyMetadata(VerticalAlignment.Center));

        #endregion

        public RadialStackPanel()
        {
            innerRadiusSet = false;
            outerRadiusSet = false;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.Children.Count == 0 ||
                (innerRadiusSet == false && outerRadiusSet == false))
                return base.ArrangeOverride(arrangeSize);
            
            List<Size> elementSizes = new List<Size>();

            for (int i = 0; i < this.Children.Count; i++)
            {
                UIElement element = this.Children[i];

                if (this.Orientation == Orientation.Vertical &&
                    element is FrameworkElement)
                {
                    FrameworkElement fe = element as FrameworkElement;
                    
                    fe.LayoutTransform = new RotateTransform(-90);
                }

                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                elementSizes.Add(element.DesiredSize);

               
            }

            double totalArcAngle = 0;

            for (int i = 0; i < this.Children.Count - 1; i++)
            {
                double radius = GetElementRadius(elementSizes[i]);

                double avgHeight = (elementSizes[i].Height + elementSizes[i + 1].Height) / 2;

                totalArcAngle += Math.Atan2(avgHeight, radius) * 180.0 / Math.PI;
            }

            double currentAngle = this.Angle;

            if (this.VerticalContentAlignment == VerticalAlignment.Top)
            {
                currentAngle = this.Angle;
            }
            else if (this.VerticalContentAlignment == VerticalAlignment.Bottom)
            {
                currentAngle = this.Angle - totalArcAngle;
            }
            else if (this.VerticalContentAlignment == VerticalAlignment.Center ||
                    this.VerticalContentAlignment == VerticalAlignment.Stretch)
            {
                currentAngle = this.Angle - totalArcAngle / 2;
            }

            Point currentPosition = new Point(0, 0);


            for (int i = 0; i < this.Children.Count; i++)
            {
                UIElement element = this.Children[i];

                double radius = GetElementRadius(elementSizes[i]);

                TranslateTransform radial = new TranslateTransform(radius, -elementSizes[i].Height / 2);
                
                RotateTransform rotate = new RotateTransform(currentAngle);

                TranslateTransform translate = new TranslateTransform(currentPosition.X, currentPosition.Y - elementSizes[i].Height / 2);
                
                TransformGroup group = new TransformGroup();

                if (TranslateRadial)
                {
                    group.Children.Add(radial);
                }
                group.Children.Add(rotate);
                if (!TranslateRadial)
                {
                    group.Children.Add(translate);
                }
                                
                element.RenderTransform = group;

                currentPosition.X -= elementSizes[i].Height * Math.Sin(currentAngle * Math.PI / 180.0);
                currentPosition.Y += elementSizes[i].Height * Math.Cos(currentAngle * Math.PI / 180.0);
                
                if (radius != 0)
                {
                    if (i < this.Children.Count - 1)
                    {
                        double avgHeight = (elementSizes[i].Height + elementSizes[i + 1].Height) / 2;

                        currentAngle += Math.Atan2(avgHeight, radius) * 180.0 / Math.PI;
                        
                    }
                }

            }
            
            return base.ArrangeOverride(arrangeSize);
        }

        private double GetElementRadius(Size elementSize)
        {
            double radius = this.InnerRadius;

            switch (this.HorizontalContentAlignment)
            {
                case HorizontalAlignment.Left:
                    if (innerRadiusSet)
                        radius = this.InnerRadius;
                    else if (outerRadiusSet)
                        radius = this.OuterRadius;
                    else
                        radius = 0;
                    break;

                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    if (innerRadiusSet && outerRadiusSet)
                        radius = (this.OuterRadius + this.InnerRadius - elementSize.Width) / 2;
                    else if (innerRadiusSet)
                        radius = this.InnerRadius;
                    else if (outerRadiusSet)
                        radius = this.OuterRadius;
                    else
                        radius = 0;
                    break;

                case HorizontalAlignment.Right:
                    if (outerRadiusSet)
                        radius = this.OuterRadius - elementSize.Width;
                    else if (innerRadiusSet)
                        radius = this.InnerRadius;
                    else
                        radius = 0;
                    break;

            }
            return radius;
        }

    }
}
