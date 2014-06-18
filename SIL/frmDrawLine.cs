using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SIL
{
    public partial class frmDrawLine : Form
    {
        List<Line> listOfLines = new List<Line>();
        Graphics surface;

        public frmDrawLine()
        {
            InitializeComponent();
            surface = this.CreateGraphics();
        }

        public void clear()
        {
            listOfLines.Clear();
        }

        public void addLine(string color, int pointOneX, int pointOneY, int pointTwoX, int pointTwoY)
        {
            listOfLines.Add(new Line(Color.FromName(color), pointOneX, pointOneY, pointTwoX, pointTwoY));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            foreach (Line line in listOfLines)
                line.drawLine(surface);
             * */
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Pen pen1 = new Pen(Color.Blue, 1.0f);

            for (int i = 0; i < listOfLines.Count; ++i)
            {
                listOfLines[i].drawLine(this.surface);
            }

            //surface.DrawRectangle(pen1, new Rectangle(10, 10, 50, 50));

            // surface.DrawLine(pen1, 50, 50, 200, 200);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            /*
            foreach (Line line in listOfLines)
                line.drawLine(surface);
             * */
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            /*
            foreach (Line line in listOfLines)
                line.drawLine(surface);
             * */
        }

        class Line
        {
            Color color;
            int pointOneX;
            int pointOneY;
            int pointTwoX;
            int pointTwoY;

            public Line(Color color, int pointOneX, int pointOneY, int pointTwoX, int pointTwoY)
            {
                this.color = color;
                this.pointOneX = pointOneX;
                this.pointOneY = pointOneY;
                this.pointTwoX = pointTwoX;
                this.pointTwoY = pointTwoY;
            }

            public void drawLine(Graphics surface)
            {
                Pen pen1 = new Pen(this.color, 1.0f);
                surface.DrawLine(pen1, pointOneX, pointOneY, pointTwoX, pointTwoY);
            }
        }
    }
}
