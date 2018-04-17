using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Blake.NUI.WPF.Utility
{
    public class AnimateUtility
    {
        #region Private Class Members
        protected struct DependencyCombo
        {
            private DependencyObject dObject;
            public DependencyObject DObject
            {
                get
                {
                    return dObject;
                }
            }

            private DependencyProperty dProperty;
            public DependencyProperty DProperty
            {
                get
                {
                    return dProperty;
                }
            }

            public DependencyCombo(DependencyObject dObject, DependencyProperty dProperty)
            {
                this.dObject = dObject;
                this.dProperty = dProperty;
            }

            public bool Equals(DependencyCombo other)
            {
                if (this.dObject == other.dObject &&
                    this.dProperty == other.dProperty)
                    return true;
                return false;
            }
        };

        protected struct AnimationData
        {
            private AnimationClock clock;
            public AnimationClock Clock
            {
                get
                {
                    return clock;
                }
            }

            private DependencyCombo dependencyCombo;
            public DependencyCombo DependencyCombo
            {
                get
                {
                    return dependencyCombo;
                }
            }

            public AnimationData(AnimationClock clock, DependencyCombo dependencyCombo)
            {
                this.clock = clock;
                this.dependencyCombo = dependencyCombo;
            }
        };

        protected static Dictionary<AnimationClock, DependencyCombo> RunningAnimation = new Dictionary<AnimationClock, DependencyCombo>();

        protected static Dictionary<DispatcherTimer, AnimationData> SavedAnimations = new Dictionary<DispatcherTimer, AnimationData>();

        #endregion

        #region Public Static Methods

        public static void StopAnimation(DependencyObject element, DependencyProperty property)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }

            IAnimatable animatable = (IAnimatable)element;

            object value = element.GetValue(property);
            animatable.BeginAnimation(property, null);
            element.SetValue(property, value);
        }

        public static AnimationClock AnimateElementDouble(DependencyObject element, DependencyProperty property, double targetValue, double fromTime, double toTime, IEasingFunction ease = null)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }

            if (double.IsNaN(targetValue))
                throw new ArgumentException("targetValue cannot be NaN", "targetValue");
            
            StopAnimation(element, property);
            
            if (ease == null)
            {
                ease = new CircleEase();
            }

            double initialValue = (double)element.GetValue(property);

            if (double.IsNaN(initialValue))
                initialValue = 0.0;

            DoubleAnimation anim = new DoubleAnimation(initialValue, targetValue, new Duration(TimeSpan.FromSeconds(toTime - fromTime)));
            //DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();
            
            //anim.KeyFrames.Add(new SplineDoubleKeyFrame(initialValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(fromTime))));
            //anim.KeyFrames.Add(new SplineDoubleKeyFrame(targetValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(toTime))
            //                            , new KeySpline(0.0, 0, 0.05, 1))
            //                         );
            
            anim.EasingFunction = ease;

            if (fromTime > 0.0)
            {
                return AnimateElementDelayHelper(element, property, anim, fromTime);
            }
            else
            {
                return AnimateElementHelper(element, property, anim);
            }
        }
        
        public static AnimationClock AnimateElementPoint(DependencyObject element, DependencyProperty property, Point targetValue, double fromTime, double toTime, IEasingFunction ease = null)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }
            
            StopAnimation(element, property);
            
            if (ease == null)
            {
                ease = new CircleEase();
            }

            Point initialValue = (Point)element.GetValue(property);

            PointAnimation anim = new PointAnimation(initialValue, targetValue, new Duration(TimeSpan.FromSeconds(toTime - fromTime)));

            anim.EasingFunction = ease;

            //PointAnimationUsingKeyFrames anim = new PointAnimationUsingKeyFrames();

            //anim.KeyFrames.Add(new SplinePointKeyFrame(initialValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(fromTime))));
            //anim.KeyFrames.Add(new SplinePointKeyFrame(targetValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(toTime))
            //                            , new KeySpline(0.0, 0, 0.05, 1))
            //                         );

            if (fromTime > 0.0)
            {
                return AnimateElementDelayHelper(element, property, anim, fromTime);
            }
            else
            {
                return AnimateElementHelper(element, property, anim);
            }
        }
        
        public static AnimationClock AnimateElementVector(DependencyObject element, DependencyProperty property, Vector targetValue, double fromTime, double toTime, IEasingFunction ease = null)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }

            StopAnimation(element, property);
            
            if (ease == null)
            {
                ease = new CircleEase();
            }

            Vector initialValue = (Vector)element.GetValue(property);

            VectorAnimation anim = new VectorAnimation(initialValue, targetValue, new Duration(TimeSpan.FromSeconds(toTime - fromTime)));

            anim.EasingFunction = ease;
            //VectorAnimationUsingKeyFrames anim = new VectorAnimationUsingKeyFrames();

            //anim.KeyFrames.Add(new SplineVectorKeyFrame(initialValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(fromTime))));
            //anim.KeyFrames.Add(new SplineVectorKeyFrame(targetValue,
            //                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(toTime))
            //                            , new KeySpline(0.0, 0, 0.05, 1))
            //                         );

            if (fromTime > 0.0)
            {
                return AnimateElementDelayHelper(element, property, anim, fromTime);
            }
            else
            {
                return AnimateElementHelper(element, property, anim);
            }
        }

        #endregion

        #region Private Methods

        private static AnimationClock AnimateElementDelayHelper(DependencyObject element, DependencyProperty property, AnimationTimeline anim, double delaySeconds)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }
            
            anim.FillBehavior = FillBehavior.HoldEnd;

            anim.Completed += new EventHandler(anim_Completed);

            //anim.AccelerationRatio = .15;
            //anim.DecelerationRatio = .85;

            //object value = element.GetValue(property);
            //animatable.BeginAnimation(property, null);
            //element.SetValue(property, value);

            AnimationClock clock = anim.CreateClock();
            DependencyCombo dc = new DependencyCombo(element, property);

            DispatcherTimer delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromSeconds(delaySeconds);
            delayTimer.Tick +=new EventHandler(delayTimer_Tick);
            delayTimer.Start();

            SavedAnimations.Add(delayTimer, new AnimationData(clock, dc));

            return clock;
        }

        private static void delayTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null)
            {
                AnimationData data;
                if (SavedAnimations.TryGetValue(timer, out data))
                {
                    SavedAnimations.Remove(timer);
                    timer.Stop();
                    timer.Tick -= delayTimer_Tick;

                    AnimationClock clock = data.Clock;
                    DependencyCombo dc = data.DependencyCombo;

                    IAnimatable animatable = (IAnimatable)dc.DObject;
                    animatable.ApplyAnimationClock(dc.DProperty, null);
                    animatable.ApplyAnimationClock(dc.DProperty, clock);

                    clock.Controller.Begin();

                    if (RunningAnimation.Values.Contains(dc))
                    {
                        List<AnimationClock> toDelete = new List<AnimationClock>();

                        foreach (KeyValuePair<AnimationClock, DependencyCombo> kvp in RunningAnimation)
                        {
                            if (kvp.Value.Equals(dc))
                            {
                                toDelete.Add(kvp.Key);
                                kvp.Key.Controller.Stop();
                            }
                        }

                        foreach (AnimationClock key in toDelete)
                        {
                            RunningAnimation.Remove(key);
                        }
                    }

                    RunningAnimation.Add(clock, dc);

                }
            }
        }

        private static AnimationClock AnimateElementHelper(DependencyObject element, DependencyProperty property, AnimationTimeline anim)
        {
            if (!(element is IAnimatable))
            {
                throw new InvalidOperationException("Element must be IAnimatable.");
            }

            anim.FillBehavior = FillBehavior.HoldEnd;

            anim.Completed += new EventHandler(anim_Completed);

            //anim.AccelerationRatio = .15;
            //anim.DecelerationRatio = .85;

            AnimationClock clock = anim.CreateClock();
            DependencyCombo dc = new DependencyCombo(element, property);

            IAnimatable animatable = (IAnimatable)dc.DObject;
            animatable.ApplyAnimationClock(dc.DProperty, null);
            animatable.ApplyAnimationClock(dc.DProperty, clock);
                        
            if (RunningAnimation.Values.Contains(dc))
            {
                List<AnimationClock> toDelete = new List<AnimationClock>();

                foreach (KeyValuePair<AnimationClock, DependencyCombo> kvp in RunningAnimation)
                {
                    if (kvp.Value.Equals(dc))
                    {
                        toDelete.Add(kvp.Key);
                        kvp.Key.Controller.Stop();
                    }
                }

                foreach (AnimationClock key in toDelete)
                {
                    RunningAnimation.Remove(key);
                }
            }
            
            RunningAnimation.Add(clock, dc);

            return clock;
        }

        private static void anim_Completed(object sender, EventArgs e)
        {
            AnimationClock clock = sender as AnimationClock;
            if (clock != null)
            {
                DependencyCombo combo;
                if (RunningAnimation.TryGetValue(clock, out combo))
                {
                    RunningAnimation.Remove(clock);

                    if (!(combo.DObject is IAnimatable))
                    {
                        throw new InvalidOperationException("Element must be IAnimatable.");
                    }

                    StopAnimation(combo.DObject, combo.DProperty);

                }
            }
        }

        #endregion

        protected AnimateUtility()
        {
        }
    }
}
