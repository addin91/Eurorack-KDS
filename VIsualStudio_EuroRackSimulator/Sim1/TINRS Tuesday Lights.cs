﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sim1
{
    public partial class TINRS_Tuesday_Lights : WeifenLuo.WinFormsUI.Docking.DockContent
    {

        public TINRS_Tuesday_Lights()
        {
            InitializeComponent();
        }

        public void DrawSquare(Graphics G, int x, int y, bool filled)
        {
            if (filled)
            {
                G.FillRectangle(new SolidBrush(Color.Yellow), x, y, minisquare, minisquare);
            }
            G.DrawRectangle(new Pen(Color.White, 1.0f), x, y, minisquare, minisquare);
        }


        public void Draw4Square(Graphics G, int x, int y, int num)
        {
            DrawSquare(G, x, y, num == 3);
            DrawSquare(G, x + minisquare, y, num == 0);
            DrawSquare(G, x + minisquare, y + minisquare, num == 1);
            DrawSquare(G, x, y + minisquare, num == 2);
        }
        int margin = 5;
        const int minisquare = 10;
        int squaresize = minisquare * 2;

        public void DrawSquares(int skip, Graphics G, int x, int y, int num, string label)
        {

            int N1 = num & 3;
            int N2 = (num >> 2) & 3;
            int N3 = (num >> 4) & 3;
            List<int> Ns = new List<int>() { N1, N2, N3 };
            int j = 0;
            int i = 0;
            if (i != skip) { Draw4Square(G, x + squaresize + margin, y, Ns[j]); j++; };
            i++;
            if (i != skip) { Draw4Square(G, x + squaresize + margin, y + squaresize + margin, Ns[j]); j++; };
            i++;
            if (i != skip) { Draw4Square(G, x, y + squaresize + margin, Ns[j]); j++; };
            i++;
            if (i != skip) { Draw4Square(G, x, y, Ns[j]); j++; };
            i++;


            G.DrawString(label, new Font("Panton ExtraBold", 15), new SolidBrush(Color.White), x + squaresize * 2 + margin * 2, y + squaresize - 7.5f);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        List<string> PatternNames = new List<string>()
        {
            "Tests",

            "TriTrance",

            // fishyfishy! with slides! 
            "Stomper",

            // Random walking with mr. Markov.
            "Markov",

            //	ALGO_PACHEDECO,
            "Wobble",
            "Chip 1",
            "Chip 2",

            "SnH",
            // classic saiko things, reimagined
            "Saiko Classic",
            "Saiko Lead",
            "Scalewalker",
            "TooEasy",
            "Random"
          };
        List<string> ScaleNames = new List<string>()
        {
            "Major",
            "Minor",
            "Dorian",
            "Blues",
            "Pentatonic",
            "12 tone" ,
            "Major Triad",
            "Minor Triad"
        };
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            Graphics G = e.Graphics;
            int s = Math.Max(1, (int)Math.Floor(pictureBox1.Width / 300.0));
            int y = 0;
            for (int i = 0; i < PatternNames.Count; i++)
            {
                y = 10 + (i / s) * (squaresize * 2 + margin * 6);
                DrawSquares(3, G, (i % s) * 300 + 10, y, i, PatternNames[i]);


            }
            y += (squaresize * 2 + margin * 6);
            for (int i = 0; i < ScaleNames.Count; i++)
            {
                int ny = 10 + (i / s) * (squaresize * 2 + margin * 6);
                DrawSquares(2, G, (i % s) * 300 + 10, y + ny, i, ScaleNames[i]);
            }

        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void TINRS_Tuesday_Lights_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
