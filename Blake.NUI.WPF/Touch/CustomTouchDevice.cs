using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace Blake.NUI.WPF.Touch
{
    public class CustomTouchDevice:TouchDevice
    {

        #region Class Members
        public static Dictionary<int, CustomTouchDevice> deviceDictionary = new Dictionary<int, CustomTouchDevice>();
     

        public static bool isSwapXY = false;
        public static bool isFlipX = false;
        public static bool isFlipY = false;



        public Point OriginalPosition { get; set; }
        public Point Position
        {
            get
            {
                double x = OriginalPosition.X, y= OriginalPosition.Y;
                if (isFlipX)
                {
                    x = 1 - x;
                }
                if (isFlipY)
                {
                    y = 1 - y;
                }
                if (isSwapXY)
                {
                    return new Point(y * _root.ActualWidth, x * _root.ActualHeight);
                }
                else
                {
                    return new Point(x * _root.ActualWidth, y * _root.ActualHeight);

                }
            }
        }

        #endregion

        static FrameworkElement _root;

        #region Public Static Methods

        public static void RegisterEvents(FrameworkElement root)
        {
            _root = root;
            
        }



        #endregion

        #region Private Static Methods

       

        #endregion



        public static void AddTouch(int id,double x,double y)
        {
            CustomTouchDevice device = null;
            if (!deviceDictionary.Keys.Contains(id))
            {
                device = new CustomTouchDevice(id);
                deviceDictionary.Add(id, device);
            }
            else
            {
                //device = deviceDictionary[id];
                return;
            }

            if (device != null)
            {

                device.OriginalPosition = new Point(x, y);
                device.SetActiveSource(PresentationSource.FromVisual(_root));
                device.Activate();
                device.ReportDown();
            }
        }

        public static void UpdateTouch(int id, double x, double y)
        {
            if (!deviceDictionary.Keys.Contains(id))
            {
                return;
                //AddTouch(id, x, y);
            }
            CustomTouchDevice device = deviceDictionary[id];
            if (device != null)
            {
                device.OriginalPosition = new Point(x, y);
                device.ReportMove();
            }
        }

        public static void RemoveTouch(int id, double? x = null, double? y = null)
        {
            if (!deviceDictionary.Keys.Contains(id))
            {
                return;
            }
            CustomTouchDevice device = deviceDictionary[id];
            if (device != null)
            {
                if (x != null && y != null)
                {
                    device.OriginalPosition = new Point(x.Value, y.Value);
                }
                device.ReportUp();
                device.Deactivate();
                device = null;
            }
            deviceDictionary.Remove(id);
        }

        

        #region Constructors

        public CustomTouchDevice(int deviceId) :
            base(deviceId)
        {
            OriginalPosition = new Point();

        }

        #endregion

        #region Overridden methods

        public override TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
        {
            return new TouchPointCollection();
        }

        public override TouchPoint GetTouchPoint(IInputElement relativeTo)
        {
            Point point = Position;
            if (relativeTo != null)
            {
                point = this.ActiveSource.RootVisual.TransformToDescendant((Visual)relativeTo).Transform(Position);
            }

            Rect rect = new Rect(point, new Size(1, 1));

            return new TouchPoint(this, point, rect, TouchAction.Move);
        }

        #endregion

    }
    
}
