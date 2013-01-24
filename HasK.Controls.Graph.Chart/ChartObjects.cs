using System;
using System.Drawing;

namespace HasK.Controls.Graph
{
    /// <summary>
    /// Interface which should be implemented by any object on chart
    /// </summary>
    public abstract class ChartObject
    {
        /// <summary>
        /// If this flag will be in object's flags, then chart will not call Draw object
        /// </summary>
        public const uint Invisible         = 0x00000001;
        /// <summary>
        /// If this flag will be in object's flags, then chart will cast object to IChartSelectableObject to determine selected object
        /// </summary>
        public const uint Selectable        = 0x00000002;
        /// <summary>
        /// Stores bit-flags capabilities of object
        /// </summary>
        public uint Flags                   = 0;
        /// <summary>
        /// The chart which contains this object
        /// </summary>
        public Chart Chart { get; private set; }
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
        /// Create base class for all visible objects
        /// </summary>
        /// <param name="chart"></param>
        public ChartVisibleObject(Chart chart) : base(chart) { }
    }
    /// <summary>
    /// Interface for all selectable on-chart objects
    /// </summary>
    public interface IChartSelectableObject
    {
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        DRect GetBounds(Graphics g, double view_scale);
    }
    /// <summary>
    /// Simple square point on chart with specified color and size
    /// </summary>
    public class ChartPoint : ChartVisibleObject, IChartSelectableObject
    {
        /// <summary>
        /// Gets or sets the position of object
        /// </summary>
        public DPoint Center { get; set; }
        /// <summary>
        /// The size value of Size property
        /// </summary>
        protected Size _size;
        /// <summary>
        /// Gets or sets the visible size of object
        /// </summary>
        public Size Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        /// <param name="color">The color of this point</param>
        /// <param name="size">The size of this point, in screen coordinates</param>
        public ChartPoint(Chart chart, DPoint center, Color color, Size size)
            : base(chart)
        {
            Flags |= ChartObject.Selectable;
            Center = center;
            Color = color;
            Size = size;
        }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        public ChartPoint(Chart chart)
            : this(chart, new DPoint(), Color.Black, new Size(8, 8)) { }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        /// <param name="color">The color of this point</param>
        public ChartPoint(Chart chart, DPoint center, Color color)
            : this(chart, center, color, new Size(8, 8)) { }
        /// <summary>
        /// Create simple square point on chart
        /// </summary>
        /// <param name="chart">Chart for point</param>
        /// <param name="center">The center position of this point, in real coordinates</param>
        public ChartPoint(Chart chart, DPoint center)
            : this(chart, center, Color.Black, new Size(8, 8)) { }
        /// <summary>
        /// Chart will calls this method to draw object
        /// </summary>
        /// <param name="g">Graphics, which should be used for drawing</param>
        public override void Draw(Graphics g)
        {
            var rp = Chart.ToScreenPoint(Center);
            g.FillRectangle(_brush, rp.X - Size.Width / 2, rp.Y - Size.Height / 2, Size.Width, Size.Height);
        }
        /// <summary>
        /// Chart will calls this method to determine visible bounds of object with given view scale and graphics
        /// </summary>
        /// <param name="g">Current Graphics object, use it for MeasureStringWidth or something else</param>
        /// <param name="view_scale">Current view scale of chart</param>
        public DRect GetBounds(Graphics g, double view_scale)
        {
            var psz = new DSize(Size.Width / view_scale, Size.Height / view_scale);
            return new DRect(Center.X - psz.Width / 2, Center.Y - psz.Height / 2, psz.Width, psz.Height);
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
        /// Gets or sets the text near the point
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets text placement type of text near the point
        /// </summary>
        public TextPlaceType TextPlace { get; set; }
        /// <summary>
        /// Gets or sets font of text near the point
        /// </summary>
        public Font Font { get; set; }
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
            Font = new Font("Tahoma", 10);
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

            var dszx = _size.Width / 2;
            var dszy = _size.Height / 2;

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

}