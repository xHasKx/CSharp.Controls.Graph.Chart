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
    /// <summary>
    /// Chart component
    /// </summary>
    public partial class Chart : UserControl
    {
        # region Common private fields
        private List<ChartObject> _items = new List<ChartObject>();

        private bool _show_grid;
        private bool _show_grid_numbers;
        private HashSet<MouseButtons> _pressed_mouse = new HashSet<MouseButtons>();
        private Point _pressed_move_point;
        private int _scr_cx, _scr_cy;
        private double _minX, _maxX, _minY, _maxY;

        private ChartObject _selected;
        private DPoint _view_center_point;
        private double _view_scale;
        private bool _suspended;
        # endregion
        # region Common public fields/properties
        /// <summary>
        /// Shows is coordinates grid should be displayed
        /// </summary>
        public bool ShowGrid
        {
            get
            {
                return _show_grid;
            }
            set
            {
                if (value != _show_grid)
                {
                    _show_grid = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Gets or sets mouse button for moving chart coordinates system
        /// </summary>
        public MouseButtons MoveButton { get; set; }
        /// <summary>
        /// Gets or sets mouse button for centering view in specified point
        /// </summary>
        public MouseButtons SetViewCenterButton { get; set; }
        /// <summary>
        /// Gets or sets mouse button for selecting objects
        /// </summary>
        public MouseButtons SelectButton { get; set; }
        /// <summary>
        /// Gets or sets mouse button for selecting objects
        /// </summary>
        public MouseButtons UnpinButton { get; set; }
        /// <summary>
        /// Get or set option for display numbers on coordinates grid
        /// </summary>
        public bool ShowGridNumbers
        {
            get
            {
                return _show_grid_numbers;
            }
            set
            {
                if (value != _show_grid_numbers)
                {
                    _show_grid_numbers = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Minumum grid value by X axis
        /// </summary>
        public double MinX
        {
            get { return _minX; }
            set{
                if (value != _minX)
                {
                    _minX = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Minumum grid value by Y axis
        /// </summary>
        public double MinY
        {
            get { return _minY; }
            set
            {
                if (value != _minY)
                {
                    _minY = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Maximum grid value by X axis
        /// </summary>
        public double MaxX
        {
            get { return _maxX; }
            set
            {
                if (value != _maxX)
                {
                    _maxX = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Maximum grid value by Y axis
        /// </summary>
        public double MaxY
        {
            get { return _maxY; }
            set
            {
                if (value != _maxY)
                {
                    _maxY = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Gets or sets the grid size for moveable objects
        /// </summary>
        public double MoveableObjectsGridSize { get; set; }
        /// <summary>
        /// Gets or sets the font of text on grid
        /// </summary>
        public Font GridTextFont { get; set; }
        /// <summary>
        /// Gets or sets selected object
        /// </summary>
        public ChartObject Selected
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
        /// <summary>
        /// Stores the current pinned mouse-movable object
        /// </summary>
        protected ChartObject _pinned_moving_object;
        /// <summary>
        /// Gets the current pinned mouse-movable object
        /// </summary>
        public ChartObject PinnedMovingObject
        {
            get
            {
                return _pinned_moving_object;
            }
        }
        /// <summary>
        /// Delegate for SelectionChanged event
        /// </summary>
        /// <param name="chart">Chart with selected object</param>
        /// <param name="old_selected">The object which was previously selected</param>
        /// <param name="now_selected">The object which is current selected</param>
        public delegate void SelChanged(Chart chart, ChartObject old_selected, ChartObject now_selected);
        /// <summary>
        /// Occurs when selection is changed
        /// </summary>
        public event SelChanged SelectionChanged;
        /// <summary>
        /// Gets or sets the center point of view
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
        /// Gets or sets view scale
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
        /// Gets or sets the frozen state of graph
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
        /// Gets or sets the color of coordinates grid
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
        /// Gets or sets the background color
        /// </summary>
        public override Color BackColor
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

        private Color _sel_color;
        private Brush _sel_brush;
        /// <summary>
        /// Color of selection
        /// </summary>
        public Color SelectionColor
        {
            get
            {
                return _sel_color;
            }
            set
            {
                if (value != _sel_color)
                {
                    _sel_color = value;
                    _sel_brush = new SolidBrush(_sel_color);
                    Redraw();
                }
            }
        }
        private bool _selectable_objects;
        /// <summary>
        /// Gets or sets ability of selecting objects
        /// </summary>
        public bool SelectableObjects
        {
            get
            {
                return _selectable_objects;
            }
            set
            {
                if (value != _selectable_objects)
                {
                    _selectable_objects = value;
                    Redraw();
                }
            }
        }
        /// <summary>
        /// Gets or sets the option - if user can scale chart by mouse wheel
        /// </summary>
        public bool CanScaleByMouse { get; set; }
        /// <summary>
        /// Gets or sets option - if user can move chart's coordinat system by mouse
        /// </summary>
        public bool CanMoveByMouse { get; set; }
        # endregion
        /// <summary>
        /// Create chart control
        /// </summary>
        public Chart()
        {
            SetDefaults();
            InitializeComponent();
            MouseWheelHandler.Add(this, MyOnMouseWheel);
        }
        # region Methods to control the visible representation of chart
        /// <summary>
        /// Redraw chart and all objects
        /// </summary>
        public void Redraw()
        {
            if (!_suspended)
                Invalidate();
        }
        /// <summary>
        /// Set default values for main chart properties
        /// </summary>
        public void SetDefaults()
        {
            Suspended = true;
            MoveButton = MouseButtons.Right;
            SelectButton = MouseButtons.Left;
            UnpinButton = MouseButtons.Left;
            SetViewCenterButton = MouseButtons.Left;
            GridColor = Color.Gray;
            GridTextFont = new Font("Tahoma", 10);
            ShowGrid = true;
            ShowGridNumbers = true;
            BackColor = Color.White;
            SelectionColor = Color.Black;
            CanScaleByMouse = true;
            CanMoveByMouse = true;
            SetGridMinMax(-10, 10, -10, 10);
            MoveableObjectsGridSize = 0.05;
            SetVisibleRect(-11, 11, 11, -11);
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
        /// <param name="x_left">Left position</param>
        /// <param name="y_top">Top position</param>
        /// <param name="x_right">Right porition</param>
        /// <param name="y_bottom">Bottom position</param>
        public void SetVisibleRect(double x_left, double y_top, double x_right, double y_bottom)
        {
            var width = x_right - x_left;
            var height = y_top - y_bottom;
            var rw = Width / width;
            var rh = Height / height;
            Suspended = true;
            ViewCenterPoint = new DPoint(x_left + width / 2, y_top - height / 2);
            ViewScale = Math.Min(rw, rh);
            Suspended = false;
        }
        # endregion
        # region Methods to control of assigned objects
        /// <summary>
        /// Add object to chart
        /// </summary>
        /// <param name="obj">Object which should be added</param>
        public void AddObject(ChartObject obj)
        {
            _items.Add(obj);
            Redraw();
        }
        /// <summary>
        /// Remove object from chart
        /// </summary>
        /// <param name="obj">Object which should be removed</param>
        /// <returns>Returns true if object was removed, otherwise false</returns>
        public bool RemoveObject(ChartObject obj)
        {
            var res = _items.Remove(obj);
            if (res)
            {
                if (obj == _selected)
                    _selected = null;
                Redraw();
            }
            return res;
        }
        /// <summary>
        /// Removing all objects from chart
        /// </summary>
        public void RemoveAllObjects()
        {
            _selected = null;
            _items.Clear();
            Redraw();
        }
        /// <summary>
        /// All items on chart
        /// </summary>
        public IList<ChartObject> Items { get { return _items; } }
        /// <summary>
        /// Set the current pinned mouse-movable object
        /// </summary>
        /// <param name="value"></param>
        public void PinMovableObjectAndAdd(ChartObject value)
        {
            if (value == null)
                _pinned_moving_object = null;
            else if ((value.Flags & ChartObject.MouseMovable) > 0)
            {
                var mobj = value as IChartMouseMovableObject;
                if (mobj != null)
                {
                    _pinned_moving_object = value;
                    if (!_items.Contains(value))
                        _items.Add(value);
                    Redraw();
                }
                else
                    throw new ArgumentException("Can't cast value as IChartMouseMovableObject");
            }
            else
                throw new ArgumentException("value hasn't flag ChartObject.MouseMovable");
        }
        # endregion
        # region Methods for drawing and processing events
        private void Draw(Graphics g)
        {
            if (!_suspended)
            {
                g.FillRectangle(_background_brush, 0, 0, Width, Height);
                if (_show_grid)
                    DrawGrid(g);
                foreach (var obj in _items)
                    if ((obj.Flags & ChartObject.Invisible) == 0)
                        obj.Draw(g);
                if (_selectable_objects && _selected != null)
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
        protected void DrawGridNum(Graphics g, double value, PointF where, bool on_the_right)
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
        /// <summary>
        /// Draw simple grid on chart
        /// </summary>
        /// <param name="g">Graphics which should be used for drawing</param>
        protected void DrawGrid(Graphics g)
        {
            var scr_x0 = ToScreenPoint(new DPoint(MinX, 0));
            var scr_x1 = ToScreenPoint(new DPoint(MaxX, 0));
            g.DrawLine(_grid_pen, scr_x0, scr_x1); // X axis
            g.DrawLine(_grid_pen, scr_x1.X, scr_x1.Y, scr_x1.X - 4, scr_x1.Y - 4); // X arrow
            g.DrawLine(_grid_pen, scr_x1.X, scr_x1.Y, scr_x1.X - 4, scr_x1.Y + 4); //
            var scr_y0 = ToScreenPoint(new DPoint(0, MinY));
            var scr_y1 = ToScreenPoint(new DPoint(0, MaxY));
            g.DrawLine(_grid_pen, scr_y0, scr_y1); // Y axis
            g.DrawLine(_grid_pen, scr_y1.X, scr_y1.Y, scr_y1.X - 4, scr_y1.Y + 4); // Y arrow
            g.DrawLine(_grid_pen, scr_y1.X, scr_y1.Y, scr_y1.X + 4, scr_y1.Y + 4); //
            if (_show_grid_numbers)
            {
                DrawGridNum(g, MinX, scr_x0, false);
                DrawGridNum(g, MaxX, scr_x1, false);
                DrawGridNum(g, MinY, scr_y0, true);
                DrawGridNum(g, MaxY, scr_y1, true);
            }
        }
        /// <summary>
        /// Draw simple selection around selected object
        /// </summary>
        /// <param name="g">Graphics which should be used for drawing</param>
        /// <param name="obj">Selected object</param>
        protected void DrawSelection(Graphics g, ChartObject obj)
        {
            if ((obj.Flags & ChartObject.Selectable) > 0)
            {
                var sobj = obj as IChartSelectableObject;
                var b = sobj.GetBounds(g, _view_scale);
                var sel_size = new Size(4, 4);

                var p1 = ToScreenPoint(new DPoint(b.X, b.Y));
                var p2 = ToScreenPoint(new DPoint(b.X + b.Width, b.Y));
                var p3 = ToScreenPoint(new DPoint(b.X + b.Width, b.Y + b.Height));
                var p4 = ToScreenPoint(new DPoint(b.X, b.Y + b.Height));

                p1.X -= sel_size.Width;
                p3.Y -= sel_size.Height;
                p4.X -= sel_size.Width;
                p4.Y -= sel_size.Height;

                g.FillRectangle(_sel_brush, new RectangleF(p1, sel_size));
                g.FillRectangle(_sel_brush, new RectangleF(p2, sel_size));
                g.FillRectangle(_sel_brush, new RectangleF(p3, sel_size));
                g.FillRectangle(_sel_brush, new RectangleF(p4, sel_size));
            }
        }
        /// <summary>
        /// OnPaint event handler
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
        }
        /// <summary>
        /// OnResize event handler
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            _scr_cx = Width / 2;
            _scr_cy = Height / 2;
            Redraw();
            base.OnResize(eventargs);
        }
        /// <summary>
        /// OnMouseDown event handler
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed_mouse.Add(e.Button);
            if (e.Button == MoveButton)
                _pressed_move_point = e.Location;
            if (_pinned_moving_object != null && e.Button == UnpinButton)
                PinMovableObjectAndAdd(null);
            else if (_selectable_objects && e.Button == SelectButton)
                TrySelectObject(e.Location);
            base.OnMouseDown(e);
        }
        /// <summary>
        /// Used for enumerate all object and check if point in its bounds
        /// </summary>
        /// <param name="point"></param>
        protected void TrySelectObject(Point point)
        {
            var dp = ToRealPoint(point);
            foreach (var obj in _items)
                if ((obj.Flags & ChartObject.Selectable) > 0)
                {
                    var sobj = obj as IChartSelectableObject;
                    if (dp.InRect(sobj.GetBounds(CreateGraphics(), _view_scale)))
                    {
                        Selected = obj;
                        break;
                    }
                }
        }
        /// <summary>
        /// OnMouseUp event handler
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed_mouse.Remove(e.Button);
            base.OnMouseUp(e);
        }
        /// <summary>
        /// OnMouseMove events handler
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_pinned_moving_object != null)
            {
                var rp = ToRealPoint(e.Location);
                var mo = _pinned_moving_object as IChartMouseMovableObject;
                var mgs = MoveableObjectsGridSize;
                rp.X = Math.Ceiling(rp.X / mgs) * mgs;
                rp.Y = Math.Ceiling(rp.Y / mgs) * mgs;
                mo.MoveTo(rp);
                Redraw();
            }
            if (_pressed_mouse.Contains(MoveButton) && CanMoveByMouse)
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
        /// <summary>
        /// Custom OnMouseWheel events handler
        /// </summary>
        protected void MyOnMouseWheel(MouseEventArgs e)
        {
            if (CanScaleByMouse) // TODO: reimplement this monkey-code
            {
                var dd = 2.0;
                if (_view_scale > 50)
                    dd = _view_scale / 10;

                if (e.Delta < 0)
                    dd = -dd;
                if (_view_scale + dd > 2.5)
                    ViewScale += dd;
                if (_view_scale > 700000)
                    _view_scale = 700000;
            }
            base.OnMouseWheel(e);
        }
        /// <summary>
        /// OnMouseDoubleClick event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == SetViewCenterButton && CanMoveByMouse)
                ViewCenterPoint = ToRealPoint(e.Location);
            base.OnMouseDoubleClick(e);
        }
        # endregion
        # region Useful tools
        /// <summary>
        /// Converting real coordinates to screen
        /// </summary>
        /// <param name="real">Point in real coordinates</param>
        public PointF ToScreenPoint(DPoint real)
        {
            return new PointF((float)(_scr_cx - (_view_center_point.X - real.X) * _view_scale), (float)(_scr_cy + (_view_center_point.Y - real.Y) * _view_scale));
        }
        /// <summary>
        /// Converting real size to screen
        /// </summary>
        /// <param name="real">Size in real coordinates</param>
        public SizeF ToScreenSize(DSize real)
        {
            return new SizeF((float)(real.Width * _view_scale), (float)(real.Height * _view_scale));
        }
        /// <summary>
        /// Converting screen coordinates to real
        /// </summary>
        /// <param name="scr">Point on screen</param>
        /// <returns></returns>
        public DPoint ToRealPoint(Point scr)
        {
            return new DPoint((scr.X - _scr_cx) / _view_scale + _view_center_point.X, _view_center_point.Y - (scr.Y - _scr_cy) / _view_scale);
        }
        /// <summary>
        /// Converting screen coordinates to real
        /// </summary>
        /// <param name="scr">Point on screen</param>
        /// <returns></returns>
        public DPoint ToRealPoint(PointF scr)
        {
            return new DPoint((scr.X - _scr_cx) / _view_scale + _view_center_point.X, _view_center_point.Y - (scr.Y - _scr_cy) / _view_scale);
        }
        /// <summary>
        /// Class for effective catching WM_MOUSEWHEEL message to change ViewScale by mouse
        /// </summary>
        private static class MouseWheelHandler
        {
            public static void Add(Control ctrl, Action<MouseEventArgs> onMouseWheel)
            {
                if (ctrl == null || onMouseWheel == null)
                    throw new ArgumentNullException();

                var filter = new MouseWheelMessageFilter(ctrl, onMouseWheel);
                Application.AddMessageFilter(filter);
                ctrl.Disposed += (s, e) => Application.RemoveMessageFilter(filter);
            }

            class MouseWheelMessageFilter
                : IMessageFilter
            {
                private readonly Control _ctrl;
                private readonly Action<MouseEventArgs> _onMouseWheel;

                public MouseWheelMessageFilter(Control ctrl, Action<MouseEventArgs> onMouseWheel)
                {
                    _ctrl = ctrl;
                    _onMouseWheel = onMouseWheel;
                }

                public bool PreFilterMessage(ref Message m)
                {
                    var parent = _ctrl.Parent;
                    if (parent != null && m.Msg == 0x20a) // WM_MOUSEWHEEL, find the control at screen position m.LParam
                    {
                        var pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);

                        var clientPos = _ctrl.PointToClient(pos);

                        if (_ctrl.ClientRectangle.Contains(clientPos)
                         && ReferenceEquals(_ctrl, parent.GetChildAtPoint(parent.PointToClient(pos))))
                        {
                            var wParam = m.WParam.ToInt32();
                            Func<int, MouseButtons, MouseButtons> getButton =
                                (flag, button) => ((wParam & flag) == flag) ? button : MouseButtons.None;

                            var buttons = getButton(wParam & 0x0001, MouseButtons.Left)
                                        | getButton(wParam & 0x0010, MouseButtons.Middle)
                                        | getButton(wParam & 0x0002, MouseButtons.Right)
                                        | getButton(wParam & 0x0020, MouseButtons.XButton1)
                                        | getButton(wParam & 0x0040, MouseButtons.XButton2)
                                        ; // Not matching for these /*MK_SHIFT=0x0004;MK_CONTROL=0x0008*/

                            var delta = wParam >> 16;
                            var e = new MouseEventArgs(buttons, 0, clientPos.X, clientPos.Y, delta);
                            _onMouseWheel(e);

                            return true;
                        }
                    }
                    return false;
                }
            }
        }
        # endregion
    }
}
