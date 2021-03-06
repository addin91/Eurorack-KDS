﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TINRS_ArtWorkGenerator;

namespace Sim1
{
    public partial class WobblerTestFrame : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public WobblerTestFrame()
        {
            for (int i = 0; i < 120000; i++)
            {
                values.Add(0);
                valuesref.Add(0);
                linvalues.Add(0);
                values2.Add(0);
                values64.Add(0);
                linvalues2.Add(0);
            }

            TestFrameLoader.Init();
            InitializeComponent();

            RebuildLFO();
            //BuildCalibrationTable();
            //BuildFreqLerp();
           // BuildPoster();

            Shape.Value = 255 / 5;

        }

        private void BuildFreqLerp()
        {
            List<string> FreqLerp = new List<string>();
            int len = 16;
            FreqLerp.Add(string.Format("#define FREQLERPLEN {0}", len));
            FreqLerp.Add(string.Format(""));
            FreqLerp.Add(string.Format("unsigned long FreqLerp[{0}] = {{", len));


            for (int i = 0; i < len; i++)
            {
                double F = (0.1 * Math.Pow(2.0, (((double)i / (double)(len-1)) * 6.64386)));
                UInt32 V = (UInt32)(((1.0 / 2000.0) * F) * ((double)(1 << 30)));
                if (i < len - 1)
                {
                    FreqLerp.Add(string.Format("\t{0}, // {1} -> {2}", V, F, V * 2000));
                }
                else
                {
                    FreqLerp.Add(string.Format("\t{0} // {1} -> {2}", V,F, V*2000));
                }

            }

            FreqLerp.Add(string.Format("}};"));
            FreqLerp.Add(string.Format(""));

            System.IO.File.WriteAllLines("FreqLerp.h", FreqLerp);
        }

