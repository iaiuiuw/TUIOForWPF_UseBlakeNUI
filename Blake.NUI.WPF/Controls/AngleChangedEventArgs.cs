using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blake.NUI.WPF.Controls
{
    public class AngleChangedEventArgs : EventArgs
    {
        public double OldAngle { get; private set; }
        public double NewAngle { get; private set; }
        public double DeltaAngle { get { return NewAngle - OldAngle; } }

        public AngleChangedEventArgs(double oldAngle, double newAngle)
        {
            this.OldAngle = oldAngle;
            this.NewAngle = newAngle;
        }
    }
}
