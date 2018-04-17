using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    /// <summary>
    /// Base class for engine handlers. 
    /// </summary>
    internal abstract class EngineHandlerBase
    {
        private readonly Func<IGestureEngine> _engineCreator;
        private readonly UIElement _trackedElement;

        /// <summary>
        /// The root element that is participating in gesture recognition
        /// </summary>
        protected UIElement TrackedElement { get { return _trackedElement; } }
        /// <summary>
        /// If true, then touch events will be swallowed by the gesture recognition. This is generally not recommended since it will inhibit the use of multiple gesture engines
        /// </summary>
        protected bool HandlesTouchEvents { get; set; }

        /// <summary>
        /// Raised when a gesture is successfully recognized. The actual gesture depends on what gesture engine was provided in the constructor.
        /// </summary>
        public event EventHandler<GestureCompletedEventArgs> GestureCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHandler"/> class.
        /// </summary>
        /// <param name="engineCreator">The function used to create an instance of the engine that actually recognizes the gesture.</param>
        /// <param name="trackedElement">The tracked element.</param>
        /// <param name="handleTouchEvent">if set to <c>true</c> then all touch events occurring on the tracked element will get e.Handled set to true to avoid further routing of the events.</param>
        public EngineHandlerBase(Func<IGestureEngine> engineCreator, UIElement trackedElement, bool handleTouchEvent = false)
        {
            this._engineCreator = engineCreator;
            this._trackedElement = trackedElement;
            this.HandlesTouchEvents = handleTouchEvent;
            trackedElement.AddHandler(UIElement.TouchDownEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchDown), true);
            trackedElement.AddHandler(UIElement.TouchUpEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchUp), true);
            trackedElement.AddHandler(UIElement.TouchMoveEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchMove), true);
        }

        /// <summary>
        /// Stops gesture recognition for the tracked element
        /// </summary>
        public void StopTracking()
        {
            _trackedElement.RemoveHandler(UIElement.TouchDownEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchDown));
            _trackedElement.RemoveHandler(UIElement.TouchUpEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchUp));
            _trackedElement.RemoveHandler(UIElement.TouchMoveEvent, new EventHandler<TouchEventArgs>(OnTrackedElementTouchMove));
            OnStopTracking();
        }

        /// <summary>
        /// Creates and setups event listeners for a new gesture engine. 
        /// </summary>
        /// <param name="device">The touch device that caused started the potential gesture</param>
        /// <returns>The newly created engine</returns>
        protected IGestureEngine CreateAndSetupGestureEngine(TouchDevice device)
        {
            var engine = _engineCreator();
            engine.TouchDevice = device;
            engine.GestureAborted += engine_GestureAborted;
            engine.GestureCompleted += engine_GestureCompleted;
            engine.GestureStarted += engine_GestureStarted;
            return engine;
        }

        /// <summary>
        /// Removes all event handlers for the specified engine
        /// </summary>
        /// <param name="engine">The engine to stop listening to</param>
        protected void UnregisterEngine(IGestureEngine engine)
        {
            // TODO: Is this needed? will we leak otherwise?
            engine.GestureAborted -= engine_GestureAborted;
            engine.GestureCompleted -= engine_GestureCompleted;
            engine.GestureStarted -= engine_GestureStarted;
        }

        /// <summary>
        /// Called when the <see cref="TrackedElement"/> is no longer tracked for touch events.
        /// </summary>
        protected virtual void OnStopTracking()
        {

        }

        /// <summary>
        /// Called when a touch down event occurs on the TrackedElement.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TouchEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackedElementTouchDown(object sender, TouchEventArgs e)
        {
            if (HandlesTouchEvents)
                e.Handled = true;
        }

        /// <summary>
        /// Called when a touch move event occurs on the TrackedElement.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TouchEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackedElementTouchMove(object sender, TouchEventArgs e)
        {
            if (HandlesTouchEvents)
                e.Handled = true;
        }
        
        /// <summary>
        /// Called when a touch up event occurs on the TrackedElement.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TouchEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackedElementTouchUp(object sender, TouchEventArgs e)
        {
            if (HandlesTouchEvents)
                e.Handled = true;
        }

        private void RaiseGestureCompleted(UIElement element, TouchDevice device)
        {
            if (GestureCompleted != null)
            {
                GestureCompleted(this, new GestureCompletedEventArgs(element, device));
            }
        }

        private void engine_GestureStarted(object sender, EventArgs e)
        {
            OnGestureStarted((IGestureEngine)sender);
        }

        private void engine_GestureAborted(object sender, EventArgs e)
        {
            OnGestureAborted((IGestureEngine)sender);
        }

        private void engine_GestureCompleted(object sender, EventArgs e)
        {
            var engine = (IGestureEngine)sender;
            OnGestureCompleted(engine);
            // By using InputHitTest() we'll get a consistent behavior with other WPF input events, for example honoring IsHitTestVisible and control visibility
            var element = TrackedElement.InputHitTest(engine.TouchDevice.GetTouchPoint(TrackedElement).Position) as UIElement;
            RaiseGestureCompleted(element ?? TrackedElement, engine.TouchDevice);
            UnregisterEngine(engine);
        }

        /// <summary>
        /// Called when an engine starts to recognize gestures
        /// </summary>
        /// <param name="engine">The engine that was started</param>
        protected virtual void OnGestureStarted(IGestureEngine engine)
        {

        }

        /// <summary>
        /// Called when an engine aborts its gesture recognition. Typically this is done when the touch events doesn't match the engine's gesture but can it can also be aborted for other reasons.
        /// </summary>
        /// <param name="engine">The engine that was aborted</param>
        protected virtual void OnGestureAborted(IGestureEngine engine)
        {

        }
        /// <summary>
        /// Called when an engine has successfully recognized a gesture.
        /// </summary>
        /// <param name="engine">The engine that recognized a gesture</param>
        protected virtual void OnGestureCompleted(IGestureEngine engine)
        {
            
        }

        /// <summary>
        /// Converts the timestamp provided in TouchEventArgs to a proper DateTime struct.
        /// </summary>
        /// <param name="timestamp">The timestamp, as received from e.TimeStamp.</param>
        protected static DateTime TimestampToDateTime(int timestamp)
        {
            DateTime dt = DateTime.Now;
            dt.AddMilliseconds(Environment.TickCount - timestamp);
            return dt;
        }
    }

    /// <summary>
    /// General purpose engine handler for gestures that only involve a single touch point (like "Tap" and "Hold")
    /// </summary>
    /// <remarks>
    /// This handler can simultaniously recognize gestures on different touch points since each contact is handled 
    /// separately.
    /// </remarks>
    internal class EngineHandler : EngineHandlerBase
    {
        private Dictionary<InputDevice, IGestureEngine> engineStatuses = new Dictionary<InputDevice, IGestureEngine>();

        public EngineHandler(Func<IGestureEngine> engineCreator, UIElement trackedElement, bool handleTouchEvent = false)
            : base(engineCreator, trackedElement, handleTouchEvent)
        {
            
        }

        private void AbortExistingEngineIfAny(InputDevice device)
        {
            if (!engineStatuses.ContainsKey(device))
                return;
            var engine = engineStatuses[device];
            engine.AbortGesture();
            engineStatuses.Remove(device);
        }

        protected override void OnTrackedElementTouchDown(object sender, TouchEventArgs e)
        {
            AbortExistingEngineIfAny(e.TouchDevice);

            var engine = CreateAndSetupGestureEngine(e.TouchDevice);
            
            var pos = e.GetTouchPoint(null).Position;
            var timestamp = TimestampToDateTime(e.Timestamp);
            engine.TrackTouchDown(pos, timestamp);
            engineStatuses[e.TouchDevice] = engine;

            base.OnTrackedElementTouchDown(sender, e);
        }

        protected override void OnTrackedElementTouchMove(object sender, TouchEventArgs e)
        {
            if (!engineStatuses.ContainsKey(e.TouchDevice))
                return;

            DateTime timestamp = TimestampToDateTime(e.Timestamp);
            Point position = e.GetTouchPoint(null).Position;
            engineStatuses[e.TouchDevice].TrackTouchMove(position, timestamp);

            base.OnTrackedElementTouchMove(sender, e);
        }

        protected override void OnTrackedElementTouchUp(object sender, TouchEventArgs e)
        {
            if (!engineStatuses.ContainsKey(e.TouchDevice))
                return;

            DateTime timestamp = TimestampToDateTime(e.Timestamp);
            Point position = e.GetTouchPoint(null).Position;

            engineStatuses[e.TouchDevice].TrackTouchUp(position, timestamp);

            base.OnTrackedElementTouchUp(sender, e);
        }

        protected override void OnGestureAborted(IGestureEngine engine)
        {
            base.OnGestureAborted(engine);
            if (engineStatuses.ContainsKey(engine.TouchDevice))
                engineStatuses.Remove(engine.TouchDevice);
        }

        protected override void OnGestureCompleted(IGestureEngine engine)
        {
            base.OnGestureCompleted(engine);
            if (engineStatuses.ContainsKey(engine.TouchDevice))
                engineStatuses.Remove(engine.TouchDevice);
        }
    }

    /// <summary>
    /// Engine handler for engines that needs more than one touch point to recognize gestures (e.g. DoubleTap)
    /// </summary>
    /// <remarks>
    /// Since this handler requires more than one touch point in order to recognize gestures it is not possible for it
    /// to simultaniously detect parallell gestures (like e.g. two DoubleTap gestures occurring at the same time).
    /// </remarks>
    internal class MultiEngineHandler : EngineHandlerBase
    {
        private IGestureEngine currentEngine;

        public MultiEngineHandler(Func<IGestureEngine> engineCreator, UIElement trackedElement, bool handleTouchEvent = false)
            : base(engineCreator, trackedElement, handleTouchEvent)
        {
            
        }
        protected override void OnTrackedElementTouchDown(object sender, TouchEventArgs e)
        {
            if (currentEngine == null)
                currentEngine = CreateAndSetupGestureEngine(e.TouchDevice);
            DateTime timestamp = TimestampToDateTime(e.Timestamp);
            Point position = e.GetTouchPoint(null).Position;
            currentEngine.TrackTouchDown(position, timestamp);

            base.OnTrackedElementTouchDown(sender, e);
        }
        protected override void OnTrackedElementTouchMove(object sender, TouchEventArgs e)
        {
            if (currentEngine == null)
                return;
            DateTime timestamp = TimestampToDateTime(e.Timestamp);
            Point position = e.GetTouchPoint(null).Position;
            currentEngine.TrackTouchMove(position, timestamp);
            base.OnTrackedElementTouchMove(sender, e);
        }
        protected override void OnTrackedElementTouchUp(object sender, TouchEventArgs e)
        {
            if (currentEngine == null)
                return;
            DateTime timestamp = TimestampToDateTime(e.Timestamp);
            Point position = e.GetTouchPoint(null).Position;
            currentEngine.TrackTouchUp(position, timestamp);
            base.OnTrackedElementTouchUp(sender, e);
        }
        protected override void OnGestureAborted(IGestureEngine engine)
        {
            currentEngine = null;
            base.OnGestureAborted(engine);
        }
        protected override void OnGestureCompleted(IGestureEngine engine)
        {
            currentEngine = null;
            base.OnGestureCompleted(engine);
        }
    }

    /// <summary>
    /// Event arguments for the GestureCompleted event
    /// </summary>
    internal class GestureCompletedEventArgs : EventArgs
    {
        public GestureCompletedEventArgs(UIElement source, TouchDevice touchDevice)
        {
            Source = source;
            TouchDevice = touchDevice;
        }
        /// <summary>
        /// The element that received the touch events that caused the recognized gesture
        /// </summary>
        public UIElement Source { get; set; }
        /// <summary>
        /// The touch device that was used to make the gesture. In the case of a gesture involving multiple touch points, this property contains the first device.
        /// </summary>
        public TouchDevice TouchDevice { get; set; }
    }
}
