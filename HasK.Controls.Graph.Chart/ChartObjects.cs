using System;
using System.Drawing;

namespace HasK.Controls.Graph
{
    /// <summary>
    /// Interface which should be implemented by any object on chart
    /// </summary>
    public interface IChartObject
    {
        /// <summary>
        /// Represent the center point of object
        /// </summary>
        DPoint Center { get; set; }
        /// <summary>
        /// Represent the chart which contains this object
        /// </summary>
        Chart Chart { get; }
        /// <summary>
        /// Shows if object can be selected
        /// </summary>
        bool Selectable { get; }
        /// <summary>
        /// Shows if object is selected
        /// </summary>
        bool IsSelected { get; set; }
        /// <summary>
        /// This function calls to draw object on chart
        /// </summary>
        /// <param name="g"></param>
        void Draw(Graphics g);
        /// <summary>
        /// This proc should return the bound of object in real coordinates
        /// </summary>
        /// <returns></returns>
        DRect GetBounds(Graphics g, double view_scale);
    }

    /// <summary>
    /// Base implementation of any object on chart
    /// </summary>
    public abstract class BaseChartObject : IChartObject
    {
        public abstract bool Selectable { get; }

        public DPoint Center { get; set; }
        public Chart Chart { get; private set; }
        public bool IsSelected
        {
            get
            {
                return (this == Chart.Selected);
            }
            set
            {
                Chart.Selected = this;
            }
        }

        public BaseChartObject(Chart chart)
        {
            Chart = chart;
        }

        public abstract void Draw(Graphics g);

        public abstract DRect GetBounds(Graphics g, double view_scale);
    }

    /// <summary>
    /// Simple square point on chart with specified color and size
    /// </summary>
    public class ChartPoint : BaseChartObject
    {

        public override bool Selectable { get { return true; } }

        public ChartPoint(Chart chart)
            : base(chart)
        {
            Color = Color.Black;
            Size = new Size(8, 8);
        }

        private Color _color;
        private Brush _brush;
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
                    _brush = new SolidBrush(_color);
                }
            }
        }
        public Size Size { get; set; }

        public override void Draw(Graphics g)
        {
            var rp = Chart.ToScreenPoint(Center);
            g.FillRectangle(_brush, rp.X - Size.Width / 2, rp.Y - Size.Height / 2, Size.Width, Size.Height);
        }

        public override DRect GetBounds(Graphics g, double view_scale)
        {
            var psz = new DSize(Size.Width / view_scale, Size.Height / view_scale);
            return new DRect(Center.X - psz.Width / 2, Center.Y - psz.Height / 2, psz.Width, psz.Height);
        }
    }

    /// <summary>
    /// One-pixel line on chart with specified points of begin, end, and specified color
    /// </summary>
    public class ChartLine : BaseChartObject
    {
        private new DPoint Center; // hide Center property

        public override bool Selectable { get { return false; } }

        public DPoint Begin { get; set; }
        public DPoint End { get; set; }

        private Color _color;
        private Pen _pen;
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
                }
            }
        }


        public ChartLine(Chart chart) : this(chart, new DPoint(), new DPoint()) { }

        public ChartLine(Chart chart, DPoint begin, DPoint end)
            : base(chart)
        {
            Begin = begin;
            End = end;
            Color = Color.Black;
        }

        public override void Draw(Graphics g)
        {
            var begin = Chart.ToScreenPoint(Begin);
            var end = Chart.ToScreenPoint(End);
            g.DrawLine(_pen, begin, end);
        }

        public override DRect GetBounds(Graphics g, double view_scale)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Point with specified color and text near it
    /// </summary>
    public class ChartTextPoint : BaseChartObject
    {
        public enum TextPlaceType { Bottom, Left, Top, Right };

        public override bool Selectable { get { return false; } }

        public string Text { get; set; }

        private Color _color;
        private Pen _pen;
        private Brush _brush;
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

        public TextPlaceType TextPlace { get; set; }

        public Font Font { get; set; }

        public ChartTextPoint(Chart chart, string text, DPoint position, TextPlaceType place_type)
            : base(chart)
        {
            Center = position;
            Text = text;
            Font = new Font("Tahoma", 10);
            Color = Color.Black;
            TextPlace = place_type;
        }

        public override void Draw(Graphics g)
        {
            var pos = Chart.ToScreenPoint(Center);
            PointF text_pos;
            var text_size = g.MeasureString(Text, Font);

            if (TextPlace == TextPlaceType.Bottom)
                text_pos = new PointF(pos.X - text_size.Width / 2, pos.Y + 5);
            else if (TextPlace == TextPlaceType.Left)
                text_pos = new PointF(pos.X - 5 - text_size.Width, pos.Y - text_size.Height / 2);
            else if (TextPlace == TextPlaceType.Top)
                text_pos = new PointF(pos.X - text_size.Width / 2, pos.Y - 5 - text_size.Height);
            else if (TextPlace == TextPlaceType.Right)
                text_pos = new PointF(pos.X + 5, pos.Y - text_size.Height / 2);
            else
                return;
            g.FillEllipse(_brush, pos.X - 3, pos.Y - 3, 6, 6);
            g.DrawString(Text, Font, _brush, text_pos);
        }

        public override DRect GetBounds(Graphics g, double view_scale)
        {
            throw new NotImplementedException();
        }
    }

}