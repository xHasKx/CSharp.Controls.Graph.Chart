using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HasK.Controls.Graph;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Color[] _colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Black, Color.LightBlue, Color.DarkCyan, Color.DarkGray };

        Random rand = new Random();

        Color RandColor()
        {
            return _colors[rand.Next(_colors.Length)];
        }

        DPoint RandPoint()
        {
            return new DPoint(chart1.MinX + (chart1.MaxX - chart1.MinX) * rand.NextDouble(),
                chart1.MinY + (chart1.MaxY - chart1.MinY) * rand.NextDouble());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var s = rand.Next(40);
            var obj = new ChartPoint(chart1, RandPoint(), RandColor(), new Size(s, s));
            chart1.AddObject(obj);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var funcs = new ChartFunction.MathFunction[] { Math.Sin, Math.Tan, Math.Ceiling, Math.Cos, Math.Atan };
            var fobj = new ChartFunction(chart1, funcs[rand.Next(funcs.Length)], Color.Black);
            // var fobj = new ChartFunction(chart1, Math.Ceiling, Color.Black);
            fobj.ExtendOnExtremum = true;
            fobj.Color = Color.Red;
            chart1.AddObject(fobj);
        }


        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Text = chart1.ToRealPoint(e.Location).ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var obj = new ChartTextPoint(chart1, rand.Next(100000).ToString(), RandPoint(), (ChartTextPoint.TextPlaceType)rand.Next(4));
            obj.Color = RandColor();
            chart1.AddObject(obj);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var line = new ChartLine(chart1, RandPoint(), RandPoint());
            line.Color = RandColor();
            chart1.AddObject(line);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            chart1.RemoveAllObjects();
            chart1.SetVisibleRect(-11, 11, 11, -11);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.SetVisibleRect(-11, 11, 11, -11);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10000; i++)
            {
                var obj = new ChartPoint(chart1, RandPoint(), RandColor());
                chart1.Items.Add(obj);
            }
            chart1.Redraw();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var obj = new ChartRectangle(chart1, new DPoint(1, 1), new DSize(4, 4), Color.Red);
            chart1.PinMovableObjectAndAdd(obj);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var obj = new ChartEllipse(chart1, new DPoint(1, 1), new DSize(7, 4));
            chart1.PinMovableObjectAndAdd(obj);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var obj = new ChartPolygon(chart1, new DPoint[] { new DPoint(1, 1), new DPoint(1, 3), new DPoint(7, 3), new DPoint(7, 2), new DPoint(5, 2), new DPoint(5, 1), new DPoint(1, 1) });
            obj.PenWidth = 2;
            chart1.PinMovableObjectAndAdd(obj);
        }
    }
}
