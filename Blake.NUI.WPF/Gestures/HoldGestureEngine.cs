using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows;

namespace Blake.NUI.WPF.Gestures
{
    /// <summary>
    /// Gesture engine that detects the "Hold" gesture. This gesture is defined as a touch down and hold for a (configurable) amount of time. Releasing the 
    /// touch point or moving it too far before the timeout has been reached will abort the gesture.
    /// </summary>
    /// <remarks>
    /// See IGestureEngine documentation for details about the behavior.
    /// </remarks>
    class HoldGestureEngine : IGestureEngine
    {
        private double _maxMovement; 
        DispatcherTimer _timer;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="HoldGestureEngine"/> class.
        /// </summary>
        /// <param name="timeout">The time that the user must keep the touch pressed in order for it to be considered a hold gesture.</param>
        /// <param name="maxMovement">The maximum movement the user is allowed to move the touch point in order for it to still be considered a hold gesture. Unit is WPF device independent pixels</param>
        public HoldGestureEngine(TimeSpan timeout, double maxMovement)
        {
            if (maxMovement < 0)
                maxMovement = Double.MaxValue;
            _maxMovement = maxMovement;
            _timer = new DispatcherTimer();
            _timer.Interval = timeout;
            _timer.Tick += (s, ee) =>
            {
                ProcessStatus();
            };
        }

        public void AbortGesture()
        {
            _timer.Stop();
            IsAborted = true;
            ProcessStatus();
        }

        private void ProcessStatus()
        {
            // This is always a one-shot timer
            _timer.Stop();
            if (this.TranslationDelta.Length > _maxMovement)
            {
                IsAborted = true;
            }

            if (IsAborted)
            {
                OnGestureAborted();
            }
            else
            {
                IsCompleted = true;
                OnGestureCompleted();
            }
        }

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

        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsAborted { get; set; }

        #region IGestureEngine Members

        public void TrackTouchDown(Point position, DateTime timestamp)
        {
            if (IsCompleted || IsAborted)
                return;

            this.StartPoint = position;
            IsStarted = true;

            OnGestureStarted();

            _timer.Start();
        }

        public void TrackTouchUp(Point position, DateTime timestamp)
        {
            AbortGesture();
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

        #endregion
    }
}
