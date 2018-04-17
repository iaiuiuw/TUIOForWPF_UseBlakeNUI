using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Blake.NUI.WPF.ManipulationHelpers
{
    /// <summary>
    /// Simple class that allows you to add and immediately remove a dummy manipulator.
    /// If there are no other manipulators, this will immediately start inertia.
    /// </summary>
    public class InertiaDummyManipulator : IManipulator
    {
        private delegate void InvokeHandler();

        public static void ManipulateAndStartInertia(UIElement manipulatedElement)
        {
            manipulatedElement.Dispatcher.BeginInvoke((InvokeHandler)delegate
            {
                Manipulation.AddManipulator(manipulatedElement, new InertiaDummyManipulator(manipulatedElement));
            });
        }

        private UIElement ManipulatedElement { get; set; }

        private InertiaDummyManipulator(UIElement manipulatedElement)
        {
            if (manipulatedElement == null)
            {
                throw new ArgumentNullException("manipulatedElement");
            }
            this.ManipulatedElement = manipulatedElement;
        }

        public Point GetPosition(IInputElement relativeTo)
        {
            ManipulatedElement.Dispatcher.BeginInvoke((InvokeHandler)delegate
            {
                Manipulation.RemoveManipulator(ManipulatedElement, this);
            });
            return new Point();
        }

        public int Id
        {
            get { return 10000; }
        }

        public void ManipulationEnded(bool cancel)
        {

        }

        public event EventHandler Updated;
    }
}
