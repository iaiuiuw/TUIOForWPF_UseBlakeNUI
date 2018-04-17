using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using TUIO;
using System.Configuration;

namespace Blake.NUI.WPF.Touch
{
    public class TUIOTouchDevice : TouchDevice
    {
        #region Class Members
        private static Dictionary<int, TUIOTouchDevice> deviceDictionary = new Dictionary<int, TUIOTouchDevice>();
        internal static List<TUIOINFO> tuioInfoCache = new List<TUIOINFO>();
        static TUIOMethods tm = new TUIOMethods();

        public static bool isSwapXY = false;
        public static bool isFlipX = false;
        public static bool isFlipY = false;

        private static TuioClient client;


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

        static Window _root;

        #region Public Static Methods

        public static void RegisterEvents(Window root)
        {
            _root = root;
            try
            {
                isSwapXY = Convert.ToBoolean(ConfigurationManager.AppSettings["TUIO_isSwapXY"]);
                isFlipX = Convert.ToBoolean(ConfigurationManager.AppSettings["TUIO_isFlipX"]);
                isFlipY = Convert.ToBoolean(ConfigurationManager.AppSettings["TUIO_isFlipY"]);
            }
            catch
            {
            }
 
                client = new TuioClient(3333);
                client.addTuioListener(tm);
                client.connect();
                root.Closing += new System.ComponentModel.CancelEventHandler(root_Closing);
                CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            while (tuioInfoCache.Count > 0)
            {
                TUIOINFO tuioinfo = tuioInfoCache[0];
                tuioInfoCache.RemoveAt(0);
                switch (tuioinfo.action)
                {
                    case TUIOAction.Add:
                        addTuioCursor(tuioinfo.cursor);
                        break;
                    case TUIOAction.Move:
                        updateTuioCursor(tuioinfo.cursor);
                        break;
                    case TUIOAction.Up:
                        removeTuioCursor(tuioinfo.cursor);
                        break;
                }
            }
        }

        static void root_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
            {
                client.removeTuioListener(tm);
                client.disconnect();
                System.Environment.Exit(0);
            }
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }



        #endregion

        #region Private Static Methods

       

        #endregion







        public static void addTuioCursor(TuioCursor c)
        {
            TUIOTouchDevice device = null;
            if (!deviceDictionary.Keys.Contains(c.getCursorID()))
            {
                device = new TUIOTouchDevice(c.getCursorID());
                deviceDictionary.Add(c.getCursorID(), device);
            }

            if (device != null)
            {

                device.OriginalPosition = new Point(c.getX(), c.getY());

                device.SetActiveSource(PresentationSource.FromVisual(_root));
                device.Activate();
                device.ReportDown();
            }
        }

        public static void updateTuioCursor(TuioCursor c)
        {
            int id = c.getCursorID();
            if (!deviceDictionary.Keys.Contains(id))
            {
                addTuioCursor(c);
            }
            TUIOTouchDevice device = deviceDictionary[id];
            if (device != null)
            {
                device.OriginalPosition = new Point(c.getX(), c.getY());
                device.ReportMove();
            }
        }

        public static void removeTuioCursor(TuioCursor c)
        {
            int id = c.getCursorID();
            if (!deviceDictionary.Keys.Contains(id))
            {
                addTuioCursor(c);
            }
            TUIOTouchDevice device = deviceDictionary[id];
            if (device != null)
            {
                device.OriginalPosition = new Point(c.getX(), c.getY());
                device.ReportUp();
                device.Deactivate();
                device = null;
            }
            deviceDictionary.Remove(id);
        }

        

        #region Constructors

        public TUIOTouchDevice(int deviceId) :
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

    public enum TUIOAction{
        Add,
        Move,
        Up
    }

    public class TUIOINFO
    {
        public TUIOAction action;
        public TuioCursor cursor;
    }

    class TUIOMethods : TuioListener
    {


         #region TUIO Touch Event Handler
        public void addTuioObject(TuioObject o)
        {

        }

        public void updateTuioObject(TuioObject o)
        {

        }

        public void removeTuioObject(TuioObject o)
        {

        }

        public void addTuioCursor(TuioCursor c)
        {
            TUIOTouchDevice.tuioInfoCache.Add(new TUIOINFO() { action = TUIOAction.Add, cursor = c });
        }

        public void updateTuioCursor(TuioCursor c)
        {
            TUIOTouchDevice.tuioInfoCache.Add(new TUIOINFO() { action = TUIOAction.Move, cursor = c });
        }

        public void removeTuioCursor(TuioCursor c)
        {
            TUIOTouchDevice.tuioInfoCache.Add(new TUIOINFO() { action = TUIOAction.Up, cursor = c });
        }

        public void refresh(TuioTime frameTime)
        {

        }

        public void addTuioBlob(TuioBlob tblb)
        {
        }

        public void updateTuioBlob(TuioBlob tblb)
        {
        }

        public void removeTuioBlob(TuioBlob tblb)
        {
        }

        #endregion
    }
}
