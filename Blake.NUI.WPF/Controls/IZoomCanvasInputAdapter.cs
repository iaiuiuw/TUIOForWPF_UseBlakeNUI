using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Blake.NUI.WPF.Controls
{
    public class ZoomCanvasInputManipulationDeltaEventArgs : EventArgs
    {
        public double RotationDelta { get; private set; }
        public Vector TranslationDelta { get; private set; }
        public Vector ScaleDelta { get; private set; }
        public Point ManipulationOrigin { get; private set; }

        public ZoomCanvasInputManipulationDeltaEventArgs(Vector translationDelta, double rotationDelta, Vector scaleDelta, Point manipulationOrigin)
        {
            this.TranslationDelta = translationDelta;
            this.RotationDelta = rotationDelta;
            this.ScaleDelta = scaleDelta;
            this.ManipulationOrigin = manipulationOrigin;
        }
    }

    public class ZoomCanvasInputManipulationCompleteEventArgs : EventArgs
    {
        public double RotationRate { get; private set; }
        public Vector TranslationVelocity { get; private set; }
        public Vector ScaleVelocity { get; private set; }

        public ZoomCanvasInputManipulationCompleteEventArgs(Vector translationVelocity, double rotationRate, Vector scaleVelocity)
        {
            this.TranslationVelocity = translationVelocity;
            this.RotationRate = rotationRate;
            this.ScaleVelocity = scaleVelocity;
        }
    }

    public class ZoomCanvasInputSetStateEventArgs : EventArgs
    {
        public bool IsLocked { get; private set; }
        public Vector Scale { get; private set; }
        public double Orientation { get; private set; }
        public Point Center { get; private set; }

        public ZoomCanvasInputSetStateEventArgs(Point center, double orientation, Vector scale, bool isLocked)
        {
            this.Center = center;
            this.Orientation = orientation;
            this.Scale = scale;
            this.IsLocked = isLocked;
        }
    }

    public interface IZoomCanvasInputAdapter
    {
        void RegisterZoomCanvas(ZoomCanvas canvas, FrameworkElement manipulationElement);

        event EventHandler<ZoomCanvasInputSetStateEventArgs> SetState;
        event EventHandler<ZoomCanvasInputManipulationDeltaEventArgs> ManipulationDelta;
        event EventHandler<ZoomCanvasInputManipulationCompleteEventArgs> ManipulationComplete;
    }
}
