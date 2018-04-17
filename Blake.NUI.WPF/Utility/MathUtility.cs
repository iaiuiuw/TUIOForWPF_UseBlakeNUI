using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Blake.NUI.WPF.Utility
{
    public static class MathUtility
    {
        public static bool IsEqualFuzzy(double valueA, double valueB, double eps = 1e-8)
        {
            return Math.Abs(valueA - valueB) < eps;
        }

        public static double MapValue(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            //Normalize
            double ret = (value - fromMin) / (fromMax - fromMin);
            //Resize and translate
            return ret * (toMax - toMin) + toMin;
        }

        public static double Clamp(double value, double min, double max)
        {
            double actualMin = Math.Min(min, max);
            double actualMax = Math.Max(min, max);
            return Math.Min(actualMax, Math.Max(actualMin, value));
        }

        public static double NormalizeAngle(double angle)
        {
            double num = angle % 360.0;
            if (num >= 360.0)
            {
                return (num - 360.0);
            }
            if (num < 0.0)
            {
                num += 360.0;
            }
            return num;
        }

        public static Vector RotateVector(Vector vector, double degrees)
        {
            double a = (degrees * Math.PI) / 180.0;
            double num2 = Math.Sin(a);
            double num3 = Math.Cos(a);
            double x = (vector.X * num3) - (vector.Y * num2);
            return new Vector(x, (vector.X * num2) + (vector.Y * num3));
        }

        //public static Vector RotateVector2(Vector v, double angle)
        //{
        //    double len = v.Length;

        //    if (len == 0)
        //        return v;

        //    double refAngle = Get2DAngle(v, new Vector(1, 0));

        //    double dx = len * Math.Cos((refAngle + angle) * Math.PI / 180.0);
        //    double dy = -len * Math.Sin((refAngle + angle) * Math.PI / 180.0);

        //    return new Vector(dx, dy);
        //}

        public static double Get2DAngle(Vector a, Vector b)
        {
            double cosine = (a.X * b.X + a.Y * b.Y) / (a.Length * b.Length);

            if (cosine > 1)
                cosine = 1;
            else if (cosine < -1)
                cosine = -1;

            if ((a.X * b.Y - a.Y * b.X) < 0)
                return -Math.Acos(cosine) * 180.0 / Math.PI;
            else
                return Math.Acos(cosine) * 180.0 / Math.PI;
        }

    }
}
