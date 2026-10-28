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
            DrawB1Donuts(tr, modelSpace, layer, p, g, o);
            DrawB2Donuts(tr, modelSpace, layer, p, g, o);
        }

        static void DrawB1(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double arc = Math.Tan(22.5 * Math.PI / 180.0);
            double r = StairConstants.CornerRadius;

            int kLow = Math.Max(1, (int)Math.Round(g.NumTreads / 3.0));
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

            int kLow = Math.Max(1, (int)Math.Round(g.NumTreads / 3.0));
            double lowX = g.FlightStart + ((g.NumTreads - kLow) * g.Tread);
            double off = cover;
            double lowCornerY = (g.NumTreads - kLow) * g.Riser;
            double lowPX = lowX + (cover * g.Riser / g.StepLength);
            double lowPY = lowCornerY - (cover * g.Tread / g.StepLength);

            double yBar = g.LandingSoffitY + cover;
            double bendX = g.BarX(yBar, off);

            double endX = g.LandingEndX - cover;
            double hook = p.LandingThickness / 4;

            var pts = new List<Point2d>
            {
                new Point2d(lowPX, lowPY),
                new Point2d(bendX, yBar),
                new Point2d(endX - r, yBar),
                new Point2d(endX, yBar + r),
                new Point2d(endX, yBar + hook)
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

            int kLow = Math.Max(1, (int)Math.Round(g.NumTreads / 3.0));
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
            double hook = p.LandingThickness / 4;

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

        static void DrawB1Donuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = StairConstants.DonutOffset;
            double ux = g.Tread / g.StepLength;
            double uy = g.Riser / g.StepLength;

            int kLow = Math.Max(1, (int)Math.Round(g.NumTreads / 3.0));
            double cx = g.FlightStart + (kLow * g.Tread);
            double px = cx + (cover * g.Riser / g.StepLength);
            double py = (kLow * g.Riser) - (cover * g.Tread / g.StepLength);

            Donut(tr, ms, layer,
                px + (off * g.Riser / g.StepLength),
                py - (off * g.Tread / g.StepLength), o);

            double legX = g.FlightStart - 40;
            double qx = legX;
            double qy = g.BarY(legX, cover);

            double dot = (ux * 0.0) + (uy * -1.0);
            double sinHalf = Math.Sqrt((1.0 - dot) / 2.0);
            double bx = ux + 0.0;
            double by = uy - 1.0;
            double bl = Math.Sqrt((bx * bx) + (by * by));
            double bd = off / sinHalf;
            double endX = qx + (bd * bx / bl);
            double endY = qy + (bd * by / bl);

            Donut(tr, ms, layer, endX, endY, o);

            double r = StairConstants.CornerRadius;
            double yBot = g.ThickeningBottomY + cover;
            double upX = g.ThickeningFlatEndX - cover;

            Donut(tr, ms, layer, legX + r, yBot + r, o);
            Donut(tr, ms, layer, (legX + upX) / 2, yBot + r, o);
            Donut(tr, ms, layer, upX - r, yBot + r, o);

            double zone = g.Waist - cover;
            double ix0 = g.ThickeningFlatEndX - cover - off + (off * g.Riser / g.StepLength);
            double iy0 = g.BarY(ix0, zone);
            double ix = ix0 - (off * g.Riser / g.StepLength);
            double iy = iy0 + (off * g.Tread / g.StepLength);

            double t = ((ix - px) * ux) + ((iy - py) * uy);
            double footX = px + (t * ux);
            double footY = py + (t * uy);

            Donut(tr, ms, layer,
                footX + (off * g.Riser / g.StepLength),
                footY - (off * g.Tread / g.StepLength), o);

            double zone1X = footX + (off * g.Riser / g.StepLength);
            double zone1Y = footY - (off * g.Tread / g.StepLength);
            double zone1Dist = Math.Sqrt(((endX - zone1X) * (endX - zone1X)) + ((endY - zone1Y) * (endY - zone1Y)));
            int zone1N = Math.Max(2, (int)Math.Ceiling(zone1Dist / p.DonutSpacing) + 1);

            for (int i = 1; i < zone1N - 1; i++)
            {
                double u = (double)i / (zone1N - 1);
                Donut(tr, ms, layer,
                    zone1X + ((endX - zone1X) * u),
                    zone1Y + ((endY - zone1Y) * u), o);
            }

            double zone2X = px + (off * g.Riser / g.StepLength);
            double zone2Y = py - (off * g.Tread / g.StepLength);
            double zone2Dist = Math.Sqrt(
                ((zone1X - zone2X) * (zone1X - zone2X)) +
                ((zone1Y - zone2Y) * (zone1Y - zone2Y)));
            int zone2N = Math.Max(2, (int)Math.Ceiling(zone2Dist / p.DonutSpacing) + 1);

            for (int i = 1; i < zone2N - 1; i++)
            {
                double u = (double)i / (zone2N - 1);
                Donut(tr, ms, layer,
                    zone2X + ((zone1X - zone2X) * u),
                    zone2Y + ((zone1Y - zone2Y) * u), o);
            }

            double drop = g.Waist - (2 * cover) - (2 * off);
            double dropX = drop * g.Riser / g.StepLength;
            double dropY = -drop * g.Tread / g.StepLength;

            double sepPairX = zone1X + dropX;
            double sepPairY = zone1Y + dropY;
            double b1PairX = zone2X + dropX;
            double b1PairY = zone2Y + dropY;

            Donut(tr, ms, layer, sepPairX, sepPairY, o);

            double botDist = Math.Sqrt(
                ((b1PairX - sepPairX) * (b1PairX - sepPairX)) +
                ((b1PairY - sepPairY) * (b1PairY - sepPairY)));
            int botN = Math.Max(2, (int)Math.Ceiling(botDist / p.DonutSpacing) + 1);

            for (int i = 1; i < botN - 1; i++)
            {
                double u = (double)i / (botN - 1);
                Donut(tr, ms, layer,
                    sepPairX + ((b1PairX - sepPairX) * u),
                    sepPairY + ((b1PairY - sepPairY) * u), o);
            }
        }
        static void DrawB2Donuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g, Point2d o)
        {
            double cover = StairConstants.BarDrop;
            double off = StairConstants.DonutOffset;
            double ux = g.Tread / g.StepLength;
            double uy = g.Riser / g.StepLength;

            int kLow = Math.Max(1, (int)Math.Round(g.NumTreads / 3.0));
            double lowX = g.FlightStart + ((g.NumTreads - kLow) * g.Tread);
            double lowCornerY = (g.NumTreads - kLow) * g.Riser;
            double x0 = lowX + (cover * g.Riser / g.StepLength);
            double y0 = lowCornerY - (cover * g.Tread / g.StepLength);

            double b1StartX = g.FlightStart + (kLow * g.Tread) + (cover * g.Riser / g.StepLength) + (off * g.Riser / g.StepLength);
            double b1StartY = (kLow * g.Riser) - (cover * g.Tread / g.StepLength) - (off * g.Tread / g.StepLength);
            double b2StartX = x0 + (off * g.Riser / g.StepLength);
            double b2StartY = y0 - (off * g.Tread / g.StepLength);

            double drop = g.Waist - (2 * cover) - (2 * off);
            double dropX = drop * g.Riser / g.StepLength;
            double dropY = -drop * g.Tread / g.StepLength;

            double b1PairX = b1StartX + dropX;
            double b1PairY = b1StartY + dropY;
            double b2PairX = b2StartX + dropX;
            double b2PairY = b2StartY + dropY;

            double zone3Dist = Math.Sqrt(
                ((b2PairX - b1PairX) * (b2PairX - b1PairX)) +
                ((b2PairY - b1PairY) * (b2PairY - b1PairY)));
            int zone3N = Math.Max(2, (int)Math.Ceiling(zone3Dist / p.DonutSpacing) + 1);

            for (int i = 0; i < zone3N; i++)
            {
                double u = (double)i / (zone3N - 1);
                Donut(tr, ms, layer,
                    b1PairX + ((b2PairX - b1PairX) * u),
                    b1PairY + ((b2PairY - b1PairY) * u), o);
            }

            double yBar = g.LandingSoffitY + cover;
            double x1 = g.BarX(yBar, cover) - (cover * ux);
            double y1 = yBar - (cover * uy);

            double topEndX = x1 + (off * g.Riser / g.StepLength);
            double topEndY = y1 - (off * g.Tread / g.StepLength);
            double bendPairX = topEndX + dropX;
            double bendPairY = topEndY + dropY;

            double zone4Dist = Math.Sqrt(
                ((topEndX - b2StartX) * (topEndX - b2StartX)) +
                ((topEndY - b2StartY) * (topEndY - b2StartY)));
            int zone4N = Math.Max(2, (int)Math.Ceiling(zone4Dist / p.DonutSpacing) + 1);

            for (int i = 0; i < zone4N; i++)
            {
                double u = (double)i / (zone4N - 1);
                Donut(tr, ms, layer,
                    b2StartX + ((topEndX - b2StartX) * u),
                    b2StartY + ((topEndY - b2StartY) * u), o);
            }

            for (int i = 1; i < zone4N - 1; i++)
            {
                double u = (double)i / (zone4N - 1);
                Donut(tr, ms, layer,
                    b2PairX + ((bendPairX - b2PairX) * u),
                    b2PairY + ((bendPairY - b2PairY) * u), o);
            }

            Donut(tr, ms, layer, bendPairX, bendPairY, o);

            double extX = g.BarX(g.LandingSoffitY, g.Waist - cover) - (off * g.Riser / g.StepLength);
            double extY = g.LandingSoffitY + (off * g.Tread / g.StepLength);

            double extDist = Math.Sqrt(
                ((extX - bendPairX) * (extX - bendPairX)) +
                ((extY - bendPairY) * (extY - bendPairY)));
            int extN = Math.Max(2, (int)Math.Ceiling(extDist / p.DonutSpacing) + 1);

            for (int i = 1; i < extN - 1; i++)
            {
                double u = (double)i / (extN - 1);
                Donut(tr, ms, layer,
                    bendPairX + ((extX - bendPairX) * u),
                    bendPairY + ((extY - bendPairY) * u), o);
            }

            Donut(tr, ms, layer, extX, extY, o);
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