using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blake.NUI.WPF.Controls;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF.Touch
{
    public class ZoomCanvasInputAdapterTouch : IZoomCanvasInputAdapter
    {
        public void RegisterZoomCanvas(ZoomCanvas canvas, System.Windows.FrameworkElement manipulationElement)
        {
            manipulationElement.IsManipulationEnabled = true;

            manipulationElement.ManipulationStarting += new EventHandler<ManipulationStartingEventArgs>(manipulationElement_ManipulationStarting);
            manipulationElement.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(manipulationElement_ManipulationDelta);
            manipulationElement.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(manipulationElement_ManipulationCompleted);
        }

        void VerifyManipulators(object sender, IEnumerable<IManipulator> manipulators)
        {
            UIElement element = sender as UIElement;
            if (element == null)
                return;

            List<IManipulator> manipulatorsToRemove = new List<IManipulator>();

            foreach (IManipulator manipulator in manipulators)
            {
                if (!GetIsManipulatorAcceptable(manipulator))
                {
                    manipulatorsToRemove.Add(manipulator);
                }
            }

            foreach (IManipulator manipulator in manipulatorsToRemove)
            {
                try
                {
                    Manipulation.RemoveManipulator(element, manipulator);
                }
                catch (InvalidOperationException)
                {
                    //Race condition, user may have removed the manipulator already
                    //Ignore InvalidOperationException
                }
            }
        }

        protected virtual bool GetIsManipulatorAcceptable(IManipulator manipulator)
        {
            return true;
        }

        protected virtual void manipulationElement_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            VerifyManipulators(sender, e.Manipulators);   
            e.Handled = true;
            OnManipulationStarting();
        }
        
        protected virtual void manipulationElement_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {

            VerifyManipulators(sender, e.Manipulators);
            e.Handled = true;
            OnManipulationComplete(e.FinalVelocities.LinearVelocity,
                                   e.FinalVelocities.AngularVelocity,
                                   e.FinalVelocities.ExpansionVelocity);
        }

        protected virtual void manipulationElement_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            VerifyManipulators(sender, e.Manipulators);
            e.Handled = true;
            OnManipulationDelta(e.DeltaManipulation.Translation,
                                e.DeltaManipulation.Rotation,
                                e.DeltaManipulation.Scale,
                                e.ManipulationOrigin);
        }

        #region Events

        public event EventHandler<ZoomCanvasInputSetStateEventArgs> SetState;

        private void OnSetState(Point center, double orientation, Vector scale, bool isLocked)
        {
            if (SetState == null)
                return;
            SetState(this, new ZoomCanvasInputSetStateEventArgs(center, orientation, scale, isLocked));
        }

        public event EventHandler ManipulationStarting;

        private void OnManipulationStarting()
        {
            if (ManipulationStarting == null)
                return;

            ManipulationStarting(this, EventArgs.Empty);
        }

        public event EventHandler<ZoomCanvasInputManipulationDeltaEventArgs> ManipulationDelta;

        private void OnManipulationDelta(Vector translationDelta, double orientationDelta, Vector scaleDelta, Point manipulationOrigin)
        {
            if (ManipulationDelta == null)
                return;
            ManipulationDelta(this, new ZoomCanvasInputManipulationDeltaEventArgs(translationDelta, orientationDelta, scaleDelta, manipulationOrigin));
        }

        public event EventHandler<ZoomCanvasInputManipulationCompleteEventArgs> ManipulationComplete;

        private void OnManipulationComplete(Vector translationVelocity, double rotationRate, Vector scaleVelocity)
        {
            if (ManipulationComplete == null)
                return;
            ManipulationComplete(this, new ZoomCanvasInputManipulationCompleteEventArgs(translationVelocity, rotationRate, scaleVelocity));
        }

        #endregion
    }
}
