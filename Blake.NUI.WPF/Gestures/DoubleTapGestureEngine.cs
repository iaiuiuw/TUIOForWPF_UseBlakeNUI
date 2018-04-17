using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows;

namespace Blake.NUI.WPF.Gestures
{
    public class DoubleTapGestureEngine : IGestureEngine
    {
        #region Class members

        double _maxMilliseconds;
        double _gapMilliseconds;
        double _minMilliseconds;
        double _maxMovement;
        DispatcherTimer _timer;

        TapGestureEngine firstTap;
        TapGestureEngine secondTap;

        #endregion

        #region Properties

        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Vector TranslationDelta
        {
            get
            {
                return EndPoint - StartPoint;
            }
        }

        private DateTime _touchDownTime;
        public DateTime TouchDownTime
        {
            get
            {
                return _touchDownTime;
            }
            private set
            {
                _touchDownTime = value;
            }
        }

        private DateTime _touchUpTime;
        public DateTime TouchUpTime
        {
            get
            {
                return _touchUpTime;
            }
            private set
            {
                _touchUpTime = value;
            }
        }

        public TimeSpan TimeDelta
        {
            get
            {
                if (IsStarted && IsCompleted)
                    return TouchUpTime - TouchDownTime;

                if (IsStarted)
                    return DateTime.Now - TouchDownTime;

                return TimeSpan.FromMilliseconds(0);
            }
        }
        
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsAborted { get; set; }

        #endregion

        #region Events
        
        public event EventHandler GestureStarted;

        private void OnGestureStarted()
        {
            if (GestureStarted != null)
            {
                GestureStarted(this, EventArgs.Empty);
            }
        }

        public event EventHandler GestureCompleted;

        private void OnGestureCompleted()
        {
            if (GestureCompleted != null)
            {
                GestureCompleted(this, EventArgs.Empty);
            }
        }

        public event EventHandler GestureAborted;

        private void OnGestureAborted()
        {
            if (GestureAborted != null)
            {
                GestureAborted(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Constructors

        public DoubleTapGestureEngine(double minMilliseconds, double gapMilliseconds, double maxMilliseconds, double maxMovement)
        {
            IsStarted = false;
            IsCompleted = false;
            IsAborted = false;

            firstTap = new TapGestureEngine(minMilliseconds, maxMilliseconds, maxMovement);
            firstTap.GestureAborted += new EventHandler(firstTap_GestureAborted);
            firstTap.GestureCompleted += new EventHandler(firstTap_GestureCompleted);
            
            secondTap = new TapGestureEngine(minMilliseconds, maxMilliseconds, maxMovement);
            secondTap.GestureAborted += new EventHandler(secondTap_GestureAborted);
            secondTap.GestureCompleted += new EventHandler(secondTap_GestureCompleted);

            _maxMilliseconds = maxMilliseconds;
            _gapMilliseconds = gapMilliseconds;
            _minMilliseconds = minMilliseconds;
            _maxMovement = maxMovement;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_gapMilliseconds);
            _timer.Tick += (s, ee) =>
            {
                AbortGesture();
            };
        }

        #endregion

        
        #region Private Methods

        void firstTap_GestureCompleted(object sender, EventArgs e)
        {
            if (!secondTap.IsStarted)
                _timer.Start();
        }

        void firstTap_GestureAborted(object sender, EventArgs e)
        {
            AbortGesture();
        }

        void secondTap_GestureCompleted(object sender, EventArgs e)
        {
            IsCompleted = true;
            OnGestureCompleted();
        }

        void secondTap_GestureAborted(object sender, EventArgs e)
        {
            AbortGesture();
        }
        
        #endregion

        #region IGestureEngine Members

        public void TrackTouchDown(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            if (!firstTap.IsStarted)
            {
                firstTap.TrackTouchDown(position, timestamp);

                IsStarted = true;
                OnGestureStarted();
            }
            else if (!secondTap.IsStarted)
            {

                Vector delta = position - firstTap.EndPoint;
                if (delta.Length > _maxMovement)
                {
                    AbortGesture();
                    return;
                }

                _timer.Stop();
                secondTap.TrackTouchDown(position, timestamp);
            }
            else
            {
                AbortGesture();
            }
        }

        public void TrackTouchUp(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            if (!firstTap.IsCompleted)
            {
                firstTap.TrackTouchUp(position, timestamp);
            }
            else if (!secondTap.IsCompleted)
            {
                secondTap.TrackTouchUp(position, timestamp);
            }
        }

        public void TrackTouchMove(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            if (!firstTap.IsCompleted)
            {
                firstTap.TrackTouchMove(position, timestamp);
            }
            else if (!secondTap.IsCompleted)
            {
                secondTap.TrackTouchMove(position, timestamp);
            }
        }

        public void AbortGesture()
        {
            if (IsAborted || IsCompleted)
                return;

            IsAborted = true;
            _timer.Stop();
            firstTap.AbortGesture();
            secondTap.AbortGesture();

            OnGestureAborted();
        }

        public TouchDevice TouchDevice
        {
            get; set; 
            //{
            //    if (firstTap.IsStarted && !secondTap.IsStarted)
            //        return firstTap.TouchDevice;
            //    if (secondTap.IsStarted)
            //        return secondTap.TouchDevice;
            //    // TODO: Figure out the logic here, throwing doesn't seem nice
            //    throw new InvalidOperationException("No touch device is associated with this engine");
            //}
            //set
            //{
            //    if (firstTap.IsCompleted)
            //        secondTap.TouchDevice = value;
            //    else
            //        firstTap.TouchDevice = value;
            //}
        }

        #endregion
    }
}
