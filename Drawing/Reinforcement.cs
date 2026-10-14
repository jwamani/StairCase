using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace StairCase
{
    public static class Reinforcement
    {
        public static void Draw(Transaction tr, BlockTableRecord modelSpace, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            DrawB1(tr, modelSpace, p, g, layer, o);
            DrawB2(tr, modelSpace, p, g, layer, o);
            DrawB3(tr, modelSpace, p, g, layer, o);
            DrawB4(tr, modelSpace, p, g, layer, o);
            DrawB5(tr, modelSpace, p, g, layer, o);
            DrawLandingDonuts(tr, modelSpace, layer, p, g, o);
            DrawFlightDonuts(tr, modelSpace, layer, p, g, o);
            DrawB1Donuts(tr, modelSpace, layer, p, g, o);
            DrawB2Donuts(tr, modelSpace, layer, p, g, o);
        }

        static void DrawB1(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double arc = Math.Tan(22.5 * Math.PI / 180.0);
            double r = StairConstants.CornerRadius;

            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double topX = g.FlightStart + (kLow * g.Tread);
            double topCornerY = kLow * g.Riser;
            double topPX = topX + (cover * g.Riser / g.StepLength);
            double topPY = topCornerY - (cover * g.Tread / g.StepLength);

            double legX = g.FlightStart - 40;

            double footY = g.BarY(legX, cover);
            double inX = -g.Tread / g.StepLength;
            double inY = -g.Riser / g.StepLength;
            double footTurn = Math.Atan2((inX * -1.0) - (inY * 0.0), (inX * 0.0) + (inY * -1.0));
            double footT = r * Math.Tan(Math.Abs(footTurn) / 2.0);

            double yBot = g.ThickeningBottomY + cover;

            double upX = g.ThickeningFlatEndX - cover;
            double crossY = g.BarY(upX, g.Waist - cover);

            var pts = new List<Point2d>
            {
                new Point2d(topPX, topPY),
                new Point2d(legX - (inX * footT), footY - (inY * footT)),
                new Point2d(legX, footY - footT),
                new Point2d(legX, yBot + r),
                new Point2d(legX + r, yBot),
                new Point2d(upX - r, yBot),
                new Point2d(upX, yBot + r),
                new Point2d(upX, crossY + (3 * cover))
            };

            var bulges = new List<double> { 0, Math.Tan(footTurn / 4.0), 0, arc, 0, arc, 0, 0 };

            PolylineBends(tr, ms, layer, pts, bulges, o);
        }

        static void DrawB2(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double arc = Math.Tan(22.5 * Math.PI / 180.0);
            double r = StairConstants.CornerRadius;

            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double lowX = g.FlightStart + ((g.NumTreads - kLow) * g.Tread);
            double off = cover;
            double lowCornerY = (g.NumTreads - kLow) * g.Riser;
            double lowPX = lowX + (cover * g.Riser / g.StepLength);
            double lowPY = lowCornerY - (cover * g.Tread / g.StepLength);

            double yBar = g.LandingSoffitY + cover;
            double bendX = g.BarX(yBar, off);

            double endX = g.LandingEndX - cover;

            var pts = new List<Point2d>
            {
                new Point2d(lowPX, lowPY),
                new Point2d(bendX, yBar),
                new Point2d(endX - r, yBar),
                new Point2d(endX, yBar + r),
                new Point2d(endX, yBar + (2 * cover))
            };

            var bulges = new List<double> { 0, 0, arc, 0, 0 };

            PolylineBends(tr, ms, layer, pts, bulges, o);
        }

        static void DrawB3(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = 0.6 * cover;
            double arc = Math.Tan(22.5 * Math.PI / 180.0);
            double kick = StairConstants.KickSize;

            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double lowX = g.FlightStart + ((g.NumTreads - kLow + 0.6) * g.Tread);
            double lowCornerY = g.StepCornerY(lowX);

            double tailY = g.LandingTopY - cover;
            double bendX = g.BarX(tailY, off);

            double tailX = bendX + (0.48 * p.LandingLength);

            var pts = new List<Point2d>
            {
                new Point2d(
                    lowX + (off * g.Riser / g.StepLength),
                    lowCornerY - (off * g.Tread / g.StepLength)),
                new Point2d(bendX, tailY),
                new Point2d(tailX, tailY),
                new Point2d(tailX + kick, tailY - kick)
            };

            var bulges = new List<double> { 0, 0, -arc, 0 };

            PolylineBends(tr, ms, layer, pts, bulges, o);
        }

        static void DrawB4(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = g.Waist - cover;

            double tailY = g.LandingTopY - cover;
            double bendX = g.BarX(tailY, off);
            double tailEndX = bendX + (0.3 * p.LandingLength);

            double upX = g.ThickeningFlatEndX - cover;
            double crossY = g.BarY(upX, off);
            double stub = 3 * cover;
            double kick = StairConstants.KickSize;
            double endX = upX - (stub * g.Tread / g.StepLength);
            double endY = crossY - (stub * g.Riser / g.StepLength);

            double tailKickX = tailEndX + kick;
            double tailKickY = tailY - kick;

            var pts = new List<Point2d>
            {
                new Point2d(tailKickX, tailKickY),
                new Point2d(tailEndX, tailY),
                new Point2d(bendX, tailY),
                new Point2d(endX, endY)
            };

            var bulges = new List<double> { 0, 0, 0, 0 };

            PolylineBends(tr, ms, layer, pts, bulges, o);
        }

        static void DrawB5(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double arc = Math.Tan(22.5 * Math.PI / 180.0);
            double r = StairConstants.CornerRadius;
            double hook = 2 * cover;

            double barY = g.LandingTopY - cover;
            double leftX = g.FlightEndX + cover;
            double rightX = g.LandingEndX - cover;

            var pts = new List<Point2d>
            {
                new Point2d(leftX, barY - hook),
                new Point2d(leftX, barY - r),
                new Point2d(leftX + r, barY),
                new Point2d(rightX - r, barY),
                new Point2d(rightX, barY - r),
                new Point2d(rightX, barY - hook)
            };

            var bulges = new List<double> { 0, -arc, 0, -arc, 0, 0 };

            PolylineBends(tr, ms, layer, pts, bulges, o);
        }

        static void DrawLandingDonuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double r = StairConstants.CornerRadius;
            double off = StairConstants.DonutOffset;

            double firstX = g.FlightEndX + cover + r;
            double lastX = g.BeamStartX - cover;
            double topY = g.LandingTopY - cover - off;
            double botY = g.LandingSoffitY + cover + off;

            int n = Math.Max(2, (int)Math.Ceiling((lastX - firstX) / p.DonutSpacing) + 1);
            double s = (lastX - firstX) / (n - 1);

            for (int i = 0; i < n; i++)
            {
                double x = firstX + (i * s);
                Donut(tr, ms, layer, x, topY, o);
                Donut(tr, ms, layer, x, botY, o);
            }
        }

        static void DrawFlightDonuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = StairConstants.DonutOffset;
            double zone = g.Waist - cover;

            double x0 = g.ThickeningFlatEndX - cover - off + (off * g.Riser / g.StepLength);
            double y0 = g.BarY(x0, zone);
            double x1 = g.BarX(g.LandingSoffitY, zone);
            double y1 = g.BarY(x1, zone);

            double run = Math.Sqrt(((x1 - x0) * (x1 - x0)) + ((y1 - y0) * (y1 - y0)));
            int n = Math.Max(2, (int)Math.Ceiling(run / p.Tread) + 1);

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / (n - 1);
                double x = x0 + ((x1 - x0) * t);
                double y = y0 + ((y1 - y0) * t);

                Donut(tr, ms, layer,
                    x - (off * g.Riser / g.StepLength),
                    y + (off * g.Tread / g.StepLength), o);
            }
        }

        static void DrawB1Donuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = StairConstants.DonutOffset;
            double ux = g.Tread / g.StepLength;
            double uy = g.Riser / g.StepLength;

            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double cx = g.FlightStart + (kLow * g.Tread);
            double px = cx + (cover * g.Riser / g.StepLength);
            double py = (kLow * g.Riser) - (cover * g.Tread / g.StepLength);

            double legX = g.FlightStart - 40;
            double qx = legX;
            double qy = g.BarY(legX, cover);

            double x0 = px - (cover * ux);
            double y0 = py - (cover * uy);

            double dot = (ux * 0.0) + (uy * -1.0);
            double sinHalf = Math.Sqrt((1.0 - dot) / 2.0);
            double bx = ux + 0.0;
            double by = uy - 1.0;
            double bl = Math.Sqrt((bx * bx) + (by * by));
            double bd = off / sinHalf;
            double endX = qx + (bd * bx / bl);
            double endY = qy + (bd * by / bl);

            double run = Math.Sqrt(((qx - x0) * (qx - x0)) + ((qy - y0) * (qy - y0)));
            int n = Math.Max(2, (int)Math.Ceiling(run / p.Tread) + 1);

            for (int i = 0; i < n - 1; i++)
            {
                double t = (double)i / (n - 1);
                double x = x0 + ((qx - x0) * t);
                double y = y0 + ((qy - y0) * t);

                Donut(tr, ms, layer,
                    x + (off * g.Riser / g.StepLength),
                    y - (off * g.Tread / g.StepLength), o);
            }

            Donut(tr, ms, layer, endX, endY, o);

            double r = StairConstants.CornerRadius;
            double yBot = g.ThickeningBottomY + cover;
            double upX = g.ThickeningFlatEndX - cover;

            Donut(tr, ms, layer, legX + r, yBot + r, o);
            Donut(tr, ms, layer, (legX + upX) / 2, yBot + r, o);
            Donut(tr, ms, layer, upX - r, yBot + r, o);
        }
        static void DrawB2Donuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = StairConstants.DonutOffset;
            double ux = g.Tread / g.StepLength;
            double uy = g.Riser / g.StepLength;

            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double lowX = g.FlightStart + ((g.NumTreads - kLow) * g.Tread);
            double lowCornerY = (g.NumTreads - kLow) * g.Riser;
            double x0 = lowX + (cover * g.Riser / g.StepLength);
            double y0 = lowCornerY - (cover * g.Tread / g.StepLength);

            double yBar = g.LandingSoffitY + cover;
            double x1 = g.BarX(yBar, cover) - (cover * ux);
            double y1 = yBar - (cover * uy);

            double run = Math.Sqrt(((x1 - x0) * (x1 - x0)) + ((y1 - y0) * (y1 - y0)));
            int n = Math.Max(2, (int)Math.Ceiling(run / p.Tread) + 1);

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / (n - 1);
                double x = x0 + ((x1 - x0) * t);
                double y = y0 + ((y1 - y0) * t);

                Donut(tr, ms, layer,
                    x + (off * g.Riser / g.StepLength),
                    y - (off * g.Tread / g.StepLength), o);
            }
        }

        static void Donut(Transaction tr, BlockTableRecord ms, ObjectId layer, double x, double y, Point2d o)
        {
            double half = StairConstants.DonutLength / 2;

            var donut = new Polyline();

            donut.AddVertexAt(0, new Point2d(x + o.X - half, y + o.Y), -1.0,
                StairConstants.DonutWidth, StairConstants.DonutWidth);

            donut.AddVertexAt(1, new Point2d(x + o.X + half, y + o.Y), -1.0,
                StairConstants.DonutWidth, StairConstants.DonutWidth);

            donut.Closed = true;
            donut.LayerId = layer;
            donut.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.RebarColor);

            ms.AppendEntity(donut);
            tr.AddNewlyCreatedDBObject(donut, true);
        }

        static void PolylineBends(Transaction tr, BlockTableRecord ms, ObjectId layer, List<Point2d> pts, List<double> bulges, Point2d o)
        {
            var polyline = new Polyline();

            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, new Point2d(pts[i].X + o.X, pts[i].Y + o.Y), bulges[i], 0, 0);
            }

            polyline.LayerId = layer;
            polyline.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.RebarColor);

            ms.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }
    }
}