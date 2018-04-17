using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Blake.NUI.WPF.Gestures
{
    public enum TapGestureMode
    {
        Regular,
        Long
    }

    public class TapGestureTrigger : TriggerBase<UIElement>
    {
        #region Class members

        Dictionary<TouchDevice, TapGestureEngine> tapStatuses;

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

        private TapGestureMode _mode;
        public TapGestureMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (_mode == TapGestureMode.Long)
                {
                    MinMilliseconds = 200;
                    MaxMilliseconds = 5000;
                }
                else
                {
                    MinMilliseconds = 0;
                    MaxMilliseconds = 1000;
                }
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

        public event EventHandler Tap;

        private void OnTap()
        {
            this.InvokeActions(null);
            if (Tap != null)
            {
                Tap(this.AssociatedObject, EventArgs.Empty);
            }
        }

        #endregion

        #region Constructors

        public TapGestureTrigger()
            : base()
        {
            tapStatuses = new Dictionary<TouchDevice, TapGestureEngine>();
        }

        #endregion

        #region Overridden methods

        protected override void OnAttached()
        {
            base.OnAttached();
            var handler = new EngineHandler(() => new TapGestureEngine(MinMilliseconds, MaxMilliseconds, MaxMovement), base.AssociatedObject, HandlesTouches);
            handler.GestureCompleted += (s, e) => OnTap();
        }
        
        #endregion
    }
}
