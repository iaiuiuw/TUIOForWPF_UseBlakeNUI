using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Interop;
using Blake.NUI.WPF.Utility;
using Blake.NUI.WPF.Touch.Interop;

namespace Blake.NUI.WPF.Touch
{
    public class NativeTouchDevice : TouchDevice
    {
        #region Static

        #region Static Fields

        private static Dictionary<int, NativeTouchDevice> deviceDictionary = new Dictionary<int, NativeTouchDevice>();

        #endregion

        #region Public Static Methods

        public static void RegisterEvents(FrameworkElement root)
        {
            Window window = VisualUtility.FindVisualParent<Window>(root);

            if (window == null)
            {
                throw new ArgumentException("Cannot register events without a window in the visual tree");
            }

            TouchHandler handler = new TouchHandler(window);

            handler.TouchDown += TouchDown;
            handler.TouchMove += TouchMove;
            handler.TouchUp += TouchUp;

        }

        #endregion

        #region Private Static Methods

        static void TouchDown(object sender, InteropTouchEventArgs e)
        {
            NativeTouchDevice device = null;
            if (!deviceDictionary.Keys.Contains(e.Id))
            {
                device = new NativeTouchDevice(e);
                deviceDictionary.Add(e.Id, device);
            }

            if (device != null)
            {
                device.TouchDown(e);
            }
        }

        static void TouchMove(object sender, InteropTouchEventArgs e)
        {
            int id = e.Id;
            if (!deviceDictionary.Keys.Contains(id))
            {
                TouchDown(sender, e);
            }

            NativeTouchDevice device = deviceDictionary[id];
            if (device != null)
            {
                device.TouchMove(e);
            }
        }

        static void TouchUp(object sender, InteropTouchEventArgs e)
        {
            int id = e.Id;
            if (!deviceDictionary.Keys.Contains(id))
            {
                TouchDown(sender, e);
            }
            NativeTouchDevice device = deviceDictionary[id];

            if (device != null)
            {
                device.TouchUp(e);
            }

            deviceDictionary.Remove(id);

        }

        #endregion

        #endregion

        #region Class

        #region EventType enum

        private enum EventType
        {
            None,
            TouchDown,
            TouchMove,
            TouchMoveIntermediate,
            TouchUp
        }

        #endregion

        #region Class Fields

        private InteropTouchEventArgs lastEventArgs;
        private List<InteropTouchEventArgs> intermediateEvents = new List<InteropTouchEventArgs>();

        private Point lastEventPosition;
        private DateTime lastEventTime;
        private EventType lastEventType = EventType.None;

        private double movementThreshold = 1;
        private double timeThreshold = 5;

        #endregion

        #region Constructors

        public NativeTouchDevice(InteropTouchEventArgs e) :
            base(e.Id)
        {
            lastEventArgs = e;
        }

        #endregion

        #region Overridden methods

        public override TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
        {
            TouchPointCollection collection = new TouchPointCollection();
            UIElement element = relativeTo as UIElement;

            if (element == null)
                return collection;

            foreach (InteropTouchEventArgs e in intermediateEvents)
            {
                Point point = new Point(e.Location.X, e.Location.Y);
                if (relativeTo != null)
                {
                    point = this.ActiveSource.RootVisual.TransformToDescendant((Visual)relativeTo).Transform(point);
                }

                Rect rect = e.BoundingRect;

                TouchAction action = TouchAction.Move;
                if (e.IsTouchDown)
                {
                    action = TouchAction.Down;
                }
                else if (e.IsTouchUp)
                {
                    action = TouchAction.Up;
                }
                collection.Add(new TouchPoint(this, point, rect, action));
            }
            return collection;
        }

        public override TouchPoint GetTouchPoint(IInputElement relativeTo)
        {
            Point point = new Point(lastEventArgs.Location.X, lastEventArgs.Location.Y);
            if (relativeTo != null)
            {
                point = this.ActiveSource.RootVisual.TransformToDescendant((Visual)relativeTo).Transform(point);
            }

            Rect rect = lastEventArgs.BoundingRect;
            if (rect.Width == 1 && rect.Height == 1)
            {
                double touchSize = 20;
                rect = new Rect(rect.Left - touchSize / 2,
                                rect.Top - touchSize / 2,
                                touchSize,
                                touchSize);
            }

            TouchAction action = TouchAction.Move;
            if (lastEventArgs.IsTouchDown)
            {
                action = TouchAction.Down;
            }
            else if (lastEventArgs.IsTouchUp)
            {
                action = TouchAction.Up;
            }
            return new TouchPoint(this, point, rect, action);
        }

        #endregion

        #region Private Methods

        private void TouchDown(InteropTouchEventArgs e)
        {
            if (lastEventType != EventType.TouchMoveIntermediate)
            {
                intermediateEvents.Clear();
            }

            this.lastEventArgs = e;
            lastEventPosition = e.Location;
            lastEventTime = DateTime.Now;
            lastEventType = EventType.TouchDown;

            this.SetActiveSource(e.ActiveSource);
            this.Activate();
            this.ReportDown();

        }

        private void TouchMove(InteropTouchEventArgs e)
        {
            if (!this.IsActive)
            {
                return;
            }

            CoalesceEvents(e);
        }

        private void CoalesceEvents(InteropTouchEventArgs e)
        {
            TimeSpan span = DateTime.Now - lastEventTime;
            Vector delta = e.Location - lastEventPosition;

            if (lastEventType != EventType.TouchMoveIntermediate)
            {
                intermediateEvents.Clear();
            }

            if (span.TotalMilliseconds < timeThreshold ||
                Math.Ceiling(delta.Length) < movementThreshold)
            {
                intermediateEvents.Add(e);
                lastEventType = EventType.TouchMoveIntermediate;
                //Debug.WriteLine("Event NO: " + touch.TouchDevice.Id + " " + point.Position.ToString() + " " + touch.Timestamp.ToString());
            }
            else
            {
                //Debug.WriteLine("Event go: " + touch.TouchDevice.Id + " " + point.Position.ToString() + " " + touch.Timestamp.ToString());

                lastEventPosition = e.Location;
                lastEventTime = DateTime.Now;
                lastEventType = EventType.TouchMove;

                this.lastEventArgs = e;
                this.ReportMove();

            }
        }

        private void TouchUp(InteropTouchEventArgs e)
        {
            if (lastEventType != EventType.TouchMoveIntermediate)
            {
                intermediateEvents.Clear();
            }

            if (!this.IsActive)
            {
                return;
            }

            this.lastEventArgs = e;

            this.ReportUp();
            this.Deactivate();
            lastEventType = EventType.TouchUp;

        }

        #endregion

        #endregion
    }
}
