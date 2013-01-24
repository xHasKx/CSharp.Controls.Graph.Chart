using System;
using System.Drawing;

namespace HasK.Controls.Graph
{
    /// <summary>
    /// Graphical 2D point with double coordinates
    /// </summary>
    public struct DPoint
    {
        /// <summary>
        /// The X-coordinate of point
        /// </summary>
        public double X;
        /// <summary>
        /// The Y-coordinate of point
        /// </summary>
        public double Y;
        /// <summary>
        /// Create graphical 2D point 
        /// </summary>
        /// <param name="x">The X-coordinate of point</param>
        /// <param name="y">The Y-coordinate of point</param>
        public DPoint(double x, double y) { X = x; Y = y; }
        /// <summary>
        /// Convert DPoint object to string representation
        /// </summary>
        public override string ToString()
        {
            return String.Format("X: {0}; Y: {1}", X, Y);
        }
        /// <summary>
        /// Checks if point in rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool InRect(DRect rect)
        {
            if (X >= rect.X && X <= rect.X + rect.Width && Y >= rect.Y && Y <= rect.Y + rect.Height)
                return true;
            return false;
        }
    }
    /// <summary>
    /// Graphical 2D size with double width and height
    /// </summary>
    public struct DSize
    {
        /// <summary>
        /// Width
        /// </summary>
        public double Width;
        /// <summary>
        /// Height
        /// </summary>
        public double Height;
        /// <summary>
        /// Create graphical 2D size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public DSize(double width, double height) { Width = width; Height = height; }
        /// <summary>
        /// Convert DSize object to string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("W: {0}; H: {1}", Width, Height);
        }
    }
    /// <summary>
    /// Graphical 2D rectangle with double coordinates
    /// </summary>
    public struct DRect
    {
        /// <summary>
        /// X coordinate of left-top corner
        /// </summary>
        public double X;
        /// <summary>
        /// Y coordinate of left-top corner
        /// </summary>
        public double Y;
        /// <summary>
        /// Width of rectangle
        /// </summary>
        public double Width;
        /// <summary>
        /// Height of rectangle
        /// </summary>
        public double Height;
        /// <summary>
        /// Create graphical 2D rectangle
        /// </summary>
        /// <param name="x">X coordinate of left-top corner</param>
        /// <param name="y">Y coordinate of left-top corner</param>
        /// <param name="width">Width of rectangle</param>
        /// <param name="height">Height of rectangle</param>
        public DRect(double x, double y, double width, double height)
        {
            X = x; Y = y; Width = width; Height = height;
        }
        /// <summary>
        /// Convert DRect object to string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("X: {0}; Y: {1}; W: {2}; H: {3}", X, Y, Width, Height);
        }
    }
}