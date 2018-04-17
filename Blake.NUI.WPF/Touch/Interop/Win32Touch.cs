using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Reflection;

namespace Blake.NUI.WPF.Touch.Interop
{
    public static class Win32Touch
    {
        public static void DisableWPFTabletSupport()
        {
            // Get a collection of the tablet devices for this window.  
            TabletDeviceCollection devices = System.Windows.Input.Tablet.TabletDevices;

            if (devices.Count > 0)
            {
                // Get the Type of InputManager.
                Type inputManagerType = typeof(System.Windows.Input.InputManager);

                // Call the StylusLogic method on the InputManager.Current instance.
                object stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, InputManager.Current, null);

                if (stylusLogic != null)
                {
                    //  Get the type of the stylusLogic returned from the call to StylusLogic.
                    Type stylusLogicType = stylusLogic.GetType();

                    // Loop until there are no more devices to remove.
                    while (devices.Count > 0)
                    {
                        // Remove the first tablet device in the devices collection.
                        stylusLogicType.InvokeMember("OnTabletRemoved",
                                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                                null, stylusLogic, new object[] { (uint)0 });
                    }
                }

            }
        }

        /// <summary>
        /// Decode the message and create a collection of event arguments
        /// </summary>
        /// <remarks>
        /// One Windows message can result a group of events
        /// </remarks>
        /// <returns>An enumerator of thr resuting events</returns>
        /// <param name="hWnd">the WndProc hWnd</param>
        /// <param name="msg">the WndProc msg</param>
        /// <param name="wParam">the WndProc wParam</param>
        /// <param name="lParam">the WndProc lParam</param>
        public static IEnumerable<InteropTouchEventArgs> DecodeMessage(IPointToClient HWndWrapper, int msg, IntPtr wParam, IntPtr lParam, float dpiX, float dpiY)
        {
            // More than one touchinput may be associated with a touch message,
            // so an array is needed to get all event information.
            int inputCount = User32.LoWord((uint)wParam.ToInt32()); // Number of touch inputs, actual per-contact messages

            TOUCHINPUT[] inputs; // Array of TOUCHINPUT structures
            inputs = new TOUCHINPUT[inputCount]; // Allocate the storage for the parameters of the per-contact messages
            try
            {
                // Unpack message parameters into the array of TOUCHINPUT structures, each
                // representing a message for one single contact.
                if (!User32.GetTouchInputInfo(lParam, inputCount, inputs, Marshal.SizeOf(inputs[0])))
                {
                    // Get touch info failed.
                    throw new Exception("Error calling GetTouchInputInfo API");
                }

                // For each contact, dispatch the message to the appropriate message
                // handler.
                // Note that for WM_TOUCHDOWN you can get down & move notifications
                // and for WM_TOUCHUP you can get up & move notifications
                // WM_TOUCHMOVE will only contain move notifications
                // and up & down notifications will never come in the same message
                for (int i = 0; i < inputCount; i++)
                {
                    InteropTouchEventArgs touchEventArgs = new InteropTouchEventArgs(HWndWrapper, dpiX, dpiY, ref inputs[i]);
                    yield return touchEventArgs;
                }
            }
            finally
            {
                User32.CloseTouchInputHandle(lParam);
            }
        }
    }
}
