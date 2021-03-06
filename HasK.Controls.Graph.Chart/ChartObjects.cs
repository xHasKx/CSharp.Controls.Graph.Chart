﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace HasK.Controls.Graph
{
    # region Base classes and interfaces for all on-chart object
    /// <summary>
    /// Interface which should be implemented by any object on chart
    /// </summary>
    public abstract class ChartObject
    {
        internal int _z_index = 0;
        /// <summary>
        /// If this flag will be in object's flags, then chart will not call Draw object
        /// </summary>
        public const uint Invisible         = 0x00000001;
        /// <summary>
        /// If this flag will be in object's flags, then chart will cast object to IChartSelectableObject to determine selected object
        /// </summary>
        public const uint Rectangle         = 0x00000002;
        /// <summary>
        /// If this flag will be in object's flags, then chart will cast object to IChartMouseMovableObject for moving object by mouse
        /// </summary>
        public const uint MouseMovable      = 0x00000004;
        /// <summary>
        /// Stores bit-flags capabilities of object
        /// </summary>
        public uint Flags                   = 0;
        /// <summary>
        /// The chart which contains this object
        /// </summary>
        public Chart Chart { get; private set; }
        /// <summary>
        /// Z-index of object on chart
        /// </summary>
        public int ZIndex
        {
            get
            {
                return _z_index;
            }
            set
            {
                _z_index = value;
                Chart.UpdateZIndex(this, value);
            }
        }
        /// <summary>
        /// Base constructor for all on-chart objects
        /// </summary>
        /// <param name="chart">The chart on which object should be displayed</param>
        public ChartObject(Chart chart)
        {
            Chart = chart;
        }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public abstract void Draw(Graphics g);
    }
    /// <summary>
    /// Base class for all visible objects
    /// </summary>
    public abstract class ChartVisibleObject : ChartObject
    {
        /// <summary>
        /// The color value of Color property
        /// </summary>
        protected Color _color;
        /// <summary>
        /// The pen with value of Color property
        /// </summary>
        protected Pen _pen;
        /// <summary>
        /// The solid brush with value of Color property
        /// </summary>
        protected Brush _brush;
        /// <summary>
        /// The fill brush of object
        /// </summary>
        protected Brush _fill;
        /// <summary>
        /// Determine if object should be filled
        /// </summary>
        protected bool _is_filled = false;
        /// <summary>
        /// Gets or sets color of object
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    _pen = new Pen(_color);
                    _brush = new SolidBrush(_color);
                }
            }
        }
        /// <summary>
        /// Gets or sets object's fill brush 
        /// </summary>
        public Brush FillBrush
        {
            get
            {
                return _fill;
            }
            set
            {
                _fill = value;
            }
        }
        /// <summary>
        /// Gets or sets object's filled flag
        /// </summary>
        public bool IsFilled
        {
            get
            {
                return _is_filled;
            }
            set
            {
                _is_filled = value;
            }
        }
        /// <summary>
        /// Gets or sets the width of pen for all drawing lines
        /// </summary>
        public float PenWidth
        {
            get
            {
                if (_pen != null)
                    return _pen.Width;
                return 0;
            }
            set
            {
                if (_pen != null)
                    _pen.Width = value;
            }
        }
        /// <summary>
        /// Create base class for all visible objects
        /// </summary>
        /// <param name="chart"></param>
        public ChartVisibleObject(Chart chart) : base(chart) { }
    }
    /// <summary>
    /// Base class for all visible rect-like objects
    /// </summary>
    public abstract class ChartRectObject : ChartVisibleObject
    {
        /// <summary>
        /// Gets or sets the point of left-bottom corner of rectangle
        /// </summary>
        public DPoint LeftBottom { get; set; }
        /// <summary>
        /// Gets or sets the size of rectangle
        /// </summary>
        public DSize Size { get; set; }
        /// <summary>
        /// Gets the center point of movable object
        /// </summary>
        public DPoint Center
        {
            get
            {
                return new DPoint(LeftBottom.X + Size.Width / 2, LeftBottom.Y + Size.Height / 2);
            }
            set
            {
                LeftBottom = new DPoint(value.X - Size.Width / 2, value.Y - Size.Height / 2);
            }
        }
        /// <summary>
        /// Gets or sets text on ellipse
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the brush of text on object
        /// </summary>
        public Brush TextBrush { get; set; }
        /// <summary>
        /// Gets or sets font of text on object
        /// </summary>
        public Font Font { get; set; }
        /// <summary>
        /// Create base class for all rect-like objects
        /// </summary>
        /// <param name="chart"></param>
        public ChartRectObject(Chart chart) : base(chart)
        {
            Flags |= ChartObject.Rectangle;
            Font = new Font("Tahoma", 10);
            LeftBottom = new DPoint();
            Size = new DSize();
            TextBrush = new SolidBrush(Color.Black);
        }
        /// <summary>
        /// Draw text on the center of rect object
        /// </summary>
        /// <param name="g">Graphics object to draw string</param>
        /// <param name="text">Text to draw on object</param>
        protected virtual void DrawStringCenter(Graphics g, string text)
        {
            if (text != null && text != string.Empty)
            {
                var c = Chart.ToScreenPoint(Center);
                var tsz = g.MeasureString(text, Font);
                g.DrawString(text, Font, TextBrush, c.X - tsz.Width / 2, c.Y - tsz.Height / 2);
            }
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public abstract DRect GetRealRect(Graphics g, double view_scale);
        /// <summary>
        /// Gets the rectangle of object in screen coordinates
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public virtual RectangleF GetScreenRect(Graphics g, double view_scale)
        {
            var rr = GetRealRect(g, view_scale);
            var lb = Chart.ToScreenPoint(new DPoint(rr.X, rr.Y));
            var sz = Chart.ToScreenSize(new DSize(rr.Width, rr.Height));
            return new RectangleF(lb.X, lb.Y, sz.Width, sz.Height);
        }
    }
    /// <summary>
    /// Interface for all mouse moveable on-chart objects
    /// </summary>
    public interface IChartMouseMovableObject
    {
        /// <summary>
        /// Gets the center point of movable object
        /// </summary>
        DPoint Center { get; }
        /// <summary>
        /// This method will be called by chart when mouse was moved on chart
        /// </summary>
        /// <param name="point">Stores the point of mouse cursor tied to grid</param>
        void MoveTo(DPoint point);
    }
    # endregion
    # region Concrete on-chart objects classes
    /// <summary>
    /// Simple square point on chart with specified color and size
    /// </summary>
    public class ChartPoint : ChartRectObject
    {
        /// <summary>
        /// Gets or sets the mode of point - in screen or in real coordinates
        /// </summary>
        public bool ScreenSizeMode { get; set; }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        /// <param name="color">The color of this point</param>
        /// <param name="size">The size of this point, in screen coordinates</param>
        public ChartPoint(Chart chart, DPoint center, Color color, DSize size)
            : base(chart)
        {
            ScreenSizeMode = true;
            Size = size;
            Center = center;
            Color = color;
        }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        public ChartPoint(Chart chart)
            : this(chart, new DPoint(), Color.Black, new DSize(8, 8)) { }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        /// <param name="color">The color of this point</param>
        public ChartPoint(Chart chart, DPoint center, Color color)
            : this(chart, center, color, new DSize(8, 8)) { }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        public ChartPoint(Chart chart, DPoint center)
            : this(chart, center, Color.Black, new DSize(8, 8)) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var rp = Chart.ToScreenPoint(Center);
            SizeF sz;
            if (ScreenSizeMode)
                sz = new SizeF((float)Size.Width, (float)Size.Height);
            else
                sz = Chart.ToScreenSize(Size);
            g.FillRectangle(_brush, rp.X - sz.Width / 2, rp.Y - sz.Height / 2, sz.Width, sz.Height);
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public override DRect GetRealRect(Graphics g, double view_scale)
        {
            return new DRect(Center.X - Size.Width / 2, Center.Y - Size.Height / 2, Size.Width, Size.Height);
        }
        /// <summary>
        /// Gets the rectangle of object in screen coordinates
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public override RectangleF GetScreenRect(Graphics g, double view_scale)
        {
            var c = Chart.ToScreenPoint(Center);
            var sz = Chart.ToScreenSize(Size);
            return new RectangleF(c.X - sz.Width / 2, c.Y - sz.Height / 2, sz.Width, sz.Height);
        }
    }
    /// <summary>
    /// One-pixel line on chart with specified points of begin, end, and specified color
    /// </summary>
    public class ChartLine : ChartVisibleObject
    {
        /// <summary>
        /// Gets or sets the begin point of line
        /// </summary>
        public DPoint Begin { get; set; }
        /// <summary>
        /// Gets or sets the end point of line
        /// </summary>
        public DPoint End { get; set; }
        /// <summary>
        /// Create line on chart
        /// </summary>
        /// <param name="chart">Chart for line</param>
        /// <param name="begin">Begin point of line</param>
        /// <param name="end">End point of line</param>
        public ChartLine(Chart chart, DPoint begin, DPoint end)
            : base(chart)
        {
            Begin = begin;
            End = end;
            Color = Color.Black;
        }
        /// <summary>
        /// Create line on chart
        /// </summary>
        /// <param name="chart">Chart for line</param>
        public ChartLine(Chart chart) : this(chart, new DPoint(), new DPoint()) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var begin = Chart.ToScreenPoint(Begin);
            var end = Chart.ToScreenPoint(End);
            g.DrawLine(_pen, begin, end);
        }
    }
    /// <summary>
    /// One-pixel line on chart with specified points of begin, end, color and text
    /// </summary>
    public class ChartTextLine : ChartLine
    {
        /// <summary>
        /// Text on line
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Font of text on line
        /// </summary>
        public Font Font { get; set; }
        /// <summary>
        /// Brush of text on line
        /// </summary>
        public Brush TextBrush { get; set; }
        /// <summary>
        /// Brush of text background on line
        /// </summary>
        public Brush TextBackgroundBrush { get; set; }
        /// <summary>
        /// Create text line on chart
        /// </summary>
        /// <param name="chart">Chart for line</param>
        /// <param name="begin">Begin point of line</param>
        /// <param name="end">End point of line</param>
        /// <param name="text">Text on line</param>
        public ChartTextLine(Chart chart, DPoint begin, DPoint end, string text)
            : base(chart, begin, end)
        {
            Text = text;
            Font = new Font("Times New Roman", 10);
            TextBrush = new SolidBrush(Color.Black);
            TextBackgroundBrush = new SolidBrush(Color.White);
        }
        /// <summary>
        /// Create text line on chart
        /// </summary>
        /// <param name="chart">Chart for line</param>
        /// <param name="text">Text on line</param>
        public ChartTextLine(Chart chart, string text) : base(chart)
        {
            Text = text;
            Font = new Font("Times New Roman", 10);
            TextBrush = new SolidBrush(Color.Black);
            TextBackgroundBrush = new SolidBrush(Color.White);
        }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            base.Draw(g);
            if (!String.IsNullOrEmpty(Text) && Font != null && TextBrush != null)
            {
                var center = Chart.ToScreenPoint(new DPoint(Begin.X + (End.X - Begin.X) / 2.0, Begin.Y + (End.Y - Begin.Y) / 2.0));
                var sz = g.MeasureString(Text, Font);
                var p = new PointF(center.X - sz.Width / 2.0f, center.Y - sz.Height / 2.0f);
                if (TextBackgroundBrush != null)
                    g.FillRectangle(TextBackgroundBrush, new RectangleF(p, sz));
                g.DrawString(Text, Font, TextBrush, p);
            }
        }
    }
    /// <summary>
    /// Simple square point on chart with specified color and text near it
    /// </summary>
    public class ChartTextPoint : ChartPoint
    {
        /// <summary>
        /// Text placement type
        /// </summary>
        public enum TextPlaceType
        {
            /// <summary>
            /// Under the point
            /// </summary>
            Under,
            /// <summary>
            /// On the left of the point
            /// </summary>
            Left,
            /// <summary>
            /// On top of the point
            /// </summary>
            Top,
            /// <summary>
            /// On the right of the point
            /// </summary>
            Right
        }
        /// <summary>
        /// Gets or sets text placement type of text near the point
        /// </summary>
        public TextPlaceType TextPlace { get; set; }
        /// <summary>
        /// Create simple square point on chart with specified color and text near it 
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="text">Text near the point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        /// <param name="place_type">Text placement type of text near the point</param>
        public ChartTextPoint(Chart chart, string text, DPoint center, TextPlaceType place_type)
            : base(chart, center)
        {
            Flags = 0;
            Text = text;
            Color = Color.Black;
            TextPlace = place_type;
        }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var pos = Chart.ToScreenPoint(Center);
            PointF text_pos;
            var text_size = g.MeasureString(Text, Font);

            var sz = Chart.ToScreenSize(Size);
            var dszx = sz.Width / 2;
            var dszy = sz.Height / 2;

            if (TextPlace == TextPlaceType.Under)
                text_pos = new PointF(pos.X - text_size.Width / 2, pos.Y + 2 + dszy);
            else if (TextPlace == TextPlaceType.Left)
                text_pos = new PointF(pos.X - 2 - dszx - text_size.Width, pos.Y - text_size.Height / 2);
            else if (TextPlace == TextPlaceType.Top)
                text_pos = new PointF(pos.X - text_size.Width / 2, pos.Y - 2 - dszy - text_size.Height);
            else if (TextPlace == TextPlaceType.Right)
                text_pos = new PointF(pos.X + 2 + dszx, pos.Y - text_size.Height / 2);
            else
                return;
            base.Draw(g);
            g.DrawString(Text, Font, _brush, text_pos);
        }
    }
    /// <summary>
    /// Math function on chart
    /// </summary>
    public class ChartFunction : ChartVisibleObject
    {
        /// <summary>
        /// Delegate for math function
        /// </summary>
        /// <param name="x">Input parameter - X value</param>
        /// <returns>Returns function(x)</returns>
        public delegate double MathFunction(double x);
        /// <summary>
        /// The function which should be drawed
        /// </summary>
        protected MathFunction _func = null;
        /// <summary>
        /// The function which should be displayed on chart
        /// </summary>
        public MathFunction Function
        {
            get
            {
                return _func;
            }
            set
            {
                _func = value;
            }
        }
        /// <summary>
        /// Create math function on chart
        /// </summary>
        /// <param name="chart">Chart for function</param>
        /// <param name="func">The math function for display</param>
        /// <param name="color">Color of function</param>
        public ChartFunction(Chart chart, MathFunction func, Color color)
            : base(chart)
        {
            Function = func;
            Color = color;
        }
        /// <summary>
        /// Create math function on chart
        /// </summary>
        /// <param name="chart">Chart for function</param>
        /// <param name="func">The math function for display</param>
        public ChartFunction(Chart chart, MathFunction func)
            : base(chart)
        {
            Function = func;
            Color = Color.Black;
        }
        /// <summary>
        /// Gets or sets the option for extending function on chart in the extremum point to +/- infinite
        /// </summary>
        public bool ExtendOnExtremum { get; set; }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            if (_func == null)
                return;
            var start_real_point = Chart.ToRealPoint(new Point(0, 0));
            var end_real_point = Chart.ToRealPoint(new Point(Chart.Width, 0));
            var next_real_point = Chart.ToRealPoint(new Point(1, 0));
            var bottom_real_point = Chart.ToRealPoint(new Point(0, Chart.Height));
            var dx = next_real_point.X - start_real_point.X;
            var start_x = start_real_point.X;
            var end_x = end_real_point.X;

            double x = start_x, y, dy, last_y, deriv = 0, last_deriv, aderiv, alast_deriv, dd_rel;
            bool extremum = false, max_min = false;
            int row_extremums = 0;

            var extend = ExtendOnExtremum;

            y = _func(x);
            var last_p = Chart.ToScreenPoint(new DPoint(x, y));
            var cnt = 0;
            while (x <= end_x)
            {
                cnt++;
                x += dx;
                last_y = y;
                y = _func(x);

                dy = y - last_y;
                last_deriv = deriv;
                deriv = dy / dx;

                extremum = false;
                if (last_deriv != 0 && Math.Sign(deriv) != Math.Sign(last_deriv))
                {
                    if (last_deriv > 0)
                        max_min = true;
                    else
                        max_min = false;
                    aderiv = Math.Abs(deriv);
                    alast_deriv = Math.Abs(last_deriv);
                    dd_rel = 0d;
                    if (aderiv > alast_deriv)
                        dd_rel = aderiv / alast_deriv;
                    else
                        dd_rel = alast_deriv / aderiv;
                    if (dd_rel < 500.0) // TODO: reimplement extremum's check
                        extremum = true;
                    if (extremum && Math.Abs(dy) < 0.1)
                    {
                        extremum = false;
                    }
                }
                
                var current_p = Chart.ToScreenPoint(new DPoint(x, y));
                if (!extremum || row_extremums != 0)
                {
                    g.DrawLine(_pen, last_p, current_p);
                    row_extremums = 0;
                }
                else
                {
                    row_extremums += 1;
                    if (extend)
                    {
                        if (max_min)
                        {
                            g.DrawLine(_pen, current_p, new PointF(current_p.X, Chart.Height));
                            g.DrawLine(_pen, new PointF(last_p.X, 0), last_p);
                        }
                        else
                        {
                            g.DrawLine(_pen, last_p, new PointF(last_p.X, Chart.Height));
                            g.DrawLine(_pen, new PointF(current_p.X, 0), current_p);
                        }
                    }
                }
                last_p = current_p;
            }
        }
    }
    /// <summary>
    /// On-chart rectangle
    /// </summary>
    public class ChartRectangle : ChartRectObject, IChartMouseMovableObject
    {
        /// <summary>
        /// Create the on-chart rectangle
        /// </summary>
        /// <param name="chart">Chart for rectangle</param>
        /// <param name="left_bottom">The left-bottom corner point</param>
        /// <param name="size">The size of rectangle</param>
        /// <param name="color">The color of rectangle</param>
        public ChartRectangle(Chart chart, DPoint left_bottom, DSize size, Color color)
            : base(chart)
        {
            Flags = ChartObject.Rectangle | ChartObject.MouseMovable;
            LeftBottom = left_bottom;
            Size = size;
            Color = color;
        }
        /// <summary>
        /// Create the on-chart rectangle
        /// </summary>
        /// <param name="chart">Chart for rectangle</param>
        /// <param name="left_bottom">The left-bottom corner point</param>
        /// <param name="size">The size of rectangle</param>
        public ChartRectangle(Chart chart, DPoint left_bottom, DSize size)
            : this(chart, left_bottom, size, Color.Black) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var lb = Chart.ToScreenPoint(LeftBottom);
            var sz = Chart.ToScreenSize(Size);
            if (_is_filled && _fill != null)
                g.FillRectangle(_fill, lb.X, lb.Y - sz.Height, sz.Width, sz.Height);
            g.DrawRectangle(_pen, lb.X, lb.Y - sz.Height, sz.Width, sz.Height);
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public override DRect GetRealRect(Graphics g, double view_scale)
        {
            return new DRect(LeftBottom.X, LeftBottom.Y, Size.Width, Size.Height);
        }
        /// <summary>
        /// This method will be called by chart when mouse was moved on chart
        /// </summary>
        /// <param name="point">Stores the point of mouse cursor tied to grid</param>
        public void MoveTo(DPoint point)
        {
            LeftBottom = new DPoint(point.X - Size.Width / 2, point.Y - Size.Height / 2);
        }
    }
    /// <summary>
    /// On-chart ellipse
    /// </summary>
    public class ChartEllipse : ChartRectObject, IChartMouseMovableObject
    {
        /// <summary>
        /// Create the on-chart ellipse
        /// </summary>
        /// <param name="chart">Chart for ellipse</param>
        /// <param name="left_bottom">The left-bottom corner point</param>
        /// <param name="size">The size of ellipse</param>
        /// <param name="color">The color of ellipse</param>
        public ChartEllipse(Chart chart, DPoint left_bottom, DSize size, Color color)
            : base(chart)
        {
            Flags = ChartObject.Rectangle | ChartObject.MouseMovable;
            LeftBottom = left_bottom;
            Size = size;
            Color = color;
        }
        /// <summary>
        /// Create the on-chart ellipse
        /// </summary>
        /// <param name="chart">Chart for ellipse</param>
        /// <param name="left_bottom">The left-bottom corner point</param>
        /// <param name="size">The size of ellipse</param>
        public ChartEllipse(Chart chart, DPoint left_bottom, DSize size)
            : this(chart, left_bottom, size, Color.Black) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var lb = Chart.ToScreenPoint(LeftBottom);
            var sz = Chart.ToScreenSize(Size);
            if (_is_filled && _fill != null)
                g.FillEllipse(_fill, lb.X, lb.Y - sz.Height, sz.Width, sz.Height);
            g.DrawEllipse(_pen, lb.X, lb.Y - sz.Height, sz.Width, sz.Height);
            DrawStringCenter(g, Text);
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public override DRect GetRealRect(Graphics g, double view_scale)
        {
            return new DRect(LeftBottom.X, LeftBottom.Y, Size.Width, Size.Height);
        }
        /// <summary>
        /// This method will be called by chart when mouse was moved on chart
        /// </summary>
        /// <param name="point">Stores the point of mouse cursor tied to grid</param>
        public void MoveTo(DPoint point)
        {
            LeftBottom = new DPoint(point.X - Size.Width / 2, point.Y - Size.Height / 2);
        }
    }
    /// <summary>
    /// On-chart polygon
    /// </summary>
    public class ChartPolygon : ChartRectObject, IChartMouseMovableObject
    {
        /// <summary>
        /// Gets the center point of movable object
        /// </summary>
        public new DPoint Center
        {
            get
            {
                return _center;
            }
        }
        /// <summary>
        /// Stores array of polygon's points
        /// </summary>
        protected DPoint[] _points;
        /// <summary>
        /// Stores the relative center of all points
        /// </summary>
        protected DPoint _center;
        /// <summary>
        /// Gets or sets the points of this polygon
        /// </summary>
        public DPoint[] Points
        {
            get
            {
                return _points;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Points can't be a null");
                if (value != _points)
                {
                    _points = value;
                    _center = new DPoint();
                    foreach (var p in _points)
                    {
                        _center.X += p.X;
                        _center.Y += p.Y;
                    }
                    var l = _points.Length;
                    _center.X /= l;
                    _center.Y /= l;
                }
            }
        }
        /// <summary>
        /// Create the on-chart polygon
        /// </summary>
        /// <param name="chart">Chart for polygon</param>
        /// <param name="points">The points of polygon</param>
        /// <param name="color">The color of polygon</param>
        public ChartPolygon(Chart chart, DPoint[] points, Color color)
            : base(chart)
        {
            Flags = ChartObject.Rectangle | ChartObject.MouseMovable;
            Points = points;
            Color = color;
        }
        /// <summary>
        /// Create the on-chart polygon
        /// </summary>
        /// <param name="chart">Chart for polygon</param>
        /// <param name="points">The points of polygon</param>
        public ChartPolygon(Chart chart, DPoint[] points)
            : this(chart, points, Color.Black) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            if (_is_filled && _fill != null)
                g.FillPolygon(_fill, (from p in _points select Chart.ToScreenPoint(p)).ToArray());
            g.DrawPolygon(_pen, (from p in _points select Chart.ToScreenPoint(p)).ToArray());
        }
        /// <summary>
        /// This method will be called by chart when mouse was moved on chart
        /// </summary>
        /// <param name="point">Stores the point of mouse cursor tied to grid</param>
        public void MoveTo(DPoint point)
        {
            var dx = point.X - _center.X;
            var dy = point.Y - _center.Y;
            var len = _points.Length;
            var new_points = new DPoint[len];
            for (int i = 0; i < len; i++)
                new_points[i] = new DPoint(_points[i].X + dx, _points[i].Y + dy);
            _center = point;
            _points = new_points;
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public override DRect GetRealRect(Graphics g, double view_scale)
        {
            var min_p = new DPoint(_points[0].X, _points[0].Y);
            var max_p = new DPoint(_points[0].X, _points[0].Y);
            foreach (var p in _points)
            {
                if (p.X < min_p.X) min_p.X = p.X;
                if (p.Y < min_p.Y) min_p.Y = p.Y;
                if (p.X > max_p.X) max_p.X = p.X;
                if (p.Y > max_p.Y) max_p.Y = p.Y;
            }
            var w = max_p.X - min_p.X;
            var h = max_p.Y - min_p.Y;
            return new DRect(min_p.X, min_p.Y, w, h);
        }
    }
    # endregion
}