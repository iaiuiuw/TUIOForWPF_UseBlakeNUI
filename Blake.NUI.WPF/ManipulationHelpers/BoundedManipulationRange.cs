using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blake.NUI.WPF.Utility;

namespace Blake.NUI.WPF.ManipulationHelpers
{
    public class BoundedManipulationRange
    {
        #region Fields

        private bool IsDirty { get; set; }

        #endregion

        #region Properties

        #region LowerBoundary

        private double _lowerBoundary = 0;
        public double LowerBoundary
        {
            get
            {
                return _lowerBoundary;
            }
            set
            {
                if (_lowerBoundary == value)
                    return;

                _lowerBoundary = value;

                IsDirty = true;
            }
        }

        #endregion

        #region UpperBoundary

        private double _upperBoundary = 1;
        public double UpperBoundary
        {
            get
            {
                return _upperBoundary;
            }
            set
            {
                if (_upperBoundary == value)
                    return;
                
                _upperBoundary = value;

                IsDirty = true;
            }
        }

        #endregion

        #region ElasticMargin

        private double _elasticMargin = 40;

        public double ElasticMargin
        {
            get
            {
                return _elasticMargin;
            }
            set
            {
                if (_elasticMargin == value)
                    return;

                _elasticMargin = value;

                IsDirty = true;
            }
        }

        #endregion

        #region Position

        private double _position = 0;
        public double Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value)
                    return;

                _position = value;

                IsDirty = true;
            }
        }

        #endregion

        #region BoundedPosition

        private double _boundedPosition = 0;
        public double BoundedPosition
        {
            get
            {
                VerifyValues();
                return _boundedPosition;
            }
            private set
            {
                if (_boundedPosition == value)
                    return;

                _boundedPosition = value;
            }
        }

        #endregion

        #region ElasticOffset

        private double _elasticOffset = 0;
        public double ElasticOffset
        {
            get
            {
                VerifyValues();
                return _elasticOffset;
            }
        }

        #endregion

        #region BoundsOverflow

        private double _boundaryOverflow = 0;
        public double BoundaryOverflow
        {
            get
            {
                VerifyValues();
                return _boundaryOverflow;
            }
        }

        #endregion

        #region IsPositionBelowLowerElasticMargin

        public bool IsPositionBelowLowerElasticMargin
        {
            get
            {
                return Position < LowerBoundary - ElasticMargin;
            }
        }

        #endregion

        #region IsPositionAboveUpperElasticMargin

        public bool IsPositionAboveUpperElasticMargin
        {
            get
            {
                return Position > UpperBoundary + ElasticMargin;
            }
        }

        #endregion
        
        #region IsPositionBelowLowerBoundary

        public bool IsPositionBelowLowerBoundary
        {
            get
            {
                return Position < LowerBoundary;
            }
        }

        #endregion

        #region IsPositionAboveUpperBoundary

        public bool IsPositionAboveUpperBoundary
        {
            get
            {
                return Position > UpperBoundary;
            }
        }

        #endregion

        #region IsPositionOutOfBounds

        public bool IsPositionOutOfBounds
        {
            get
            {
                return IsPositionAboveUpperBoundary ||
                       IsPositionBelowLowerBoundary;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void VerifyValues()
        {
            if (!IsDirty)
                return;

            IsDirty = false;
            
            if (Position < LowerBoundary)
            {
                _boundaryOverflow = Position - LowerBoundary;
            }
            else if (Position > UpperBoundary)
            {
                _boundaryOverflow = Position - UpperBoundary;
            }
            else
            {
                _boundaryOverflow = 0;
            }
            
            _elasticOffset = GetRubberStretch(BoundaryOverflow, ElasticMargin);

            _boundedPosition = MathUtility.Clamp(Position, LowerBoundary, UpperBoundary);
        }

        private static double GetRubberStretch(double value, double maxDisplacement)
        {
            if (value == 0 && maxDisplacement == 0)
                return 0;
            double x = Math.Abs(value);
            return Math.Sign(value) * maxDisplacement * x / (x + maxDisplacement);           
        }        

        #endregion
    }
}
