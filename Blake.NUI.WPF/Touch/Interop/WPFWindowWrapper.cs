//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Drawing;

namespace Blake.NUI.WPF.Touch.Interop
{
    /// <summary>
    /// Wrapp HWND source such as System.Windows.Forms.Control, or System.Windows.Window
    /// </summary>
    public interface IPointToClient
    {
        IntPtr Handle { get; }
        /// <summary>
        /// Computes the location of the specified screen point into client coordinates
        /// </summary>
        /// <param name="point">The screen coordinate System.Drawing.Point to convert</param>
        /// <returns>A point that represents the converted point in client coordinates</returns>
        Point PointToClient(Point point);
    }

    /// <summary>
    /// Represents a WPF Window
    /// </summary>
    class WPFWindowWrapper : IPointToClient
    {
        private readonly System.Windows.Window _window;

        public WPFWindowWrapper(System.Windows.Window window)
        {
            _window = window;

            GetHandle(window);

            if (this.Handle == IntPtr.Zero)
            {
                window.Loaded += (s, e) => { GetHandle(window); };
                return;
            }
        }

        private void GetHandle(System.Windows.Window window)
        {
            WindowInteropHelper interop = new WindowInteropHelper(window);
            this.Handle = interop.Handle;
        }
        
        #region IPointToClient Members

        public IntPtr Handle { get; private set; }

        public System.Drawing.Point PointToClient(System.Drawing.Point point)
        {
            System.Windows.Point sourcePoint = new System.Windows.Point(point.X, point.Y);
            System.Windows.Point destinationPoint = _window.PointFromScreen(sourcePoint);
            return new System.Drawing.Point((int)(0.5 + destinationPoint.X), (int)(0.5 + destinationPoint.Y));
        }

        #endregion
    }
    
}