using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Blake.NUI.WPF
{
    public abstract class InputFilter : DependencyObject
    {
        #region Static Fields

        private static Dictionary<InputDevice, bool?> ValidDevices = new Dictionary<InputDevice, bool?>();

        #endregion

        #region Attached Properties

        #region Filter

        /// <summary>
        /// The Filter attached property's name.
        /// </summary>
        public const string FilterPropertyName = "Filter";

        /// <summary>
        /// Gets the value of the Filter attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the Filter property of the specified object.</returns>
        public static InputFilter GetFilter(DependencyObject obj)
        {
            return (InputFilter)obj.GetValue(FilterProperty);
        }

        /// <summary>
        /// Sets the value of the Filter attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the Filter value of the specified object.</param>
        public static void SetFilter(DependencyObject obj, InputFilter value)
        {
            obj.SetValue(FilterProperty, value);
        }

        /// <summary>
        /// Identifies the Filter attached property.
        /// </summary>
        public static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached(
            FilterPropertyName,
            typeof(InputFilter),
            typeof(UIElement),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnTouchFilterPropertyChanged)));

        private static void OnTouchFilterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = obj as UIElement;
            if (obj == null)
                return;

            var oldFilter = e.OldValue as InputFilter;
            if (oldFilter != null)
            {
                oldFilter.UnregisterEvents(element);
            }

            var newFilter = e.NewValue as InputFilter;
            if (newFilter != null)
            {
                newFilter.RegisterEvents(element);
            }
        }

        #endregion

        #endregion

        #region Dependency Properties

        #region InvertFilter

        /// <summary>
        /// The <see cref="InvertFilter" /> dependency property's name.
        /// </summary>
        public const string InvertFilterPropertyName = "InvertFilter";

        /// <summary>
        /// Gets or sets the value of the <see cref="InvertFilter" />
        /// property. This is a dependency property.
        /// </summary>
        public bool InvertFilter
        {
            get
            {
                return (bool)GetValue(InvertFilterProperty);
            }
            set
            {
                SetValue(InvertFilterProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="InvertFilter" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty InvertFilterProperty = DependencyProperty.Register(
            InvertFilterPropertyName,
            typeof(bool),
            typeof(InputFilter),
            new UIPropertyMetadata(false));

        #endregion

        #endregion

        #region Constructors

        public InputFilter()
        {

        }

        #endregion

        #region Private Methods

        private static bool? GetIsFilterValid(InputDevice device)
        {
            if (!ValidDevices.ContainsKey(device))
                return null;
            return ValidDevices[device];
        }

        private static void SetIsFilterValid(InputDevice device, bool? value)
        {
            if (!ValidDevices.ContainsKey(device))
            {
                ValidDevices.Add(device, value);
            }
            else
            {
                ValidDevices[device] = value;
            }
        }

        #region Protected Methods

        protected void ProcessEventDeactivation(object sender, InputEventArgs e)
        {
            ProcessEvent(sender, e);
            if (ValidDevices.ContainsKey(e.Device))
            {
                ValidDevices.Remove(e.Device);
            }
        }

        protected void ProcessEvent(object sender, InputEventArgs e)
        {
            bool? wasValid = GetIsFilterValid(e.Device);

            bool isValid = IsDeviceValid(wasValid, e.Device);

            if (InvertFilter)
            {
                isValid = !isValid;
            }

            if (!isValid)
            {
                e.Handled = true;
            }
            
            SetIsFilterValid(e.Device, isValid);
        }

        #endregion

        #endregion

        #region Abstract Methods

        protected abstract bool IsDeviceValid(bool? wasValid, InputDevice touchDevice);

        protected abstract void RegisterEvents(UIElement element);

        protected abstract void UnregisterEvents(UIElement element);
        
        #endregion
    }

}