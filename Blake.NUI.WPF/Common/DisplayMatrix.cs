using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Blake.NUI.WPF.Utility;
using System.Xml.Serialization;

namespace Blake.NUI.WPF.Common
{
    public class DisplayMatrix : Animatable, INotifyPropertyChanged
    {
        #region Animatable Methods

        protected override Freezable CreateInstanceCore()
        {
            return new DisplayMatrix();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Properties

        #region IsDirty

        private bool _isMatrixDirty = true;

        private bool _isDirty;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                if (_isDirty)
                    _isMatrixDirty = true;
                RaisePropertyChanged("IsDirty");
            }
        }

        #endregion

        #region ZIndex

        private int _zIndex;
        public int ZIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                _zIndex = value;
                OnViewChanged();
                RaisePropertyChanged("ZIndex");
            }
        }

        #endregion

        #region TransformMatrix

        private Matrix _transformMatrix;
        [XmlIgnore]
        public Matrix TransformMatrix
        {
            get
            {
                if (_isMatrixDirty)
                {
                    _transformMatrix = GenerateTransformMatrix(Center, Orientation, Scale);
                    _isMatrixDirty = false;
                }
                return _transformMatrix;
            }
            set
            {
                _transformMatrix = value;
                RaisePropertyChanged("TransformMatrix");
                OnViewChanged();
            }
        }

        #endregion

        #region Center DP

        private Point _center;

        public Point Center
        {
            get { return _center; }
            set
            {
                _center = value;
                OnViewChanged();
                SetValue(CenterProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Center.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point), typeof(DisplayMatrix), new FrameworkPropertyMetadata(new Point(0.0, 0.0), new PropertyChangedCallback(CenterPropertyChanged)));

        public static void CenterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DisplayMatrix data = obj as DisplayMatrix;
            if (data == null)
                return;

            data._center = (Point)e.NewValue;
            data.OnViewChanged();
            //data.UpdateMatrix();
        }
        #endregion

        #region Orientation DP

        private double _orientation;

        public double Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = MathUtility.NormalizeAngle(value);
                OnViewChanged();
                SetValue(OrientationProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(double), typeof(DisplayMatrix), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OrientationPropertyChanged)));


        public static void OrientationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DisplayMatrix data = obj as DisplayMatrix;
            if (data == null)
                return;

            data._orientation = MathUtility.NormalizeAngle((double)e.NewValue);
            data.OnViewChanged();
        }
        #endregion

        #region Scale DP

        private Vector _scale;

        public Vector Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                SetValue(ScaleProperty, value);
                UpdateActualSizeFromScale();
                OnViewChanged();
            }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(Vector), typeof(DisplayMatrix), new FrameworkPropertyMetadata(new Vector(1.0, 1.0), new PropertyChangedCallback(ScalePropertyChanged)));

        public static void ScalePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DisplayMatrix data = obj as DisplayMatrix;
            if (data == null)
                return;

            data._scale = (Vector)e.NewValue;
            data.UpdateActualSizeFromScale();
            data.OnViewChanged();
        }
        #endregion

        #region OriginalSize

        /// <summary>
        /// The <see cref="OriginalSize" /> property's name.
        /// </summary>
        public const string OriginalSizePropertyName = "OriginalSize";

        private Size _originalSize = new Size(1,1);

        /// <summary>
        /// Gets the OriginalSize property.
        /// </summary>
        public Size OriginalSize
        {
            get
            {
                return _originalSize;
            }

            set
            {
                if (_originalSize == value)
                {
                    return;
                }

                var oldValue = _originalSize;
                _originalSize = value;
                UpdateActualSizeFromScale();
                RaisePropertyChanged(OriginalSizePropertyName);
            }
        }

        #endregion

        #region ActualSize

        /// <summary>
        /// The <see cref="ActualSize" /> property's name.
        /// </summary>
        public const string ActualSizePropertyName = "ActualSize";

        private Size _actualSize = new Size(1,1);

        /// <summary>
        /// Gets the ActualSize property.
        /// </summary>
        public Size ActualSize
        {
            get
            {
                return _actualSize;
            }

            set
            {
                if (_actualSize == value)
                {
                    return;
                }

                _actualSize = value;

                UpdateScaleFromActualSize();
                OnViewChanged();

                // Update bindings, no broadcast
                RaisePropertyChanged(ActualSizePropertyName);
            }
        }

        #endregion
        
        #endregion

        #region Constructors

        public DisplayMatrix()
        {
            ZIndex = 100000;

            Center = new Point(0, 0);
            Orientation = 0.0;
            Scale = new Vector(1.0, 1.0);
            UpdateMatrix();
        }

        #endregion

        #region Events

        public event EventHandler ViewChanged;

        internal void OnViewChanged()
        {
            this.IsDirty = true;
            if (ViewChanged != null)
            {
                ViewChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Public Methods

        public void ManipulationDelta(Vector deltaTranslation, double deltaRotation, Vector deltaScale, Point manipulationOrigin)
        {
            Point center = this.Center;

            if (double.IsNaN(center.X))
            {
                center.X = 0;
                this.Center = center;
            }
            if (double.IsNaN(center.Y))
            {
                center.Y = 0;
                this.Center = center;
            }

            Point manipulationStart = manipulationOrigin - deltaTranslation;

            //Get offset of the manipulation
            Vector offsetToCenter = (Vector)(center - manipulationStart);

            //Rotate the offset
            Vector rotatedOffset = MathUtility.RotateVector(offsetToCenter, deltaRotation);
            deltaTranslation += rotatedOffset - offsetToCenter;

            //Scale the offset
            Vector scaledOffsetToCenter = (Vector)(offsetToCenter * deltaScale.X);

            //Update the translation so manipulation remains centered
            deltaTranslation += scaledOffsetToCenter - offsetToCenter;

            Vector oldScale = this.Scale;

            Point newCenter = this.Center + deltaTranslation;
            double newOrientation = this.Orientation + deltaRotation;
            Vector newScale = new Vector(oldScale.X * deltaScale.X,
                                           oldScale.Y * deltaScale.Y);

            this.BatchUpdate(newCenter, newOrientation, newScale);
        }

        public void BatchUpdate(Point center, double orientation, Vector scale)
        {
            _center = center;
            SetValue(CenterProperty, center);

            _orientation = orientation;
            SetValue(OrientationProperty, orientation);

            _scale = scale;
            SetValue(ScaleProperty, scale);
            UpdateActualSizeFromScale();

            UpdateMatrix();
            OnViewChanged();

        }

        public void UpdateMatrix()
        {
            TransformMatrix = GenerateTransformMatrix(Center, Orientation, Scale);
        }

        #endregion

        #region Public Static Methods

        public static Matrix GenerateTransformMatrix(Point center, double orientation, Vector scale)
        {
            //Initialize a blank Matrix
            Matrix matrix = new Matrix();

            //Apply the delta rotation
            matrix.RotateAt(orientation, 0, 0);

            //Apply the delta scale
            matrix.ScaleAt(scale.X, scale.Y, 0, 0);

            //Apply the delta translation
            if (!double.IsNaN(center.X) && !double.IsNaN(center.Y))
            {
                matrix.Translate(center.X, center.Y);
            }

            return matrix;
        }

        public static Rect MatrixToBounds(DisplayMatrix data, Size size)
        {
            Vector sizeVector = new Vector(size.Width, size.Height);

            sizeVector = data.TransformMatrix.Transform(sizeVector);

            Vector radius = new Vector(size.Width / 2, size.Height / 2);
            radius = data.TransformMatrix.Transform(radius);

            Point corner = data.Center - radius;
            return new Rect(corner, sizeVector);
        }

        public static DisplayMatrix BoundsToMatrix(Rect rect, Size defaultSize)
        {
            DisplayMatrix data = new DisplayMatrix();

            //Point corner = rect.TopLeft;
            //Vector sizeVector = new Vector(rect.Size.Width, rect.Size.Height);

            Point center = rect.TopLeft + new Vector(rect.Width / 2, rect.Height / 2);
            
            Vector scale = new Vector();
            if (defaultSize.Width == 0 || defaultSize.Height == 0)
                throw new DivideByZeroException();

            double scaleX = rect.Size.Width / defaultSize.Width;
            double scaleY = rect.Size.Height / defaultSize.Height;
            scale.X = Math.Min(scaleX, scaleY);
            scale.Y = Math.Min(scaleX, scaleY);
            
            //Vector offset = ScatterMatrixHelper.CalculateRenderOffset(defaultSize, new Point(0.5, 0.5), center, 0.0, scale);

            //data.Center = new Point(offset.X, offset.Y);
            data.Scale = scale;
            data.Center = center;
            return data;
        }
        
        #endregion

        #region Private Methods

        private void UpdateActualSizeFromScale()
        {
            _actualSize.Width = OriginalSize.Width * Scale.X;
            _actualSize.Height = OriginalSize.Height * Scale.Y;
        }

        private void UpdateScaleFromActualSize()
        {
            _scale.X = ActualSize.Width / OriginalSize.Width;
            _scale.Y = ActualSize.Height / OriginalSize.Height;
        }

        #endregion
    }
}
