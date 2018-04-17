using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Blake.NUI.WPF.Common;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;
using Blake.NUI.WPF.Utility;
using System.ComponentModel;

namespace Blake.NUI.WPF.Controls
{
    public class ZoomCanvas : ContentControl, INotifyPropertyChanged
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

        DispatcherTimer updateTimer;
        bool isZoomUpdateNecessary = true;

        #endregion

        #region Template Parts
        
        FrameworkElement zoomElement;
        string zoomElementName = "PART_ZoomElement";

        FrameworkElement manipulationElement;
        string manipulationElementName = "PART_ManipulationElement";

        #endregion
        
        #region Properties

        #region ZoomMatrix

        public const string ZoomMatrixPropertyName = "ZoomMatrix";

        private DisplayMatrix _zoomMatrix;
        public DisplayMatrix ZoomMatrix
        {
            get
            {
                if (_zoomMatrix == null)
                {
                    ZoomMatrix = new DisplayMatrix();
                    ZoomMatrix.Center = new Point(0, 0);
                }

                return _zoomMatrix;
            }
            set
            {
                if (_zoomMatrix == value)
                    return;

                if (_zoomMatrix != null)
                {
                    ZoomMatrix.ViewChanged -= CurrentView_ViewChanged;
                }
                
                _zoomMatrix = value;

                if (_zoomMatrix != null)
                {
                    ZoomMatrix.ViewChanged += CurrentView_ViewChanged;
                }

                isZoomUpdateNecessary = true;
                RaisePropertyChanged(ZoomMatrixPropertyName);
            }
        }
        #endregion

        #region Center
        /// <summary>
        /// The <see cref="Center" /> property's name.
        /// </summary>
        public const string CenterPropertyName = "Center";
        
        /// <summary>
        /// Gets the Center property.
        /// </summary>
        public Point Center
        {
            get
            {
                if (ZoomMatrix == null)
                    return new Point(0, 0);
                return ZoomMatrix.Center;
            }

            set
            {
                if (ZoomMatrix == null)
                    return;
                if (ZoomMatrix.Center == value)
                {
                    return;
                }

                ZoomMatrix.Center = value;

                RaisePropertyChanged(CenterPropertyName);
            }
        }

        #endregion

        public bool IsRotateEnabled { get; set; }

        public Matrix ViewMatrix
        {
            get
            {
                return ZoomMatrix.TransformMatrix;
            }
        }

        public Vector ViewScale
        {
            get
            {
                return GetScaleFromMatrix(ZoomMatrix.TransformMatrix);
            }
        }

        public Matrix ViewMatrixInvert
        {
            get
            {
                Matrix ret = ZoomMatrix.TransformMatrix;
                if (!ret.HasInverse)
                {
                    Debug.Write("Matrix has no inverse");
                    return Matrix.Identity;
                }
                ret.Invert();
                return ret;
            }
        }
        
        public Vector ViewScaleInvert
        {
            get
            {
                return GetScaleFromMatrix(ViewMatrixInvert);
            }
        }

        #endregion

        #region Dependency Properties
        
        #region IsLocked DP

        /// <summary>
        /// The <see cref="IsLocked" /> dependency property's name.
        /// </summary>
        public const string IsLockedPropertyName = "IsLocked";

