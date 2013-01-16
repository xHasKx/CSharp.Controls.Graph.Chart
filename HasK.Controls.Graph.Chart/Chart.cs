using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HasK.Controls.Graph
{
    public partial class Chart : UserControl
    {
        # region Private fields
        private List<IChartObject> _items = new List<IChartObject>();

        private const MouseButtons MoveButton = MouseButtons.Right;
        private const MouseButtons SelectButton = MouseButtons.Left;

        private bool _display_grid;
        private HashSet<MouseButtons> _pressed_mouse = new HashSet<MouseButtons>();
        private Point _pressed_move_point;
        private int _scr_cx, _scr_cy;

        private IChartObject _selected;
        private DPoint _view_center_point;
        private double _view_scale;
        private bool _suspended;

        # endregion

        # region Public fields
        /// <summary>
        /// Shows is coordinates grid should be displayed
        /// </summary>
        public bool DisplayGrid
        {
            get
            {
                return _display_grid;
            }
            set
            {
                if (value != _display_grid)
                {
                    _display_grid = value;
                    Redraw();
                }
            }
        }

        public double MinX { get; private set; }
        public double MinY { get; private set; }
        public double MaxX { get; private set; }
        public double MaxY { get; private set; }

        public Font GridTextFont { get; set; }

        /// <summary>
        /// Get or set selected object
        /// </summary>
        public IChartObject Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (value != _selected)
                {
                    var old_selected = _selected;
                    _selected = value;
                    if (SelectionChanged != null)
                        SelectionChanged(this, old_selected, _selected);
                    Redraw();
                }
            }
        }

        public delegate void SelChanged(Chart chart, IChartObject old_selected, IChartObject now_selected);
        /// <summary>
        /// Occurs when selection is changed
        /// </summary>
        public event SelChanged SelectionChanged;

        /// <summary>
        /// Get or set the center point of view
        /// </summary>
        public DPoint ViewCenterPoint
        {
            get
            {
                return _view_center_point;
            }
            set
            {
                if (value.X != _view_center_point.X || value.Y != _view_center_point.Y)
                {
                    _view_center_point = value;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Get or set view scale
        /// </summary>
        public double ViewScale
        {
            get
            {
                return _view_scale;
            }
            set
            {
                if (value != _view_scale)
                {
                    _view_scale = value;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Get or set the frozen state of graph
        /// </summary>
        public bool Suspended
        {
            get
            {
                return _suspended;
            }
            set
            {
                _suspended = value;
                Redraw();
            }
        }
        # endregion

        # region Visible properties of chart
        private Color _grid_color;
        private Pen _grid_pen;
        private Brush _grid_brush;
        /// <summary>
        /// Get or set the color of coordinates grid
        /// </summary>
        public Color GridColor
        {
            get
            {
                return _grid_color;
            }
            set
            {
                if (value != _grid_color)
                {
                    _grid_color = value;
                    _grid_pen = new Pen(_grid_color);
                    _grid_brush = new SolidBrush(_grid_color);
                    Redraw();
                }
            }
        }

        private Color _background_color;
        private Brush _background_brush;
        /// <summary>
        /// Get or set the background color
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return _background_color;
            }
            set
            {
                if (value != _background_color)
                {
                    _background_color = value;
                    _background_brush = new SolidBrush(_background_color);
                    Redraw();
                }
            }
        }
        # endregion

        public Chart()
        {
            InitializeComponent();
        }

        # region Methods to control the visible representation of chart
        public void Redraw()
        {
            if (!_suspended)
                Invalidate();
        }

        public void SetDefaults()
        {
            Suspended = true;
            GridColor = Color.Gray;
            GridTextFont = new Font("Tahoma", 10);
            DisplayGrid = true;
            BackgroundColor = Color.White;
            SetGridMinMax(-10, 10, -10, 10);
            SetVisibleRect(-11, 11, 22, 22);
            Suspended = false;
        }

        /// <summary>
        /// Set grid bounds
        /// </summary>
        public void SetGridMinMax(double minX, double maxX, double minY, double maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            Redraw();
        }

        /// <summary>
        /// Set visible rectangle on chart in real coordinates
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void SetVisibleRect(double x, double y, double width, double height)
        {
            var rw = Width / width;
            var rh = Height / height;
            Suspended = true;
            ViewCenterPoint = new DPoint(x + width / 2, y - height / 2);
            ViewScale = Math.Min(rw, rh);
            Suspended = false;
        }

        public void SetVisibleRect(DRect rect)
        {
            SetVisibleRect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        # endregion

        # region Methods to control of assigned objects
        /// <summary>
        /// Add object to chart
        /// </summary>
        /// <param name="obj">Object which should be added</param>
        public void AddObject(IChartObject obj)
        {
            _items.Add(obj);
            Redraw();
        }
        /// <summary>
        /// Remove object from chart
        /// </summary>
        /// <param name="obj">Object which should be removed</param>
        /// <returns>Returns true if object was removed, otherwise false</returns>
        public bool RemoveObject(IChartObject obj)
        {
            var res = _items.Remove(obj);
            if (res)
                Redraw();
            return res;
        }
        /// <summary>
        /// All items on chart
        /// </summary>
        public IEnumerable<IChartObject> Items { get { return _items; } }
        # endregion

        # region Methods for drawing and processing events
        private void Draw(Graphics g)
        {
            if (!_suspended)
            {
                g.FillRectangle(_background_brush, 0, 0, Width, Height);
                DrawGrid(g);
                foreach (var obj in _items)
                    obj.Draw(g);
                if (_selected != null)
                    DrawSelection(g, _selected);
            }
        }

        /// <summary>
        /// Draw double value near grid line
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="value">Double value</param>
        /// <param name="where">Point in screen coordinates where value should be</param>
        /// <param name="on_the_right">If true - draw value on the right, otherwise - from the bottom</param>
        protected void DrawGridNum(Graphics g, double value, Point where, bool on_the_right)
        {
            var sval = value.ToString();
            var ssize = g.MeasureString(sval, GridTextFont);
            if (on_the_right)
            {
                where.X += 5;
                where.Y -= (int)(ssize.Height / 2);
            }
            else
            {
                where.Y += 5;
                where.X -= (int)(ssize.Width / 2);
            }
            g.DrawString(sval, GridTextFont, _grid_brush, where);
        }

        protected void DrawGrid(Graphics g)
        {
            var scr_x0 = ToScreenPoint(new DPoint(MinX, 0));
            var scr_x1 = ToScreenPoint(new DPoint(MaxX, 0));
            g.DrawLine(_grid_pen, scr_x0, scr_x1);
            g.DrawLine(_grid_pen, scr_x1.X, scr_x1.Y, scr_x1.X - 4, scr_x1.Y - 4);
            g.DrawLine(_grid_pen, scr_x1.X, scr_x1.Y, scr_x1.X - 4, scr_x1.Y + 4);
            DrawGridNum(g, MinX, scr_x0, false);
            DrawGridNum(g, MaxX, scr_x1, false);
            
            var scr_y0 = ToScreenPoint(new DPoint(0, MinY));
            var scr_y1 = ToScreenPoint(new DPoint(0, MaxY));
            g.DrawLine(_grid_pen, scr_y0, scr_y1);
            g.DrawLine(_grid_pen, scr_y1.X, scr_y1.Y, scr_y1.X - 4, scr_y1.Y + 4);
            g.DrawLine(_grid_pen, scr_y1.X, scr_y1.Y, scr_y1.X + 4, scr_y1.Y + 4);
            DrawGridNum(g, MinY, scr_y0, true);
            DrawGridNum(g, MaxY, scr_y1, true);
        }

        protected void DrawSelection(Graphics g, IChartObject obj)
        {
            var b = obj.GetBounds();
            var rbr = Brushes.Black;
            var sel_size = new Size(4, 4);

            var p1 = ToScreenPoint(new DPoint(b.X, b.Y));
            var p2 = ToScreenPoint(new DPoint(b.X + b.Width, b.Y));
            var p3 = ToScreenPoint(new DPoint(b.X + b.Width, b.Y + b.Height));
            var p4 = ToScreenPoint(new DPoint(b.X, b.Y + b.Height));

            p1.Offset(-sel_size.Width, 0);
            p3.Offset(0, -sel_size.Height); p4.Offset(-sel_size.Width, -sel_size.Height);

            g.FillRectangle(rbr, new Rectangle(p1, sel_size));
            g.FillRectangle(rbr, new Rectangle(p2, sel_size));
            g.FillRectangle(rbr, new Rectangle(p3, sel_size));
            g.FillRectangle(rbr, new Rectangle(p4, sel_size));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var gr = e.Graphics;
            Draw(gr);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            _scr_cx = Width / 2;
            _scr_cy = Height / 2;
            Redraw();
            base.OnResize(eventargs);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed_mouse.Add(e.Button);
            if (e.Button == MoveButton)
                _pressed_move_point = e.Location;
            if (e.Button == SelectButton)
                TrySelectObject(e.Location);
            base.OnMouseDown(e);
        }

        private void TrySelectObject(Point point)
        {
            var dp = ToRealPoint(point);
            foreach (var obj in _items)
                if (dp.InRect(obj.GetBounds()))
                {
                    Selected = obj;
                    break;
                }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed_mouse.Remove(e.Button);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Focus();
            if (_pressed_mouse.Contains(MoveButton))
            {
                var dx = e.X - _pressed_move_point.X;
                var dy = e.Y - _pressed_move_point.Y;
                _pressed_move_point = e.Location;
                var rdx = dx / _view_scale;
                var rdy = dy / _view_scale;
                ViewCenterPoint = new DPoint(_view_center_point.X - rdx, _view_center_point.Y + rdy);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var dd = 1.0; // TODO: rework this scaling
            if (e.Delta < 0)
                dd = -dd;
            if (_view_scale + dd > 2.5)
                ViewScale += dd;
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == MoveButton)
                ViewCenterPoint = ToRealPoint(e.Location);
            base.OnMouseDoubleClick(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            Focus();
            base.OnEnter(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetDefaults();
        }
        # endregion

        # region Methods of useful tools

        /// <summary>
        /// Converting real coordinates to screen
        /// </summary>
        /// <param name="real"></param>
        /// <returns></returns>
        public Point ToScreenPoint(DPoint real)
        {
            return new Point(_scr_cx - (int)((_view_center_point.X - real.X) * _view_scale), _scr_cy + (int)((_view_center_point.Y - real.Y) * _view_scale));
        }

        /// <summary>
        /// Converting screen coordinates to real
        /// </summary>
        /// <param name="scr"></param>
        /// <returns></returns>
        public DPoint ToRealPoint(Point scr)
        {
            return new DPoint((scr.X - _scr_cx) / _view_scale + _view_center_point.X, _view_center_point.Y - (scr.Y - _scr_cy) / _view_scale);
        }
        # endregion
    }

}
