using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Interactivity;
using Blake.NUI.WPF.Utility;
using Blake.NUI.WPF.Gestures;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Blake.NUI.WPF.Controls
{
    /// <summary>
    /// Interaction logic for CircleText.xaml
    /// </summary>
    public partial class CircleText : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Fields

        double totalWidth = 0;
        bool isInUpdate = false;

        #endregion

        #region Properties

        #region Text DP
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CircleText),
            new PropertyMetadata("", new PropertyChangedCallback(OnTextPropertyChanged)));

        static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region Repeat DP
        public bool Repeat
        {
            get { return (bool)GetValue(RepeatProperty); }
            set { SetValue(RepeatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatProperty =
            DependencyProperty.Register("Repeat", typeof(bool), typeof(CircleText),
            new PropertyMetadata(false, new PropertyChangedCallback(OnRepeatPropertyChanged)));

        static void OnRepeatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region Foreground DP
        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(CircleText),
           new PropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnForegroundPropertyChanged)));

        static void OnForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region IsInverted DP
        public bool IsInverted
        {
            get { return (bool)GetValue(IsInvertedProperty); }
            set { SetValue(IsInvertedProperty, value); }
        }

        public static readonly DependencyProperty IsInvertedProperty =
            DependencyProperty.Register("IsInverted", typeof(bool), typeof(CircleText),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsInvertedPropertyChanged)));

        static void OnIsInvertedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region HorizontalAlignment DP
        public new HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        public static readonly new DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(CircleText),
            new PropertyMetadata(HorizontalAlignment.Left, new PropertyChangedCallback(OnHorizontalAlignmentPropertyChanged)));

        static void OnHorizontalAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region VerticalAlignment DP
        /// <summary>
        /// The <see cref="VerticalAlignment" /> dependency property's name.
        /// </summary>
        public const string VerticalAlignmentPropertyName = "VerticalAlignment";

        /// <summary>
        /// Gets or sets the value of the <see cref="VerticalAlignment" />
        /// property. This is a dependency property.
        /// </summary>
        public new VerticalAlignment VerticalAlignment
        {
            get
            {
                return (VerticalAlignment)GetValue(VerticalAlignmentProperty);
            }
            set
            {
                SetValue(VerticalAlignmentProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalAlignment" /> dependency property.
        /// </summary>
        public static readonly new DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register(
            VerticalAlignmentPropertyName,
            typeof(VerticalAlignment),
            typeof(CircleText),
            new UIPropertyMetadata(VerticalAlignment.Bottom, new PropertyChangedCallback(OnVerticalAlignmentPropertyChanged)));

        static void OnVerticalAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }
        #endregion

        #region Radius DP
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set
            {
                if (value > 0)
                    SetValue(RadiusProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(CircleText),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnRadiusPropertyChanged)));

        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (MathUtility.IsEqualFuzzy((double)e.NewValue, (double)e.OldValue) || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region Thickness DP
        private double _thickness;
        public double Thickness
        {
            get
            {
                return _thickness;
            }
            protected set
            {
                _thickness = value;
                RaisePropertyChanged("Thickness");
            }
        }

        #endregion

        #region SweepAngle DP
        private double _sweepAngle;
        public double SweepAngle
        {
            get
            {
                return _sweepAngle;
            }
            protected set
            {
                if (MathUtility.IsEqualFuzzy(value,_sweepAngle))
                    return;

                double oldAngle = _sweepAngle;
                _sweepAngle = value;
                
                RaisePropertyChanged("SweepAngle");
                OnSweepAngleChanged(oldAngle, _sweepAngle);
            }
        }

        #endregion

        #region ReferenceAngle DP
        public double ReferenceAngle
        {
            get { return (double)GetValue(ReferenceAngleProperty); }
            set { SetValue(ReferenceAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReferenceAngleProperty =
            DependencyProperty.Register("ReferenceAngle", typeof(double), typeof(CircleText),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnReferenceAnglePropertyChanged)));

        static void OnReferenceAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (MathUtility.IsEqualFuzzy((double)e.NewValue, (double)e.OldValue) || e.NewValue == null)
            {
                return;
            }

            circleText.Update();

            circleText.OnReferenceAngleChanged((double)e.OldValue, (double)e.NewValue);
        }
        
        #endregion

        #region FontSize DP
        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set
            {
                if (value > 0)
                    SetValue(FontSizeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(CircleText),
            new PropertyMetadata(24.0, new PropertyChangedCallback(OnFontSizePropertyChanged)));

        static void OnFontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircleText circleText = d as CircleText;

            if (circleText == null)
                return;

            if (MathUtility.IsEqualFuzzy((double)e.NewValue, (double)e.OldValue) || e.NewValue == null)
            {
                return;
            }

            circleText.Update();
        }

        #endregion

        #region TextBackground DP
        /// <summary>
        /// The <see cref="TextBackground" /> dependency property's name.
        /// </summary>
        public const string TextBackgroundPropertyName = "TextBackground";

        /// <summary>
        /// Gets or sets the value of the <see cref="TextBackground" />
        /// property. This is a dependency property.
        /// </summary>
        public Brush TextBackground
        {
            get
            {
                return (Brush)GetValue(TextBackgroundProperty);
            }
            set
            {
                SetValue(TextBackgroundProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="TextBackground" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextBackgroundProperty = DependencyProperty.Register(
            TextBackgroundPropertyName,
            typeof(Brush),
            typeof(CircleText),
            new UIPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        #endregion

        #region FontFamily DP

        /// <summary>
        /// The <see cref="FontFamily" /> dependency property's name.
        /// </summary>
        public const string FontFamilyPropertyName = "FontFamily";

        /// <summary>
        /// Gets or sets the value of the <see cref="FontFamily" />
        /// property. This is a dependency property.
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)GetValue(FontFamilyProperty);
            }
            set
            {
                SetValue(FontFamilyProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="FontFamily" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            FontFamilyPropertyName,
            typeof(FontFamily),
            typeof(CircleText),
            new UIPropertyMetadata(null));

        #endregion

        #endregion

        #region Events

        public event EventHandler Tap;

        private void OnTap()
        {
            if (Tap == null)
                return;

            Tap(this, EventArgs.Empty);
        }

        public event EventHandler<AngleChangedEventArgs> SweepAngleChanged;

        protected void OnSweepAngleChanged(double oldAngle, double newAngle)
        {
            if (SweepAngleChanged == null)
                return;

            SweepAngleChanged(this, new AngleChangedEventArgs(oldAngle, newAngle));

        }

        public event EventHandler<AngleChangedEventArgs> ReferenceAngleChanged;

        protected void OnReferenceAngleChanged(double oldAngle, double newAngle)
        {
            if (ReferenceAngleChanged == null)
                return;

            ReferenceAngleChanged(this, new AngleChangedEventArgs(oldAngle, newAngle));

        }

        #endregion

        #region Constructors
        
        public CircleText()
        {
            InitializeComponent();
            
            TapGestureTrigger tap = new TapGestureTrigger();
            tap.HandlesTouches = true;
            tap.Tap += new EventHandler(tap_Tap);
            
            Interaction.GetTriggers(this).Add(tap);
        }
        
        #endregion

        #region Private Methods
        
        void tap_Tap(object sender, EventArgs e)
        {
            OnTap();
        }

        private void Update()
        {
            if (isInUpdate)
                return;
            
            if (Radius <= 0)
                return;

            isInUpdate = true;


            this.SweepAngle = RenderText(ReferenceAngle, true);

            if (this.Repeat)
            {
                double angle = SweepAngle + 15;

                angle = 360 / ((int)((360.0 - angle) / angle));

                int startChild = 0;

                double current = ReferenceAngle;
                while (current < ReferenceAngle + 360.0)
                {
                    RenderText(current, false, startChild);
                    current += angle;
                    startChild += Text.Length;
                }

                int extraTextBlocks = LayoutRoot.Children.Count - startChild;

                for (int i = 0; i < extraTextBlocks; i++)
                {
                    LayoutRoot.Children.RemoveAt(Text.Length);
                }
                this.SweepAngle = 360.0;
            }
            else
            {
                double startAngle = ReferenceAngle;
                if (HorizontalAlignment == HorizontalAlignment.Center)
                    startAngle -= SweepAngle / 2.0;
                else if (HorizontalAlignment == HorizontalAlignment.Right)
                    startAngle -= SweepAngle;
                
                RenderText(startAngle, false);

                int extraTextBlocks = LayoutRoot.Children.Count - Text.Length;

                for (int i = 0; i < extraTextBlocks; i++)
                {
                    LayoutRoot.Children.RemoveAt(Text.Length);
                }
                UpdateTouchTarget();
            }
            isInUpdate = false;
        }

        private void UpdateTouchTarget()
        {
            Polygon polygon = null;

            if (canvasPolygons.Children.Count > 0)
            {
                polygon = canvasPolygons.Children[0] as Polygon;
            }

            if (polygon == null)
            {
                canvasPolygons.Children.Clear();
                polygon = new Polygon();
                polygon.Fill = TextBackground;
                canvasPolygons.Children.Add(polygon);
            }

            polygon.Points.Clear();
            double padding = 10;
            for (int i = 0; i < LayoutRoot.Children.Count; i++)
            {
                TextBlock currentSegment = LayoutRoot.Children[i] as TextBlock;

                double xOffset = 0;
                if (i == 0)
                    xOffset = -padding;
                if (i == LayoutRoot.Children.Count - 1)
                    xOffset = padding;

                Point topLeft = new Point(xOffset, -padding);
                topLeft = currentSegment.RenderTransform.Transform(topLeft);

                Point topRight = new Point(xOffset + currentSegment.DesiredSize.Width, -padding);
                topRight = currentSegment.RenderTransform.Transform(topRight);

                polygon.Points.Add(topLeft);
                polygon.Points.Add(topRight);
            }                

            for (int i = LayoutRoot.Children.Count - 1; i >= 0; i--)
            {
                TextBlock currentSegment = LayoutRoot.Children[i] as TextBlock;

                double xOffset = 0;
                if (i == 0)
                    xOffset = -padding;
                if (i == LayoutRoot.Children.Count - 1)
                    xOffset = padding;

                Point bottomRight = new Point(xOffset + currentSegment.DesiredSize.Width, padding + currentSegment.DesiredSize.Height);
                bottomRight = currentSegment.RenderTransform.Transform(bottomRight);

                Point bottomLeft = new Point(xOffset, padding + currentSegment.DesiredSize.Height);
                bottomLeft = currentSegment.RenderTransform.Transform(bottomLeft);
                
                polygon.Points.Add(bottomRight);
                polygon.Points.Add(bottomLeft);
            }

        }

        private double RenderText(double initialAngle, bool measureOnly, int startChild = 0)
        {
            double currentAngle = initialAngle;
            totalWidth = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                TextBlock textSegment = null;

                if (startChild + i < LayoutRoot.Children.Count)
                    textSegment = LayoutRoot.Children[startChild + i] as TextBlock;
                
                if (textSegment == null)
                {
                    textSegment = new TextBlock();

                    if (this.FontFamily != null)
                    {
                        textSegment.FontFamily = this.FontFamily;
                    }

                    DropShadowEffect shadow = new DropShadowEffect();
                    shadow.BlurRadius = 6.0;
                    shadow.Color = Colors.White;
                    shadow.ShadowDepth = 0;
                    textSegment.Effect = shadow; 
                    textSegment.FontWeight = FontWeights.Normal;
                    textSegment.MinWidth = 0;
                    TextOptions.SetTextFormattingMode(textSegment, TextFormattingMode.Ideal);
                    TextOptions.SetTextRenderingMode(textSegment, TextRenderingMode.ClearType);
                    TextOptions.SetTextHintingMode(textSegment, TextHintingMode.Auto);
                    
                    LayoutRoot.Children.Add(textSegment);

                    TransformGroup renderGroup = new TransformGroup();

                    RotateTransform rotateCenter = new RotateTransform(0);
                    TranslateTransform translate = new TranslateTransform(0, 0);
                    RotateTransform rotate = new RotateTransform(0);

                    renderGroup.Children.Add(rotateCenter);
                    renderGroup.Children.Add(translate);
                    renderGroup.Children.Add(rotate);

                    textSegment.RenderTransform = renderGroup;
                }
                string character = Text[i].ToString();

                bool dirty = false;
                if (textSegment.Text != character)
                {
                    textSegment.Text = character;
                    dirty = true;
                }

                textSegment.Foreground = this.Foreground;
                if (textSegment.FontSize != this.FontSize)
                {
                    textSegment.FontSize = this.FontSize;
                    dirty = true;
                }

                if (dirty || !textSegment.IsMeasureValid)
                {
                    textSegment.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
                double width = textSegment.DesiredSize.Width;
                double height = textSegment.DesiredSize.Height;
                totalWidth += width;
                this.Thickness = height;

                double effectiveRadius = Radius;

                if (IsInverted)
                {
                    effectiveRadius = -(Radius + height);
                }
                double alignedRadius = Radius;

                if (VerticalAlignment == System.Windows.VerticalAlignment.Top)
                {
                    alignedRadius -= height;

                    if (IsInverted)
                        effectiveRadius += height;
                    else
                        effectiveRadius -= height;
                }

                double deltaAngle = Math.Atan2(width, Math.Abs(alignedRadius)) * 180.0 / Math.PI;

                if (measureOnly == false)
                {
                    TransformGroup renderGroup = textSegment.RenderTransform as TransformGroup;
                    
                    RotateTransform rotateCenter = renderGroup.Children[0] as RotateTransform;
                    double deltaAngle2 = deltaAngle / 2.0;
                    if (rotateCenter.Angle != deltaAngle2)
                        rotateCenter.Angle = deltaAngle2;
                    rotateCenter.CenterX = width / 2;
                    rotateCenter.CenterY = height;

                    TranslateTransform translate = renderGroup.Children[1] as TranslateTransform;
                    translate.Y = effectiveRadius;

                    RotateTransform rotate = renderGroup.Children[2] as RotateTransform;
                    rotate.Angle = currentAngle;

                }
                if (IsInverted)
                    currentAngle += deltaAngle;
                else
                    currentAngle -= deltaAngle;
            }

            return Math.Abs(currentAngle - initialAngle);
        }

        #endregion
    }
}
