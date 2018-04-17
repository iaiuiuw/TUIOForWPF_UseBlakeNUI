using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    public class HoldGestureTrigger : TriggerBase<UIElement>
    {
        public bool HandlesTouches { get; set; }
        public TimeSpan HoldTimeout { get; set; }
        public double MaxMovement { get; set; }
        public event EventHandler Hold;

        public HoldGestureTrigger()
            : base()
        {
            HoldTimeout = TimeSpan.FromMilliseconds(500);
            MaxMovement = Double.MaxValue;
        }

        private void OnHold()
        {
            this.InvokeActions(null);
            if (Hold != null)
                Hold(this, EventArgs.Empty);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            var handler = new EngineHandler(() => new HoldGestureEngine(HoldTimeout, MaxMovement), base.AssociatedObject);
            handler.GestureCompleted += (s, e) => this.InvokeActions(null);
        }
    }
}