        void BuildCalibrationTable()
        {
            List<String> HeaderLines = new List<string>();
            List<String> HeaderLinesA = new List<string>();
            List<String> HeaderLinesB = new List<string>();
            int skip = 4;
            int total = 256 / skip;
            HeaderLines.Add(String.Format("#define SHAPECOMPENSATIONCOUNT {0}", total));
            HeaderLines.Add("");
            HeaderLinesB.Add(String.Format("int const BasicShapeMult[SHAPECOMPENSATIONCOUNT] = {{"));
            HeaderLinesA.Add(String.Format("int const BasicShapeLow[SHAPECOMPENSATIONCOUNT] = {{"));

            string L = "";
            string L2 = "";

            for (int i = 0; i < 256; i += skip)
            {
                Shape.Value = 0;
                Phase.Value = 64;
                Speed.Value = 255;
                Mod.Value = i;

                RebuildLFO(true);
                Int64 vmin = values64[0];
                Int64 vmax = values64[0];
                for (int j = i; j < 5000; j++)
                {
                    if (values64[j] < vmin) vmin = values64[j];
                    else
                    {
                        if (values64[j] > vmax) vmax = values64[j];
                    }
                }
                if (L.Length == 0) L += "\t";
                if (L2.Length == 0) L2 += "\t";
                L += String.Format("{0}", (vmin + vmax) / 2);
                Int64 range = vmax - vmin;

                L2 += String.Format("{0}", (vmax - vmin));
                if (i < 256 - skip) { L += ", "; L2 += ", "; };
                if ((i / skip) % 8 == 7)
                {

                    HeaderLinesB.Add(L2);
                    HeaderLinesA.Add(L);
                    L = "";
                    L2 = "";

                }
                Console.WriteLine("{0} : {1} -> {2}", i, vmin, vmax);
            }
            if (L2.Length > 0) HeaderLinesB.Add(L2);
            if (L.Length > 0) HeaderLinesA.Add(L);

            HeaderLinesA.Add("};");
            HeaderLinesA.Add("");
            HeaderLinesB.Add("};");
            HeaderLines.AddRange(HeaderLinesA);
            HeaderLines.AddRange(HeaderLinesB);

            System.IO.File.WriteAllLines("BasicShapeCompensation.h", HeaderLines.ToArray());


            for (int i = 0; i < 256; i++)
            {
                long P = TestFrameLoader.GetLFOPhaseInc(i);
                float F = (0xffffffff / P) / 2000.0f; ;
                Console.WriteLine("{0:X} -> {1}", P, F);
            }

        }


        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        void BuildPoster()
        {
            List<Polygon> Lines = new List<Polygon>();
            SVGWriter S = new SVGWriter();

            int C = 0;

            int bR1 = 1;
            int bG1 = 58;
            int bB1 = 66;

            int R1 = 21;
            int G1 = 142;
            int B1 = 160;

            double h1, h2, s1, s2, v1, v2;


            int R2 = 255;
            int G2 = 234;
            int B2 = 0;


            ColorToHSV(Color.FromArgb(R1, G1, B1), out h1, out s1, out v1);
            ColorToHSV(Color.FromArgb(R2, G2, B2), out h2, out s2, out v2);


            int qmax = 8;
            int width = 128;
            for (int q = 0; q < qmax; q++)
            {
                Console.WriteLine("column {0}/{1}", q + 1, qmax);
                for (float i = 0; i < 256; i += 2)
                {

                    Shape.Value = (int)(i);
                    Phase.Value = (int)((i/5) %256);
                    Speed.Value = 255;
                    float envelopeoffset = (float)(1 - Math.Abs(((float)i / 255 - 0.5))*2) * 14;
                    if (envelopeoffset < 0) envelopeoffset = 0;
                    Mod.Value = Math.Min((((q * 255) / (qmax - 1))) + (int)envelopeoffset,255);
                    float Ybase = i * 5;
                    float H = 30;

                    RebuildLFO(true,2*4*6*width);
                    Polygon P = new Polygon();
                    Polygon P2 = new Polygon() { depth = 2 };
                    float targetmix = q / (float)(qmax - 1);
                    float br = 1.0f;// (i / 512.0f) + 0.5f;
                    double h, s, v;
                    h = h1 + (h2 - h1) * targetmix;
                    s = s1 + (s2 - s1) * targetmix;
                    v = v1 + (v2 - v1) * targetmix;
                    var Co = ColorFromHSV(h, s, v);
                    P.r = (byte)((R1 + (R2 - R1) * targetmix) * br);
                    P.g = (byte)((G1 + (G2 - G1) * targetmix) * br);
                    P.b = (byte)((B1 + (B2 - B1) * targetmix) * br);
                    // P.r = Co.R;
                    // P.g = Co.G;
                    // P.b = Co.B;

                    for (int j = 0; j < width; j++)
                    {
                        P.Vertices.Add(new GlmNet.vec2(j + q * width, Ybase + (float)linvalues2[(int)(j * 4 * 8.5)] * H));
                        // P2.Vertices.Add(new GlmNet.vec2(j, Ybase + (float)linvalues2[j * 4] * H));
                    }
                    Lines.Add(P);
                    // Lines.Add(P2);
                    C++;
                }
            }
            SVGWriter.Write("wobbler_poster-" + DateTime.Now.ToLongDateString() + ".svg", 600 * 4, 256 * 10, Lines, 1, false, Color.FromArgb(0, 58, 66));
        }

        private void Attack_Scroll(object sender, EventArgs e)
        {
            RebuildLFO();
            pictureBox1.Invalidate();
        }

        private void Decay_Scroll(object sender, EventArgs e)
        {
            RebuildLFO();

            pictureBox1.Invalidate();
        }

        private void Sustain_Scroll(object sender, EventArgs e)
        {
            RebuildLFO();
            pictureBox1.Invalidate();
        }

        private void Release_Scroll(object sender, EventArgs e)
        {
            RebuildLFO();
            pictureBox1.Invalidate();
        }

        private void RebuildLFO(bool force = false, int defcount=5000)
        {
            int count = defcount;
            if (force == false) count = Math.Min(count, pictureBox1.Width);
            TestFrameLoader.ResetStatic();
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    TestFrameLoader.LFOTrigger(1, 1);
                }

                if (i == 1)
                {
                    TestFrameLoader.LFOTrigger(0, 1);
                }

                var V = TestFrameLoader.GetLFO(1, Speed.Value, Shape.Value << 8, Mod.Value << 8, Phase.Value << 4);
                var V2 = TestFrameLoader.GetLFOBasicShape(1);

