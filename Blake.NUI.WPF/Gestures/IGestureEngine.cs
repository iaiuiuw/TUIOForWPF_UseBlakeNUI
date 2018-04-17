using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Gestures
{
    /// <summary>
    /// Interface that defines the behavior of a gesture recognizing engine
    /// </summary>
    /// <remarks>
    /// A gesture engine implementation is responsible for detecting a single gesture. The typical life span of an engine is that
    /// it's created when a touch is pressed, constantly feed data while the touch is moving or when it has been released. 
    /// 
    /// A gesture engine is expected to raise the Aborted event as soon as it fails to detect a gesture and similarily it should raise the Completed
    /// event when it has successfully recognized a gesture.
    /// 
    /// An engine will only live for one detection sequence - after either Aborted or Completed is raised, the engine will no longer be used by the handler.
    /// </remarks>
    public interface IGestureEngine
    {
        /// <summary>
        /// Called by the engine handler whenever a touch point has been pressed down
        /// </summary>
        /// <param name="position">The position of the touch point, in screen coordinates</param>
        /// <param name="timestamp">The time of the touch press</param>
        void TrackTouchDown(Point position, DateTime timestamp);
        /// <summary>
        /// Called by the engine handler whenever a touch point is released.
        /// </summary>
        /// <param name="position">The position of the touch point, in screen coordinates</param>
        /// <param name="timestamp">The time of the touch press</param>
        void TrackTouchUp(Point position, DateTime timestamp);
        /// <summary>
        /// Called by the engine handler whenever a touch point is moved.
        /// </summary>
        /// <param name="position">The position of the touch point, in screen coordinates</param>
        /// <param name="timestamp">The time of the touch press</param>
        void TrackTouchMove(Point position, DateTime timestamp);
        /// <summary>
        /// Aborts the gesture recognizer.
        /// </summary>
        void AbortGesture();
        /// <summary>
        /// Gets or sets the touch device involved in the current gesture.
        /// </summary>
        TouchDevice TouchDevice { get; set; }
        /// <summary>
        /// Raised by the engine when it's beginning the process of regonizing a gesture (typically when it first receives a touch down)
        /// </summary>
        event EventHandler GestureStarted;
        /// <summary>
        /// Raised by the engine when it has aborted its gesture recognition
        /// </summary>
        event EventHandler GestureAborted;
        /// <summary>
        /// Raised by the engine when it has successfully recognized a gesture.
        /// </summary>
        event EventHandler GestureCompleted;
    }
}
