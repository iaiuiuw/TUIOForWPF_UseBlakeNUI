//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;

namespace Blake.NUI.WPF.Touch.Interop
{
    
    /// <summary>
    /// EventArgs passed to Touch handlers 
    /// </summary>
    public class InteropTouchEventArgs : EventArgs
    {
        private readonly IPointToClient _HWndWrapper;
        private readonly float _dpiXFactor;
        private readonly float _dpiYFactor;

        /// <summary>
        /// Create new touch event argument instance
        /// </summary>
        /// <param name="hWndWrapper">The target control</param>
        /// <param name="touchInput">one of the inner touch input in the message</param>
        internal InteropTouchEventArgs(IPointToClient HWndWrapper, float dpiX, float dpiY, ref TOUCHINPUT touchInput)
        {
            _HWndWrapper = HWndWrapper;
            _dpiXFactor = 96F / dpiX;
            _dpiYFactor = 96F / dpiY;
            DecodeTouch(ref touchInput);
        }

        private bool CheckFlag(int value)
        {
            return (Flags & value) != 0;
        }

        

        // Decodes and handles WM_TOUCH* messages.
        private void DecodeTouch(ref TOUCHINPUT touchInput)
        {
            // TOUCHINFO point coordinates and contact size is in 1/100 of a pixel; convert it to pixels.
            // Also convert screen to client coordinates.
            if ( (touchInput.dwMask & User32.TOUCHINPUTMASKF_CONTACTAREA) != 0)
                ContactSize = new Size(AdjustDpiX(touchInput.cxContact / 100), AdjustDpiY(touchInput.cyContact / 100));
                          
            Id = touchInput.dwID;

            System.Drawing.Point p = _HWndWrapper.PointToClient(new System.Drawing.Point(touchInput.x / 100, touchInput.y / 100));
            Location = new Point(AdjustDpiX(p.X), AdjustDpiY(p.Y));

            Time = touchInput.dwTime;
            TimeSpan ellapse = TimeSpan.FromMilliseconds(Environment.TickCount - touchInput.dwTime);
            AbsoluteTime = DateTime.Now - ellapse;
           
            Mask = touchInput.dwMask;
            Flags = touchInput.dwFlags;
        }


        private int AdjustDpiX(int value)
        {
            return (int)(value * _dpiXFactor);
        }

        private int AdjustDpiY(int value)
        {
            return (int)(value * _dpiYFactor);
        }
      
        /// <summary>
        /// Touch client coordinate in pixels
        /// </summary>
        public Point Location {get; private set; }

        public PresentationSource ActiveSource
        {
            get
            {
                return HwndSource.FromHwnd(_HWndWrapper.Handle);
            }
        }

        public Rect BoundingRect
        {
            get
            {
                if (!ContactSize.HasValue)
                {
                    return new Rect(Location, new Size(1, 1));
                }

                Point corner = new Point(Location.X - ContactSize.Value.Width / 2,
                                         Location.Y - ContactSize.Value.Height / 2);
                return new Rect(corner, ContactSize.Value);
            }
        }
        /// <summary>
        /// A touch point identifier that distinguishes a particular touch input
        /// </summary>
        public int Id { get; private set; }
       
        /// <summary>
        /// A set of bit flags that specify various aspects of touch point
        /// press, release, and motion. 
        /// </summary>
        public int Flags { get; private set; }
        
        /// <summary>
        /// mask which fields in the structure are valid
        /// </summary>
        public int Mask { get; private set; }

        /// <summary>
        /// touch event time
        /// </summary>
        public DateTime AbsoluteTime { get; private set; }

        /// <summary>
        /// touch event time from system up
        /// </summary>
        public int Time { get; private set; }
        
        /// <summary>
        /// the size of the contact area in pixels
        /// </summary>
        public Size? ContactSize { get; private set; }

        /// <summary>
        /// Is Primary Contact (The first touch sequence)
        /// </summary>
        public bool IsPrimaryContact
        {
            get { return (Flags & User32.TOUCHEVENTF_PRIMARY) != 0; }
        }

        /// <summary>
        /// Specifies that movement occurred
        /// </summary>
        public bool IsTouchMove
        {
            get { return CheckFlag(User32.TOUCHEVENTF_MOVE); }
        }

        /// <summary>
        /// Specifies that the corresponding touch point was established through a new contact
        /// </summary>
        public bool IsTouchDown
        {
            get { return CheckFlag(User32.TOUCHEVENTF_DOWN); }
        }

        /// <summary>
        /// Specifies that a touch point was removed
        /// </summary>
        public bool IsTouchUp
        {
            get { return CheckFlag(User32.TOUCHEVENTF_UP); }
        }

        /// <summary>
        /// Specifies that a touch point is in range
        /// </summary>
        public bool IsTouchInRange
        {
            get { return CheckFlag(User32.TOUCHEVENTF_INRANGE); }
        }

        /// <summary>
        /// specifies that this input was not coalesced.
        /// </summary>
        public bool IsTouchNoCoalesce
        {
            get { return CheckFlag(User32.TOUCHEVENTF_NOCOALESCE); }
        }

        /// <summary>
        /// Specifies that the touch point is associated with a pen contact
        /// </summary>
        public bool IsTouchPen
        {
            get { return CheckFlag(User32.TOUCHEVENTF_PEN); }
        }

        /// <summary>
        /// The touch event came from the user's palm
        /// </summary>
        /// <remarks>Set <see cref="DisablePalmRejection"/> to true</remarks>
        public bool IsTouchPalm
        {
            get { return CheckFlag(User32.TOUCHEVENTF_PALM); }
        }
    }
}