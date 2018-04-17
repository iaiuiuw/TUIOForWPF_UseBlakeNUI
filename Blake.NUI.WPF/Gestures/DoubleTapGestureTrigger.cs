using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    public class DoubleTapGestureTrigger : TriggerBase<UIElement>
    {
        #region Class members

        DoubleTapGestureEngine gestureEngine;

        #endregion

        #region Properties

        private bool _handlesTouches = false;
        public bool HandlesTouches
        {
            get
            {
                return _handlesTouches;
            }
            set
            {
                _handlesTouches = value;
            }
        }

        private int _minMilliseconds = 0;
        public int MinMilliseconds
        {
            get
            {
                return _minMilliseconds;
            }
            set
            {
                _minMilliseconds = value;
            }
        }
        
        private int _maxMilliseconds = 1000;
        public int MaxMilliseconds
        {
            get
            {
                return _maxMilliseconds;
            }
            set
            {
                _maxMilliseconds = value;
            }
        }

        private int _gapMilliseconds = 400;
        public int GapMilliseconds
        {
            get
            {
                return _gapMilliseconds;
            }
            set
            {
                _gapMilliseconds = value;
            }
        }

        private double _maxMovement = 25;
        public double MaxMovement
        {
            get
            {
                return _maxMovement;
            }
            set
            {
                _maxMovement = value;
            }
        }

        #endregion
        
        #region Events

        public event EventHandler DoubleTap;

        private void OnDoubleTap()
        {
            this.InvokeActions(null);
            if (DoubleTap != null)
            {
                DoubleTap(this.AssociatedObject, EventArgs.Empty);
            }
        }

        #endregion

        #region Constructors

        public DoubleTapGestureTrigger()
            : base()
        {
            
        }

        #endregion

        #region Overridden methods

        protected override void OnAttached()
        {
            base.OnAttached();
            var handler = new MultiEngineHandler(() => new DoubleTapGestureEngine(MinMilliseconds, GapMilliseconds, MaxMilliseconds, MaxMovement), base.AssociatedObject, HandlesTouches);
            handler.GestureCompleted += (s, e) => OnDoubleTap();
        }
        
        #endregion
    }
}