        /// <summary>
        /// Gets or sets the value of the <see cref="IsLocked" />
        /// property. This is a dependency property.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return (bool)GetValue(IsLockedProperty);
            }
            set
            {
                SetValue(IsLockedProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="IsLocked" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register(
            IsLockedPropertyName,
            typeof(bool),
            typeof(ZoomCanvas),
            new UIPropertyMetadata(false, new PropertyChangedCallback(IsLockedPropertyChanged)));

        private static void IsLockedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ZoomCanvas canvas = obj as ZoomCanvas;

            if (canvas == null)
                return;

            canvas.UpdateIsLocked();
        }

        #endregion

        #region InputAdapter DP
        /// <summary>
        /// The <see cref="InputAdapter" /> dependency property's name.
        /// </summary>
        public const string InputAdapterPropertyName = "InputAdapter";

        /// <summary>
        /// Gets or sets the value of the <see cref="InputAdapter" />
        /// property. This is a dependency property.
        /// </summary>
        public IZoomCanvasInputAdapter InputAdapter
        {
            get
            {
                return (IZoomCanvasInputAdapter)GetValue(InputAdapterProperty);
            }
            set
            {
                SetValue(InputAdapterProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="InputAdapter" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty InputAdapterProperty = DependencyProperty.Register(
            InputAdapterPropertyName,
            typeof(IZoomCanvasInputAdapter),
            typeof(ZoomCanvas),
            new UIPropertyMetadata(null, new PropertyChangedCallback(InputAdapterPropertyChanged)));

        private static void InputAdapterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ZoomCanvas canvas = obj as ZoomCanvas;

            if (canvas == null)
                return;

            IZoomCanvasInputAdapter newAdapter = e.NewValue as IZoomCanvasInputAdapter;
            IZoomCanvasInputAdapter oldAdapter = e.OldValue as IZoomCanvasInputAdapter;

            if (oldAdapter != null)
            {
                oldAdapter.SetState -= canvas.ChildSetState;
                oldAdapter.ManipulationDelta -= canvas.ChildManipulationDelta;
                oldAdapter.ManipulationComplete -= canvas.ChildManipulationComplete;
            }

            if (newAdapter != null)
            {
                newAdapter.SetState += canvas.ChildSetState;
                newAdapter.ManipulationDelta += canvas.ChildManipulationDelta;
                newAdapter.ManipulationComplete += canvas.ChildManipulationComplete;
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler ViewChanged;

        private void OnViewChanged()
        {
            if (ViewChanged == null)
                return;

            ViewChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Constructors

        static ZoomCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomCanvas), new FrameworkPropertyMetadata(typeof(ZoomCanvas)));
        }

        public ZoomCanvas()
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(15);
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Start();
                        
            IsRotateEnabled = true;
            UpdateIsLocked();

            this.Loaded += new RoutedEventHandler(ZoomCanvas_Loaded);
        }
        
        #endregion
        
        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.zoomElement = this.GetTemplateChild(zoomElementName) as FrameworkElement;

            if (this.zoomElement != null)
            {
                UpdateIsLocked();
            }

            this.manipulationElement = this.GetTemplateChild(manipulationElementName) as FrameworkElement;

            if (this.InputAdapter != null)
            {
                this.InputAdapter.RegisterZoomCanvas(this, manipulationElement);
            }
        }
        
        #endregion

        #region Public Matrix Helpers

        public Point WorldToScreen(Point p)
        {
            return ViewMatrix.Transform(p);
        }

        public Vector WorldToScreen(Vector v)
        {
            return ViewMatrix.Transform(v);
        }

        public Point ScreenToWorld(Point p)
        {
            return ViewMatrixInvert.Transform(p);
        }

        public Vector ScreenToWorld(Vector v)
        {
            return ViewMatrixInvert.Transform(v);
        }

        public static Vector GetScaleFromMatrix(Matrix matrix)
        {
            Vector ret = new Vector();

            ret.X = new Vector(matrix.M11, matrix.M12).Length;
            ret.Y = new Vector(matrix.M21, matrix.M22).Length;

            return ret;
        }

        #endregion

        #region Public Methods

        public void FlyTo(Rect rect, double newOrientation, TimeSpan delta)
        {
            StopFlyTo();
            Rect currentRect = DisplayMatrix.MatrixToBounds(this.ZoomMatrix, this.RenderSize);
            DisplayMatrix temp = DisplayMatrix.BoundsToMatrix(rect, this.RenderSize);
            
            Point newCenter = (rect.TopLeft + new Vector(rect.Width / 2, rect.Height / 2));
            newCenter = new Point(-newCenter.X, -newCenter.Y);
            double scaleX = currentRect.Size.Width / rect.Size.Width;
            double scaleY = currentRect.Size.Height / rect.Size.Height;
            
            Vector newScale = this.ZoomMatrix.Scale * Math.Min(scaleX, scaleY);
            newScale = new Vector(1/temp.Scale.X, 1/temp.Scale.Y);
            AnimateUtility.AnimateElementPoint(this.ZoomMatrix, DisplayMatrix.CenterProperty, newCenter, 0, delta.TotalSeconds);
            AnimateUtility.AnimateElementVector(this.ZoomMatrix, DisplayMatrix.ScaleProperty, newScale, 0, delta.TotalSeconds);
            AnimateUtility.AnimateElementDouble(this.ZoomMatrix, DisplayMatrix.OrientationProperty, newOrientation, 0, delta.TotalSeconds);
        }

        #endregion
        
        #region Input Adapter Events

        void ChildSetState(object sender, ZoomCanvasInputSetStateEventArgs e)
        {
            StopFlyTo();
            this.ZoomMatrix.BatchUpdate(e.Center, e.Orientation, e.Scale);
        }

        void ChildManipulationDelta(object sender, ZoomCanvasInputManipulationDeltaEventArgs e)
        {
            StopFlyTo();
            UpdateZoomView(e.TranslationDelta, e.RotationDelta, e.ScaleDelta, e.ManipulationOrigin);
        }

        void ChildManipulationComplete(object sender, ZoomCanvasInputManipulationCompleteEventArgs e)
        {

        }

        #endregion

        #region Private Methods
        
        void ZoomCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.Center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
        }

        #region Matrix Update Methods

        private void UpdateZoomView(Vector deltaTranslation, double deltaRotation, Vector deltaScale, Point manipulationOrigin)
        {
            if (!IsRotateEnabled)
            {
                deltaRotation = 0;
            }

            this.ZoomMatrix.ManipulationDelta(deltaTranslation, deltaRotation, deltaScale, manipulationOrigin);

            UpdateZoomElement();
        }
        
        private void UpdateZoomElement()
        {
            if (zoomElement == null)
                return;
            isZoomUpdateNecessary = false;

            ScatterMatrixHelper.UpdateScatterMatrix(zoomElement, zoomElement.RenderSize, ZoomMatrix);
            OnViewChanged();
            Debug.WriteLine("ZoomElement width: " + zoomElement.ActualWidth + " height: " + zoomElement.ActualHeight);
        }

        #endregion

        private void StopFlyTo()
        {
            AnimateUtility.StopAnimation(this.ZoomMatrix, DisplayMatrix.CenterProperty);
            AnimateUtility.StopAnimation(this.ZoomMatrix, DisplayMatrix.ScaleProperty);
            AnimateUtility.StopAnimation(this.ZoomMatrix, DisplayMatrix.OrientationProperty);
        }

        private void CurrentView_ViewChanged(object sender, EventArgs e)
        {
            isZoomUpdateNecessary = true;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (isZoomUpdateNecessary)
            {
                UpdateZoomElement();
            }
        }
        
        private void UpdateIsLocked()
        {
            if (zoomElement == null)
                return;

            if (IsLocked)
            {
                zoomElement.IsHitTestVisible = false;
            }
            else
            {
                zoomElement.IsHitTestVisible = true;
            }
        }

        #endregion
    }
}
