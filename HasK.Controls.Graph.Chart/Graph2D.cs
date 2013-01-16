using System;
using System.Drawing;

namespace HasK.Controls.Graph
{
    /// <summary>
    /// Graphical 2D point with double coordinates
    /// </summary>
    public struct DPoint
    {
        public double X, Y;

        public DPoint(double x, double y) { X = x; Y = y; }

        public override string ToString()
        {
            return String.Format("<DPoint {0}:{1}>", X, Y);
        }

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
        public double Width, Height;

        public DSize(double width, double height) { Width = width; Height = height; }

        public override string ToString()
        {
            return String.Format("<DSize {0}:{1}>", Width, Height);
        }
    }

    /// <summary>
    /// Graphical 2D rectangle with double coordinates
    /// </summary>
    public struct DRect
    {
        public double X, Y, Width, Height;

        public DRect(double x, double y, double width, double height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public override string ToString()
        {
            return String.Format("<DRect {0}:{1} {2}x{3}>", X, Y, Width, Height);
        }
    }
}