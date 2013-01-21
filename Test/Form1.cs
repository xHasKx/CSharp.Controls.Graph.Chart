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

        private void button1_Click(object sender, EventArgs e)
        {
            var obj = new ChartPoint(chart1);
            var r = new Random();
            obj.Center = new DPoint(chart1.MinX + (chart1.MaxX - chart1.MinX) * r.NextDouble(),
                chart1.MinY + (chart1.MaxY - chart1.MinY) * r.NextDouble());
            obj.Color = Color.Red;
            chart1.AddObject(obj);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.ViewScale *= 2;
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Text = chart1.ViewScale.ToString();
        }
    }
}
