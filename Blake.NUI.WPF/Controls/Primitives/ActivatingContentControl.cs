using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Blake.NUI.WPF.Controls.Primitives
{
    public enum ActivationStatus
    {
        Inactive,
        Deactivating,
        Active,
        Activating
    }

    public enum EnabledStatus
    {
        Disabled,
        Disabling,
        Enabled,
        Enabling
    }

    public class ActivatingContentControl : ContentControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Properties

        #region ActivationStatus

        public const string ActivationStatusName = "ActivationStatus";

        private ActivationStatus _activationStatus;
        public ActivationStatus ActivationStatus
        {
            get
            {
                return _activationStatus;
            }
            protected set
            {
                if (_activationStatus == value)
                    return;

                ActivationStatus _oldValue = _activationStatus;
                _activationStatus = value;

                RaisePropertyChanged(ActivationStatusName);

                OnActivationStatusChanged(_oldValue, _activationStatus);
            }
        }

        #endregion

        #region IsActive DP

        /// <summary>
        /// The <see cref="IsActive" /> dependency property's name.
        /// </summary>
        public const string IsActivePropertyName = "IsActive";

        /// <summary>
        /// Gets or sets the value of the <see cref="IsActive" />
        /// property. This is a dependency property.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (bool)GetValue(IsActiveProperty);
            }
            set
            {
                this.SetIsActive(value);
            }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            IsActivePropertyName,
            typeof(bool),
            typeof(ActivatingContentControl),
            new UIPropertyMetadata(false, new PropertyChangedCallback(OnIsActivePropertyChanged)));

        private static void OnIsActivePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ActivatingContentControl control = obj as ActivatingContentControl;
            if (control == null)
                return;

            control.SetIsActive((bool)e.NewValue);
        }

        #endregion

        #region IsActiveOrActivating

        public bool IsActiveOrActivating
        {
            get
            {
                return this.ActivationStatus == ActivationStatus.Activating ||
                       this.ActivationStatus == ActivationStatus.Active;
            }
        }

        #endregion

        #region EnabledStatus
        public const string EnabledStatusPropertyName = "EnabledStatus";

        private EnabledStatus _enabledStatus = EnabledStatus.Enabled;

        public EnabledStatus EnabledStatus
        {
            get
            {
                return _enabledStatus;
            }

            protected set
            {
                if (_enabledStatus == value)
                {
                    return;
                }

                var _oldValue = _enabledStatus;
                _enabledStatus = value;
                
                RaisePropertyChanged(EnabledStatusPropertyName);
                
                OnEnabledStatusChanged(_oldValue, _enabledStatus);
            }
        }
        #endregion

        #region IsEnabled DP
        /// <summary>
        /// The <see cref="IsEnabled" /> dependency property's name.
        /// </summary>
        public const string IsEnabledPropertyName = "IsEnabled";

        /// <summary>
        /// Gets or sets the value of the <see cref="IsEnabled" />
        /// property. This is a dependency property.
        /// </summary>
        public new bool IsEnabled
        {
            get
            {
                return (bool)GetValue(IsEnabledProperty);
            }
            set
            {
                SetValue(IsEnabledProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="IsEnabled" /> dependency property.
        /// </summary>
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            IsEnabledPropertyName,
            typeof(bool),
            typeof(ActivatingContentControl),
            new UIPropertyMetadata(true, new PropertyChangedCallback(OnIsEnabledPropertyChanged)));
        
        private static void OnIsEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ActivatingContentControl control = obj as ActivatingContentControl;
            if (control == null)
                return;

            control.SendOnIsEnabledChanged(e);
            control.SetIsEnabled((bool)e.NewValue);
        }

        #endregion

        #region IsEnabledOrEnabling

        public bool IsEnabledOrEnabling
        {
            get
            {
                return this.EnabledStatus == EnabledStatus.Enabled ||
                       this.EnabledStatus == EnabledStatus.Enabling;
            }
        }

        #endregion

        #region IsEnabledCore

        protected override bool IsEnabledCore
        {
            get
            {
                return this.EnabledStatus != EnabledStatus.Disabled;
            }
        }

        #endregion

        #endregion

        #region Events

        #region Activation Events

        #region Activating Event

        public event EventHandler Activating;

        protected void SendOnActivating()
        {
            if (Activating == null)
                return;

            Activating(this, EventArgs.Empty);
        }

        #endregion

        #region Activated Event

        public event EventHandler Activated;

        protected void SendOnActivated()
        {
            if (Activated == null)
                return;

            Activated(this, EventArgs.Empty);
        }
        #endregion
        
        #region Deactivating Event

        public event EventHandler Deactivating;

        protected void SendOnDeactivating()
        {
            if (Deactivating == null)
                return;

            Deactivating(this, EventArgs.Empty);
        }

        #endregion

        #region Deactivated Event

        public event EventHandler Deactivated;

        protected void SendOnDeactivated()
        {
            if (Deactivated == null)
                return;

            Deactivated(this, EventArgs.Empty);
        }

        #endregion

        #endregion

        #region Enabled Events

        #region Enabling Event

        public event EventHandler Enabling;

        protected void SendOnEnabling()
        {
            if (Enabling == null)
                return;
            
            Enabling(this, EventArgs.Empty);
        }

        #endregion

        #region Enabled Event

        public event EventHandler Enabled;
        
        protected void SendOnEnabled()
        {
            if (Enabled == null)
                return;
            Enabled(this, EventArgs.Empty);
        }

        #endregion

        #region Disabling Event

        public event EventHandler Disabling;

        protected void SendOnDisabling()
        {
            if (Disabling == null)
                return;

            Disabling(this, EventArgs.Empty);
        }

        #endregion

        #region Disabled Event

        public event EventHandler Disabled;

        protected void SendOnDisabled()
        {
            if (Disabled == null)
                return;
            
            Disabled(this, EventArgs.Empty);
        }

        #endregion

        #region IsEnabledChanged Event

        public new event DependencyPropertyChangedEventHandler IsEnabledChanged;

        protected void SendOnIsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsEnabledChanged == null)
                return;

            IsEnabledChanged(this, e);
        }

        #endregion

        #endregion

        #endregion

        #region Constructors

        public ActivatingContentControl()
        {
            this.Loaded += new RoutedEventHandler(ActivatingContentControl_Loaded);
        }
        
        #endregion

        #region Public Methods

        public void SetIsActive(bool isActive, bool useTransitions = true, IEasingFunction ease = null)
        {
            if (isActive)
            {
                if (ActivationStatus == Primitives.ActivationStatus.Deactivating ||
                    ActivationStatus == Primitives.ActivationStatus.Inactive)
                {
                    OnActivating(useTransitions, ease);
                }
            }
            else
            {
                if (ActivationStatus == Primitives.ActivationStatus.Activating ||
                    ActivationStatus == Primitives.ActivationStatus.Active)
                {
                    OnDeactivating(useTransitions, ease);
                }
            }
        }

        public void ToggleIsActive(bool useTransitions = true, IEasingFunction ease = null)
        {
            if (ActivationStatus == Primitives.ActivationStatus.Active ||
                ActivationStatus == Primitives.ActivationStatus.Activating)
            {
                OnDeactivating(useTransitions, ease);
            }
            else
            {
                OnActivating(useTransitions, ease);
            }
        }
        
        public void SetIsEnabled(bool isEnabled, bool useTransitions = true, IEasingFunction ease = null)
        {
            if (isEnabled)
            {
                if (EnabledStatus == Primitives.EnabledStatus.Disabling ||
                    EnabledStatus == Primitives.EnabledStatus.Disabled)
                {
                    OnEnabling(useTransitions, ease);
                }
            }
            else
            {
                if (EnabledStatus == Primitives.EnabledStatus.Enabling ||
                    EnabledStatus == Primitives.EnabledStatus.Enabled)
                {
                    OnDisabling(useTransitions, ease);
                }
            }
        }

        #endregion

        #region Protected Methods

        #region Activation Methods

        protected void OnActivating(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.ActivationStatus = ActivationStatus.Activating;
            
            bool activatedHandled = OnActivatingOverride(useTransitions, ease);

            SendOnActivating();

            if (!activatedHandled)
                OnActivated(useTransitions, ease);
        }

        protected void OnActivated(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.ActivationStatus = ActivationStatus.Active;

            OnActivatedOverride(useTransitions, ease);
            SendOnActivated();
        }
        
        protected void OnDeactivating(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.ActivationStatus = ActivationStatus.Deactivating;

            bool deactivatingHandled = OnDeactivatingOverride(useTransitions, ease);

            SendOnDeactivating();

            if (!deactivatingHandled)
                OnDeactivated(useTransitions, ease);
        }

        protected void OnDeactivated(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.ActivationStatus = ActivationStatus.Inactive;
            OnDeactivatedOverride(useTransitions, ease);

            SendOnDeactivated();
        }

        #endregion
        
        #region Enabled Methods

        protected void OnEnabling(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.EnabledStatus = Primitives.EnabledStatus.Enabling;

            bool enabledHandled = OnEnablingOverride(useTransitions, ease);

            SendOnEnabling();

            if (!enabledHandled)
                OnEnabled(useTransitions, ease);
        }

        protected void OnEnabled(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.EnabledStatus = Primitives.EnabledStatus.Enabled;

            OnEnabledOverride(useTransitions, ease);
            SendOnEnabled();
        }

        protected void OnDisabling(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.EnabledStatus = Primitives.EnabledStatus.Disabling;

            bool disablingHandled = OnDisablingOverride(useTransitions, ease);

            SendOnDisabling();

            if (!disablingHandled)
                OnDisabled(useTransitions, ease);
        }

        protected void OnDisabled(bool useTransitions = true, IEasingFunction ease = null)
        {
            this.EnabledStatus = Primitives.EnabledStatus.Disabled;
            OnDisabledOverride(useTransitions, ease);

            SendOnDisabled();
        }

        #endregion

        #endregion

        #region Virtual Methods

        #region Activation Methods

        /// <summary>
        /// Called when the control is activating. Start animations in this method if useTransitions = true, otherwise
        /// set the values directly.
        /// </summary>
        /// <param name="useTransitions"></param>
        /// <param name="ease"></param>
        /// <returns>Return true if you will call OnActivated yourself when the animation is complete. </returns>
        protected virtual bool OnActivatingOverride(bool useTransitions = true, IEasingFunction ease = null)
        {
            return false;
        }

        protected virtual void OnActivatedOverride(bool useTransitions = true, IEasingFunction ease = null)
        {

        }

        /// <summary>
        /// Called when the control is deactivating. Start animations in this method if useTransitions = true, otherwise
        /// set the values directly
        /// </summary>
        /// <param name="useTransitions"></param>
        /// <param name="ease"></param>
        /// <returns>Return true if you will call OnDeactivated yourself when the animation is complete.</returns>
        protected virtual bool OnDeactivatingOverride(bool useTransitions = true, IEasingFunction ease = null)
        {
            return false;
        }

        protected virtual void OnDeactivatedOverride(bool useTransitions = true, IEasingFunction ease = null)
        {

        }

        #endregion
        
        #region Enabled Methods

        /// <summary>
        /// Called when the control is enabling. Start animations in this method if useTransitions = true, otherwise
        /// set the values directly.
        /// </summary>
        /// <param name="useTransitions"></param>
        /// <param name="ease"></param>
        /// <returns>Return true if you will call OnEnabled yourself when the animation is complete. </returns>
        protected virtual bool OnEnablingOverride(bool useTransitions = true, IEasingFunction ease = null)
        {
            return false;
        }

        protected virtual void OnEnabledOverride(bool useTransitions = true, IEasingFunction ease = null)
        {

        }

        /// <summary>
        /// Called when the control is disabling. Start animations in this method if useTransitions = true, otherwise
        /// set the values directly
        /// </summary>
        /// <param name="useTransitions"></param>
        /// <param name="ease"></param>
        /// <returns>Return true if you will call OnDisabled yourself when the animation is complete.</returns>
        protected virtual bool OnDisablingOverride(bool useTransitions = true, IEasingFunction ease = null)
        {
            return false;
        }

        protected virtual void OnDisabledOverride(bool useTransitions = true, IEasingFunction ease = null)
        {

        }

        #endregion

        #endregion

        #region Private Methods

        private void OnActivationStatusChanged(ActivationStatus _oldValue, ActivationStatus _activationStatus)
        {
            if (_activationStatus == Primitives.ActivationStatus.Active)
                base.SetValue(IsActiveProperty, true);
            else if (_activationStatus == Primitives.ActivationStatus.Inactive)
                base.SetValue(IsActiveProperty, false);
        }

        private void OnEnabledStatusChanged(EnabledStatus _oldValue, EnabledStatus _activationStatus)
        {
            if (_enabledStatus == Primitives.EnabledStatus.Enabled)
                base.SetValue(IsEnabledProperty, true);
            else if (_enabledStatus == Primitives.EnabledStatus.Disabled)
                base.SetValue(IsEnabledProperty, false);
        }

        void ActivatingContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsActiveOrActivating)
                OnActivating(false);
            else
                OnDeactivating(false);

            if (this.IsEnabledOrEnabling)
                OnEnabling(false);
            else
                OnDisabling(false);
        }

        #endregion
    }
    
}
 