                values64[i] = V2;
                values2[i] = V / 4096.0f;
                linvalues2[i] = TestFrameLoader.GetLFOPhased(1)/4096.0f;
            }
        }

        private void Curvature_Scroll(object sender, EventArgs e)
        {
            RebuildLFO();
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            List<PointF> P = new List<PointF>(pictureBox1.Width);
            List<PointF> P2 = new List<PointF>(pictureBox1.Width);
            List<PointF> P3 = new List<PointF>(pictureBox1.Width);
            List<PointF> P4 = new List<PointF>(pictureBox1.Width);
            List<PointF> P5 = new List<PointF>(pictureBox1.Width);

            for (int i = 0; i < pictureBox1.Width; i++)
            {
                int p1 = (i + pos + 4000 - pictureBox1.Width) % 4000;
                P.Add(new PointF(i, ScaleVal(values[p1])));
                P5.Add(new PointF(i, ScaleVal(valuesref[p1])));
                P2.Add(new PointF(i, ScaleVal(linvalues[p1])));
                P3.Add(new PointF(i, (pictureBox1.Height / 2) + ScaleVal(values2[i])));
                P4.Add(new PointF(i, (pictureBox1.Height / 2) + ScaleVal(linvalues2[i])));
            }

            g.DrawLines(new Pen(Color.Blue, 1.0f), P5.ToArray());
            g.DrawLines(new Pen(Color.Green, 1.0f), P2.ToArray());
            g.DrawLines(new Pen(Color.Yellow, 1.0f), P.ToArray());
            g.DrawLines(new Pen(Color.Green, 1.0f), P4.ToArray());
            g.DrawLines(new Pen(Color.Yellow, 1.0f), P3.ToArray());

            g.DrawLine(new Pen(Color.BlueViolet), new PointF(0, ScaleVal(0.5)), new PointF(pictureBox1.Width - 1, ScaleVal(0.5)));
            g.DrawLine(new Pen(Color.Lime), new PointF(0, ScaleVal(0.95)), new PointF(pictureBox1.Width - 1, ScaleVal(0.95)));
            g.DrawLine(new Pen(Color.Lime), new PointF(0, ScaleVal(0.05)), new PointF(pictureBox1.Width - 1, ScaleVal(0.05)));

            g.DrawLine(new Pen(Color.BlueViolet), new PointF(0, (pictureBox1.Height / 2) + ScaleVal(0.5)), new PointF(pictureBox1.Width - 1, (pictureBox1.Height / 2) + ScaleVal(0.5)));

            for (int i = 0; i < 2; i++)
            {
                int B = TestFrameLoader.GetLFOGate(i);
                if (B > 0) B = 255;
                var C = Color.FromArgb(B, B, 0);
                Rectangle R = new Rectangle();
                R.Width = 10;
                R.Height = 10;
                R.X = pictureBox1.Width - 12 - i * 12;
                R.Y = 2;
                g.FillRectangle(new SolidBrush(C), R);
                g.DrawRectangle(new Pen(Color.White, 1), R);
            }

            for (int i = 0; i < 18; i++)
            {
                int B = Math.Min(255, TestFrameLoader.GetLFOLed(i));
                var C = Color.FromArgb(B, B, 0);
                Rectangle R = new Rectangle();
                R.Width = 10;
                R.Height = 10;
                R.X = 2 + i * 12;
                R.Y = 2;
                g.FillRectangle(new SolidBrush(C), R);
                g.DrawRectangle(new Pen(Color.White, 1), R);
            }
        }

        float ScaleVal(double inp)
        {
            return 20 + (float)((pictureBox1.Height / 2 - 40) * (1.0 - inp));
        }

        List<double> values = new List<double>(120000);
        List<double> valuesref = new List<double>(120000);
        List<double> linvalues = new List<double>(120000);

        List<double> values2 = new List<double>(120000);
        List<Int64> values64 = new List<Int64>(120000);
        List<double> linvalues2 = new List<double>(120000);
        int pos = 0;

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            RebuildLFO();
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void EnvUpdate_Tick(object sender, EventArgs e)
        {
            pos = (pos + 1) % 4000;

            for (int i = 0; i < 10; i++)
            {
                double D = 0;
                values[pos] = TestFrameLoader.GetLFO(0, Speed.Value , Shape.Value << 8, Mod.Value << 8, Phase.Value << 4) / 4096.0f;
                D = TestFrameLoader.GetLFOPhased(0) / 4096.0f;
                valuesref[pos] = (TestFrameLoader.GetLFOBasicShape(0) / 65536.0f) / (4096.0f * 4) + 0.50f;
                linvalues[pos] = D;
            }
        }

        private void DispUpdate_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            TestFrameLoader.LFOTrigger(1, 0);
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            TestFrameLoader.LFOTrigger(0, 0);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
