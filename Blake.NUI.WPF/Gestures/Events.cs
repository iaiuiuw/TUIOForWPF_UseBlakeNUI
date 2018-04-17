using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    /// <summary>
    /// Container class for the gesture routed events and supporting functionality
    /// </summary>
    public static class Events
    {
        /// <summary>Gets or sets the minimum amount of time a between touch down/touch up before it can be called a Tap gesture</summary>
        public static int TapMinMilliseconds { get; set; }
        /// <summary>Gets or sets the maximum amount of time a between touch down/touch up before it can be called a Tap gesture</summary>
        public static int TapMaxMilliseconds { get; set; }
        /// <summary>Gets or sets the maximum amount of time a between two taps in order to call it a double tap</summary>
        public static int DoubleTapGapMilliseconds { get; set; }
        /// <summary>Gets or sets the maximum amount of pixel movements that the touch point can move during a tap gesture</summary>
        public static double TapMaxMovement { get; set; }
        /// <summary>Gets or sets the amount of time before a touch down is generating a hold gesture</summary>
        public static TimeSpan HoldGestureTimeout { get; set; }
        /// <summary>Gets or sets the maximum amount of pixel movements that the touch point can move during a hold gesture</summary>
        public static double HoldMaxMovement { get; set; }
        
        static Events()
        {
            TapMinMilliseconds = 0;
            TapMaxMilliseconds = 500;
            TapMaxMovement = 25;
            DoubleTapGapMilliseconds = 200;
            HoldGestureTimeout = TimeSpan.FromMilliseconds(500);
            HoldMaxMovement = -1; // no limit
        }

        /// <summary>
        /// Registers a framework element for gesture recognition. Any element below the root element will be eligable for gesture events and events bubble through the tree like normal routed events
        /// </summary>
        /// <param name="root">The root element where gesture support should be supported. This element and any element below it in the tree will get gesture support</param>
        public static void RegisterGestureEventSupport(FrameworkElement root)
        {
            // TODO: should we allow an element to unregister?
            EngineHandlerBase engine = null;
            engine = new EngineHandler(() => new HoldGestureEngine(HoldGestureTimeout, HoldMaxMovement), root, false);
            engine.GestureCompleted += (s, e) => e.Source.RaiseEvent(new GestureEventArgs(HoldGestureEvent, e.TouchDevice));

            engine = new EngineHandler(() => new TapGestureEngine(TapMinMilliseconds, TapMaxMilliseconds, TapMaxMovement), root, false);
            engine.GestureCompleted += (s, e) => e.Source.RaiseEvent(new GestureEventArgs(TapGestureEvent, e.TouchDevice));

            engine = new MultiEngineHandler(() => new DoubleTapGestureEngine(TapMinMilliseconds, DoubleTapGapMilliseconds, TapMaxMilliseconds, TapMaxMovement), root, false);
            engine.GestureCompleted += (s, e) => e.Source.RaiseEvent(new GestureEventArgs(DoubleTapGestureEvent, e.TouchDevice));

        }

        #region HoldGesture Routed Event

        /// <summary>
        /// HoldGesture Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent HoldGestureEvent = EventManager.RegisterRoutedEvent("HoldGesture", 
            RoutingStrategy.Bubble, typeof(GestureEventHandler), typeof(Events));

        public static void AddHoldGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(HoldGestureEvent, handler);
            }
        }

        public static void RemoveHoldGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(HoldGestureEvent, handler);
            }
        }

        #endregion

        #region TapGesture Routed Event

        /// <summary>
        /// TapGesture Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent TapGestureEvent = EventManager.RegisterRoutedEvent("TapGesture",
            RoutingStrategy.Bubble, typeof(GestureEventHandler), typeof(Events));

        /// <summary>
        /// Adds a handler for the TapGesture attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be added</param>
        public static void AddTapGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(TapGestureEvent, handler);
            }
        }

        /// <summary>
        /// Removes a handler for the TapGesture attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be removed</param>
        public static void RemoveTapGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(TapGestureEvent, handler);
            }
        }

        #endregion

        #region DoubleTapGesture Routed Event

        /// <summary>
        /// DoubleTapGesture Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent DoubleTapGestureEvent = EventManager.RegisterRoutedEvent("DoubleTapGesture",
            RoutingStrategy.Bubble, typeof(GestureEventHandler), typeof(Events));

        /// <summary>
        /// Adds a handler for the DoubleTapGesture attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be added</param>
        public static void AddDoubleTapGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(DoubleTapGestureEvent, handler);
            }
        }

        /// <summary>
        /// Removes a handler for the DoubleTapGesture attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be removed</param>
        public static void RemoveDoubleTapGestureHandler(DependencyObject d, GestureEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(DoubleTapGestureEvent, handler);
            }
        }

        #endregion
    }
    
    /// <summary>
    /// Event handler for the GestureCompleted event
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">Event arguments containing information about the gesture</param>
    public delegate void GestureEventHandler(object sender, GestureEventArgs e);

    public class GestureEventArgs : RoutedEventArgs
    {
        private readonly TouchDevice _device;
        public GestureEventArgs(RoutedEvent routedEvent, TouchDevice device)
            : base(routedEvent)
        {
            _device = device;
        }
        public GestureEventArgs(RoutedEvent routedEvent, object source, TouchDevice device)
            : base(routedEvent, source)
        {
            _device = device;
        }
        public TouchDevice TouchDevice { get { return _device; } }
    }
}
