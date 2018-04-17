using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Blake.NUI.WPF.Common
{
    public static class ScatterMatrixHelper
    {
        public static void UpdateScatterMatrix(FrameworkElement element, Size size, DisplayMatrix matrix)
        {
            UpdateScatterMatrix(element, size, matrix.Center, matrix.Orientation, matrix.Scale);
        }

        public static void UpdateScatterMatrix(FrameworkElement element, Size size, Point center, double orientation, Vector scale)
        {
            Vector offset = CalculateRenderOffset(size, element.RenderTransformOrigin, center, orientation, scale);

            TransformGroup group = element.RenderTransform as TransformGroup;
            if ((group != null) && (group.Children.Count == 3))
            {
                RotateTransform rotateTransform = group.Children[0] as RotateTransform;
                if (rotateTransform != null)
                {
                    rotateTransform.Angle = orientation;
                }
                else
                {
                    group.Children[0] = new RotateTransform(orientation);
                }
                ScaleTransform scaleTransform = group.Children[1] as ScaleTransform;
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = scale.X;
                    scaleTransform.ScaleY = scale.Y;
                }
                else
                {
                    group.Children[1] = new ScaleTransform(scale.X, scale.Y);
                }

                TranslateTransform translateTransform = group.Children[2] as TranslateTransform;
                if (translateTransform != null)
                {
                    translateTransform.X = offset.X;
                    translateTransform.Y = offset.Y;
                }
                else
                {
                    group.Children[2] = new TranslateTransform(offset.X, offset.Y);
                }

            }
            else
            {
                group = new TransformGroup();
                group.Children.Add(new RotateTransform(orientation));
                group.Children.Add(new ScaleTransform(scale.X, scale.Y));
                group.Children.Add(new TranslateTransform(offset.X, offset.Y));
                element.RenderTransform = group;
            }
        }

        /// <summary>
        /// Gets the render offset that should be used to produce the specified center, angle, and scale 
        /// </summary>
        /// <param name="size">The size of the space</param>
        /// <param name="renderTransformOrigin">The normalized center point</param>
        /// <param name="center"></param>
        /// <param name="angle"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Vector CalculateRenderOffset(Size size, Point renderTransformOrigin, Point center, double angle, Vector scale)
        {
            double width = size.Width;
            double height = size.Height;
            Point renderOrigin = GetRenderOrigin(width, height, renderTransformOrigin);
            Matrix renderMatrix = GetRenderMatrix(renderOrigin, new Vector(0.0, 0.0), angle, scale);

            Point transformCenter = new Point(width * 0.5, height * 0.5);
            Point renderedCenter = renderMatrix.Transform(transformCenter);

            return (Vector)(center - renderedCenter);
        }

        /// <summary>
        /// Gets the render origin Point that corresponds to the normalized origin
        /// </summary>
        /// <param name="width">Width of the space</param>
        /// <param name="height">Height of the space</param>
        /// <param name="renderTransformOrigin">Normalized point within the space</param>
        /// <returns>The render origin</returns>
        public static Point GetRenderOrigin(double width, double height, Point renderTransformOrigin)
        {
            return new Point(width * renderTransformOrigin.X, height * renderTransformOrigin.Y);
        }

        /// <summary>
        /// Creates a standard matrix from the offset, rotation, and scale
        /// </summary>
        /// <param name="renderOrigin">Center point of the scale and rotation</param>
        /// <param name="offset">How far to translate the matrix</param>
        /// <param name="rotation">How many degrees to rotate the matrix</param>
        /// <param name="scale">The scale factor, (1,1) resulting in no scaling</param>
        /// <returns>The transformed matrix</returns>
        public static Matrix GetRenderMatrix(Point renderOrigin, Vector offset, double rotation, Vector scale)
        {
            Matrix mx = Matrix.Identity;
            mx.RotateAt(rotation, renderOrigin.X, renderOrigin.Y);
            mx.ScaleAt(scale.X, scale.Y, renderOrigin.X, renderOrigin.Y);
            mx.Translate(offset.X, offset.Y);
            return mx;
        }
    }
}
