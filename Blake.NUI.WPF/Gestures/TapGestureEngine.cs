using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    class TapGestureEngine : IGestureEngine
    {
        #region Class members

        double _maxMilliseconds;
        double _minMilliseconds;
        double _maxMovement;
        DispatcherTimer _timer;

        #endregion

        #region Properties

        public TouchDevice TouchDevice { get; set; }

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

        public TapGestureEngine(double minMilliseconds, double maxMilliseconds, double maxMovement)
        {
            IsStarted = false;
            IsCompleted = false;
            IsAborted = false;

            _maxMilliseconds = maxMilliseconds;
            _minMilliseconds = minMilliseconds;
            _maxMovement = maxMovement;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_maxMilliseconds);
            _timer.Tick += (s, ee) =>
            {
                AbortGesture();
            };
        }

        #endregion

        #region Private Methods

        private void ProcessStatus()
        {
            if (IsAborted)
            {
                OnGestureAborted();
                return;
            }

            double milliseconds = this.TimeDelta.TotalMilliseconds;

            if (milliseconds < _minMilliseconds ||
                milliseconds > _maxMilliseconds)
            {
                IsAborted = true;
                OnGestureAborted();
                return;
            }

            if (this.TranslationDelta.Length > _maxMovement)
            {
                IsAborted = true;
                OnGestureAborted();
                return;
            }

            IsCompleted = true;
            OnGestureCompleted();
        }

        #endregion

        #region IGestureEngine Members

        public void TrackTouchDown(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            this.TouchDownTime = timestamp;
            this.StartPoint = position;
            IsStarted = true;

            OnGestureStarted();

            _timer.Start();
        }

        public void TrackTouchUp(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            this.TouchUpTime = TouchUpTime;
            this.EndPoint = position;

            ProcessStatus();
        }

        public void TrackTouchMove(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            Vector delta = position - StartPoint;
            if (delta.Length > _maxMovement)
            {
                AbortGesture();
            }
        }

        public void AbortGesture()
        {
            _timer.Stop();
            IsAborted = true;
            ProcessStatus();
        }
        #endregion
    }
}
