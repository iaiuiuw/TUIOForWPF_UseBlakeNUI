//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace Blake.NUI.WPF.Touch.Interop
{
    /// <summary>
    /// Handles touch events for a hWnd
    /// </summary>
    public class TouchHandler
    {
        private bool _disablePalmRejection;

        public IPointToClient HwndWrapper { get; private set; }

        public IntPtr Handle { get; private set; }

        public bool IsHandleCreated
        {
            get { return Handle != IntPtr.Zero; }
        }

        /// <summary>
        /// The X DPI of the target window
        /// </summary>
        public float DpiX { get; private set; }

        /// <summary>
        /// The Y DPI of the target window
        /// </summary>
        public float DpiY { get; private set; }

        public TouchHandler(Window window)
        {
            Win32Touch.DisableWPFTabletSupport();
            GetHandle(window);
            HwndWrapper = new WPFWindowWrapper(window);

        }

        private void GetHandle(Window window)
        {
            WindowInteropHelper interop = new WindowInteropHelper(window);
            this.Handle = interop.Handle;
            
            if (!IsHandleCreated)
            {
                window.Loaded += (s, e) => { GetHandle(window); };
                return;
            }

            HwndSource.FromHwnd(interop.Handle).AddHook(new HwndSourceHook(WindowProc));
            
            if (!SetHWndTouchInfo())
            {
                throw new NotSupportedException("Cannot register window");
            }

            //take the desktop DPI
            using (Graphics graphics = Graphics.FromHwnd(Handle))
            {
                DpiX = graphics.DpiX;
                DpiY = graphics.DpiY;
            }
        }

        /// <summary>
        /// Enabling this flag disables palm rejection
        /// </summary>
        public bool DisablePalmRejection
        {
            get
            {
                return _disablePalmRejection;
            }
            set
            {
                if (_disablePalmRejection == value)
                    return;

                _disablePalmRejection = value;

                if (IsHandleCreated)
                {
                    User32.UnregisterTouchWindow(Handle);
                    SetHWndTouchInfo();
                }
            }
        }

        /// <summary>
        /// Register for touch event
        /// </summary>
        /// <returns>true if succeeded</returns>
        protected bool SetHWndTouchInfo()
        {
            return User32.RegisterTouchWindow(Handle, _disablePalmRejection ? User32.TouchWindowFlag.WantPalm : 0);
        }

        /// <summary>
        /// Intercept and fire touch events
        /// </summary>
        /// <param name="hWnd">The Windows Handle</param>
        /// <param name="msg">Windows Message</param>
        /// <param name="wparam">wParam</param>
        /// <param name="lparam">lParam</param>
        /// <returns></returns>
        protected IntPtr WindowProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == User32.WM_TOUCH)
            {
                    foreach (InteropTouchEventArgs arg in Win32Touch.DecodeMessage(this.HwndWrapper, msg, wparam, lparam, DpiX, DpiY))
                    {
                        if (TouchDown != null && arg.IsTouchDown)
                        {
                            TouchDown(this, arg);
                        }
                    
                        if (TouchMove != null && arg.IsTouchMove)
                        {
                            TouchMove(this, arg);
                        }
                    
                        if (TouchUp != null && arg.IsTouchUp)
                        {
                            TouchUp(this, arg);
                        }
                    }
                    handled = true;
            }
            if (msg >= User32.WM_MOUSEFIRST &&
                msg <= User32.WM_MOUSELAST &&
                User32.IsPenOrTouchMessage())
            {
                //Handle mouse events promoted from touch so promoted duplicate WPF mouse events don't occur
                handled = true;
            }
            return IntPtr.Zero;
        }

        // Touch event handlers

        /// <summary>
        /// Register to receive TouchDown Events
        /// </summary>
        public event EventHandler<InteropTouchEventArgs> TouchDown;   // touch down event handler

        /// <summary>
        /// Register to receive TouchUp Events
        /// </summary>
        public event EventHandler<InteropTouchEventArgs> TouchUp;     // touch up event handler

        /// <summary>
        /// Register to receive TouchMove Events
        /// </summary>
        public event EventHandler<InteropTouchEventArgs> TouchMove;   // touch move event handler
    }


}