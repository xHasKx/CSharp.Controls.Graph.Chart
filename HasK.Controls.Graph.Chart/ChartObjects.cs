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
        DRect GetBounds();
    }

    /// <summary>
    /// Base implementation of any object on chart
    /// </summary>
    public class BaseChartObject : IChartObject
    {
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

        public virtual void Draw(Graphics g)
        {
            throw new NotImplementedException();
        }

        public virtual DRect GetBounds()
        {
            throw new NotImplementedException();
        }
    }

    public class ChartPoint : BaseChartObject
    {
        public ChartPoint(Chart chart)
            : base(chart)
        {
            Color = Color.Red;
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

        public override DRect GetBounds()
        {
            var sc = Chart.ViewScale;
            var psz = new DSize(Size.Width / sc, Size.Height / sc);
            return new DRect(Center.X - psz.Width / 2, Center.Y - psz.Height / 2, psz.Width, psz.Height);
        }
    }
}