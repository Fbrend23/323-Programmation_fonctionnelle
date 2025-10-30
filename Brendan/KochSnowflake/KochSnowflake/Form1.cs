using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KochSnowflake
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = chkAntialias.Checked ? SmoothingMode.AntiAlias : SmoothingMode.None;

            // Triangle équilatéral adapté au panel
            var bounds = drawingPanel.ClientRectangle;
            if (bounds.Width < 10 || bounds.Height < 10) return;

            float margin = 30f;
            float w = bounds.Width - 2 * margin;
            float h = bounds.Height - 2 * margin;
            float side = Math.Min(w, (float)(2.0 * h / Math.Sqrt(3.0f)));

            float baseLeftX = bounds.Left + (bounds.Width - side) / 2f;
            float baseY = bounds.Bottom - margin;

            // Points du triangle équilatéral (comme avant)
            PointF p1 = new(baseLeftX, baseY);
            PointF p2 = new(baseLeftX + side, baseY);
            PointF p3 = new(baseLeftX + side / 2f, baseY - (float)(Math.Sqrt(3) * side / 2.0));

            // Centre (barycentre) du triangle = "intérieur"
            PointF center = new((p1.X + p2.X + p3.X) / 3f, (p1.Y + p2.Y + p3.Y) / 3f);

            int depth = (int)numDepth.Value;
            using var pen = new Pen(Color.RoyalBlue, 1.2f);

            // On passe 'center' pour orienter les pics vers l'extérieur
            DrawKochCurve(g, pen, p1, p2, depth, center);
            DrawKochCurve(g, pen, p2, p3, depth, center);
            DrawKochCurve(g, pen, p3, p1, depth, center);

        }

        private void numDepth_ValueChanged(object sender, EventArgs e) => drawingPanel.Invalidate();
        private void chkAntialias_CheckedChanged(object sender, EventArgs e) => drawingPanel.Invalidate();

        // --- Récursion Koch ---
        private void DrawKochCurve(Graphics g, Pen basePen, PointF A, PointF B, int depth, PointF interiorRef)
        {
            if (depth == 0)
            {
                g.DrawLine(basePen, A, B);
                return;
            }

            // Tiers
            var AB = new PointF(B.X - A.X, B.Y - A.Y);
            var v = new PointF(AB.X / 3f, AB.Y / 3f);
            var P1 = new PointF(A.X + v.X, A.Y + v.Y);
            var P2 = new PointF(A.X + 2f * v.X, A.Y + 2f * v.Y);

            // Milieu du segment central
            var M = new PointF((P1.X + P2.X) / 2f, (P1.Y + P2.Y) / 2f);

            // Perpendiculaire unitaire à AB (gauche de A->B)
            double lenAB = Math.Sqrt(AB.X * AB.X + AB.Y * AB.Y);
            if (lenAB == 0) return;
            var uPerpLeft = new PointF(-(float)(AB.Y / lenAB), (float)(AB.X / lenAB));

            // Hauteur du petit triangle équilatéral (sur le tiers) : |AB| * sqrt(3) / 6
            float h = (float)(lenAB * Math.Sqrt(3) / 6.0);

            // Vecteur depuis M vers l'intérieur (référence)
            var toInterior = new PointF(interiorRef.X - M.X, interiorRef.Y - M.Y);

            // Si la perpendiculaire pointe vers l'intérieur, on prend l’opposé (on veut l’extérieur)
            float dot = uPerpLeft.X * toInterior.X + uPerpLeft.Y * toInterior.Y;
            var outward = (dot > 0)
                ? new PointF(-uPerpLeft.X * h, -uPerpLeft.Y * h)
                : new PointF(uPerpLeft.X * h, uPerpLeft.Y * h);

            var Ppeak = new PointF(M.X + outward.X, M.Y + outward.Y);

            using var pen = new Pen(DepthColor(depth), basePen.Width);
            DrawKochCurve(g, pen, A, P1, depth - 1, interiorRef);
            DrawKochCurve(g, pen, P1, Ppeak, depth - 1, interiorRef);
            DrawKochCurve(g, pen, Ppeak, P2, depth - 1, interiorRef);
            DrawKochCurve(g, pen, P2, B, depth - 1, interiorRef);
        }


        private static PointF Rotate(PointF v, double degrees)
        {
            double rad = degrees * Math.PI / 180.0;
            double c = Math.Cos(rad);
            double s = Math.Sin(rad);
            return new PointF((float)(v.X * c - v.Y * s), (float)(v.X * s + v.Y * c));
        }
        private static Color DepthColor(int depth)
        {
            depth = Math.Max(0, Math.Min(10, depth));
            int r = 30 + 12 * (10 - depth);
            int g = 60 + 6 * depth;
            int b = 200 - 2 * depth;
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }

    }
}
