using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace StairCase
{
    public static class Reinforcement
    {
        // Bars are drawn one at a time. B1, B2 done, B3..B5 follow.
        public static void Draw(Transaction tr, BlockTableRecord modelSpace, StairParams p, StairGeometry g, ObjectId layer)
        {
            DrawB1(tr, modelSpace, p, g, layer);
            DrawB2(tr, modelSpace, p, g, layer);
        }

        // B1: lower main bar. Diagonal on the cover-25 line from the 0.3
        // step-count corner down into the slab, then a U in the thickening
        // whose right leg meets the bottom-zone bar (cover 25 above soffit).
        // Corners use the Test2 pattern: short two-point jog + fixed bulge.
        static void DrawB1(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer)
        {
            double cover = StairConstants.BarDrop; // 25
            double arc = Math.Tan(22.5 * Math.PI / 180.0); // Test2 corner bulge (90 degree turn)
            double r = StairConstants.FourthBarCornerRadius; // 15

            // Top end: perpendicular from the 0.3 step-count corner, cover below it.
            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double topX = g.FlightStart + (kLow * g.Tread);
            double topCornerY = kLow * g.Riser;
            double topPX = topX + (cover * g.Riser / g.StepLength);
            double topPY = topCornerY - (cover * g.Tread / g.StepLength);

            // Left leg drops at riser face minus 40.
            double legX = g.FlightStart - 40;

            // Foot corner pair: tangent points each side of the foot, bulge
            // from the same formula with this corner's own meeting angle.
            double footY = g.BarY(legX, cover);
            double inX = -g.Tread / g.StepLength;
            double inY = -g.Riser / g.StepLength;
            double footTurn = Math.Atan2((inX * -1.0) - (inY * 0.0), (inX * 0.0) + (inY * -1.0));
            double footT = r * Math.Tan(Math.Abs(footTurn) / 2.0);

            // Bottom bar 25 above thickening bottom.
            double yBot = g.ThickeningBottomY + cover;

            // Right leg: cover left of thickening flat end, top past the
            // crossing with the bottom-zone bar by a short stub.
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

            PolylineBends(tr, ms, layer, pts, bulges);
        }

        // B2: upper main bar. Diagonal on the cover line (same as B1) from
        // the 0.3 step-count point below the top, up to a bend, then
        // horizontal at soffit+25 to near the beam end with a short up-hook.
        static void DrawB2(Transaction tr, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer)
        {
            double cover = StairConstants.BarDrop; // 25
            double arc = Math.Tan(22.5 * Math.PI / 180.0); // Test2 corner bulge (90 degree turn)
            double r = StairConstants.FourthBarCornerRadius; // 15

            // Bottom end: perpendicular from its step corner, cover below it.
            // The diagonal rides the same cover line as B1 up to the bend.
            int kLow = Math.Max(1, (int)Math.Round(0.3 * g.NumTreads));
            double lowX = g.FlightStart + ((g.NumTreads - kLow) * g.Tread);
            double off = cover;
            double lowCornerY = (g.NumTreads - kLow) * g.Riser;
            double lowPX = lowX + (cover * g.Riser / g.StepLength);
            double lowPY = lowCornerY - (cover * g.Tread / g.StepLength);

            // Bend where the bottom-zone line meets the landing bottom bar level.
            double yBar = g.LandingSoffitY + cover;
            double bendX = g.BarX(yBar, off);

            // Bend corner pair: tangent points each side, bulge from this
            // corner's own meeting angle (same formula as Test2's).
            double upX = g.Tread / g.StepLength;
            double upY = g.Riser / g.StepLength;
            double bendTurn = Math.Atan2((upX * 0.0) - (upY * 1.0), (upX * 1.0) + (upY * 0.0));
            double bendT = r * Math.Tan(Math.Abs(bendTurn) / 2.0);

            // End hook: cover from the right face, short stub up.
            double endX = g.LandingEndX - cover;

            var pts = new List<Point2d>
            {
                new Point2d(lowPX, lowPY),
                new Point2d(bendX - (upX * bendT), yBar - (upY * bendT)),
                new Point2d(bendX + bendT, yBar),
                new Point2d(endX - r, yBar),
                new Point2d(endX, yBar + r),
                new Point2d(endX, yBar + (3 * cover))
            };

            var bulges = new List<double> { 0, Math.Tan(bendTurn / 4.0), 0, arc, 0, 0 };

            PolylineBends(tr, ms, layer, pts, bulges);
        }

        static void DrawDiagonalDonuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double endY = g.LandingTopY - StairConstants.BarDrop;
            double endX = g.BarX(endY, StairConstants.UpperBarDrop);
            double startX = endX - StairConstants.UpperBarRun;

            double stopX = g.BarX(g.LandingSoffitY, StairConstants.UpperBarDrop);

            double stepX = p.DonutSpacing * g.Tread / g.StepLength;

            double normalX = -g.Riser / g.StepLength;
            double normalY = g.Tread / g.StepLength;

            double drop = StairConstants.DonutOffset * g.StepLength / g.Tread;


            double lowEnd = Step(g, 3).X;
            double lowStart = lowEnd - (StairConstants.LowerBarRun * g.Tread / g.StepLength);

            double upStart = g.FlightStart
                + (StairConstants.FourthBarStep * g.Tread)
                + (StairConstants.FourthBarDrop * g.Riser / g.StepLength);

            double upEnd = g.BarX(g.LandingTopY - StairConstants.FourthBarBelowTop, StairConstants.FourthBarDrop);

            Point3d lowA = Step(g, 3);
            Point3d lowB = new Point3d(
                lowA.X - (StairConstants.LowerBarRun * g.Tread / g.StepLength),
                lowA.Y - (StairConstants.LowerBarRun * g.Riser / g.StepLength),
                0);

            double upperCornerX = g.FlightStart + (StairConstants.FourthBarStep * g.Tread);

            Point3d upA = new Point3d(
                upperCornerX + (StairConstants.FourthBarDrop * g.Riser / g.StepLength),
                g.BarY(upperCornerX, StairConstants.FourthBarDrop),
                0);

            Point3d upB = new Point3d(upEnd, g.LandingTopY - StairConstants.FourthBarBelowTop, 0);

            for (double x = startX; x < stopX; x += stepX)
            {
                var bar = new Point3d(x, g.BarY(x, StairConstants.UpperBarDrop) + drop, 0);

                Donut(tr, ms, layer, bar.X, bar.Y);

                Point3d a;
                Point3d b;

                if (x >= upStart && x <= upEnd)
                {
                    a = upA;
                    b = upB;
                }
                else if (x >= lowStart && x <= lowEnd)
                {
                    a = lowA;
                    b = lowB;
                }
                else
                {
                    continue;
                }

                double gap = Gap(a, b, bar) - StairConstants.DonutOffset;

                Donut(tr, ms, layer,
                    bar.X + (normalX * gap),
                    bar.Y + (normalY * gap));
            }
        }

        static double Gap(Point3d a, Point3d b, Point3d m)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double len = Math.Sqrt((dx * dx) + (dy * dy));

            return Math.Abs(((m.X - a.X) * dy) - ((m.Y - a.Y) * dx)) / len;
        }

        static void DrawLandingDonuts(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double topY = g.LandingTopY - StairConstants.FifthBarThirdBelowTop;
            double bottomY = g.LandingSoffitY + StairConstants.BarDrop;

            double startX = g.FlightEndX + StairConstants.FifthBarThirdFromStartX;
            double endX = FourthBarThirdX(g);

            double length = endX - startX;

            double secondY = topY - StairConstants.MiddleBarTurnDown;
            double secondX = g.BarX(secondY, StairConstants.MiddleBarDrop);
            double middleFirstX = secondX - StairConstants.MiddleBarStartBack;
            double middleFirstY = g.BarY(middleFirstX, StairConstants.MiddleBarDrop);

            double ux = secondX - middleFirstX;
            double uy = secondY - middleFirstY;
            double ul = Math.Sqrt((ux * ux) + (uy * uy));

            ux = ux / ul;
            uy = uy / ul;

            double donutY = bottomY + StairConstants.DonutOffset;

            double crossX = middleFirstX + (((donutY - middleFirstY) * ux) / uy);

            double first = startX;
            double push = StairConstants.DonutOffset / uy;

            if (first < crossX + push)
            {
                first = crossX + push;
            }

            for (double x = startX + p.DonutSpacing; x < endX; x += p.DonutSpacing)
            {
                Donut(tr, ms, layer, x, topY - StairConstants.DonutOffset);
                Donut(tr, ms, layer, x, donutY);
            }

            Donut(tr, ms, layer, first, topY - StairConstants.DonutOffset);
            Donut(tr, ms, layer, first, donutY);
        }

        static void DrawLowerBar(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            var points = new List<Point3d>();

            points.Add(Step(g, 3));

            double length = StairConstants.LowerBarRun;
            double across = length * StairConstants.Tread / g.StepLength;
            double down = length * StairConstants.Riser / g.StepLength;

            points.Add(new Point3d(points[0].X - across, points[0].Y - down, 0));

            points.Add(new Point3d(1592, -69, 0));
            points.Add(new Point3d(1592, -155, 0));
            points.Add(new Point3d(1612, -175, 0));
            points.Add(new Point3d(2137, -175, 0));
            points.Add(new Point3d(2147, -165, 0));
            points.Add(new Point3d(2147, 125, 0));

            Polyline(tr, ms, layer, points);

            DonutRuns(tr, ms, layer, p, points, 1);
        }

        static void DonutRuns(
            Transaction tr,
            BlockTableRecord ms,
            ObjectId layer,
            StairParams p,
            List<Point3d> points,
            int firstChainRun)
        {
            var bounds = new List<int>();

            bounds.Add(0);

            for (int i = 1; i < points.Count - 1; i++)
            {
                if (Turn(points[i - 1], points[i], points[i + 1]))
                {
                    bounds.Add(i);
                }
            }

            bounds.Add(points.Count - 1);

            int runs = bounds.Count - 1;

            var normalX = new double[runs];
            var normalY = new double[runs];
            var length = new double[runs];

            for (int r = 0; r < runs; r++)
            {
                double dx = points[bounds[r + 1]].X - points[bounds[r]].X;
                double dy = points[bounds[r + 1]].Y - points[bounds[r]].Y;

                length[r] = Math.Sqrt((dx * dx) + (dy * dy));

                normalX[r] = -dy / length[r];
                normalY[r] = dx / length[r];
            }

            double shortest = (2 * StairConstants.DonutOffset) + StairConstants.DonutLength;

            for (int r = 0; r < runs - 1; r++)
            {
                if (length[r] < shortest)
                {
                    continue;
                }

                double bx = normalX[r] + normalX[r + 1];
                double by = normalY[r] + normalY[r + 1];
                double bl = Math.Sqrt((bx * bx) + (by * by));

                int c = bounds[r + 1];

                Donut(tr, ms, layer,
                    points[c].X + ((bx / bl) * StairConstants.DonutOffset),
                    points[c].Y + ((by / bl) * StairConstants.DonutOffset));
            }

            for (int r = 0; r < runs; r++)
            {
                if (length[r] < shortest || r < firstChainRun)
                {
                    continue;
                }

                bool fromEnd = r < runs - 1;

                for (int k = 1; k * p.DonutSpacing < length[r]; k++)
                {
                    double d = k * p.DonutSpacing;
                    double t = fromEnd ? length[r] - d : d;

                    int a = bounds[r];

                    Donut(tr, ms, layer,
                        points[a].X + (((points[bounds[r + 1]].X - points[a].X) * t / length[r]) + (normalX[r] * StairConstants.DonutOffset)),
                        points[a].Y + (((points[bounds[r + 1]].Y - points[a].Y) * t / length[r]) + (normalY[r] * StairConstants.DonutOffset)));
                }
            }
        }

        static bool Turn(Point3d a, Point3d b, Point3d c)
        {
            double abx = b.X - a.X;
            double aby = b.Y - a.Y;
            double bcx = c.X - b.X;
            double bcy = c.Y - b.Y;

            double ab = Math.Sqrt((abx * abx) + (aby * aby));
            double bc = Math.Sqrt((bcx * bcx) + (bcy * bcy));

            if (ab < 1e-9 || bc < 1e-9)
            {
                return false;
            }

            double dot = (((abx / ab) * (bcx / bc)) + ((aby / ab) * (bcy / bc)));

            return dot < StairConstants.DonutTurnCos;
        }

        static void Donut(Transaction tr, BlockTableRecord ms, ObjectId layer, double x, double y)
        {
            double half = StairConstants.DonutLength / 2;

            var donut = new Polyline();

            donut.AddVertexAt(0, new Point2d(x - half, y), -1.0,
                StairConstants.DonutWidth, StairConstants.DonutWidth);

            donut.AddVertexAt(1, new Point2d(x + half, y), -1.0,
                StairConstants.DonutWidth, StairConstants.DonutWidth);

            donut.Closed = true;
            donut.LayerId = layer;
            donut.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.RebarColor);

            ms.AppendEntity(donut);
            tr.AddNewlyCreatedDBObject(donut, true);
        }

        static double FourthBarThirdX(StairGeometry g)
        {
            double secondY = g.LandingTopY - StairConstants.FourthBarBelowTop;

            return g.BarX(secondY, StairConstants.FourthBarDrop) + StairConstants.FourthBarRun;
        }

        static void DrawFifthBar(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double startX = g.FlightEndX + StairConstants.FifthBarFromStartX;
            double thirdX = g.FlightEndX + StairConstants.FifthBarThirdFromStartX;
            double thirdY = g.LandingTopY - StairConstants.FifthBarThirdBelowTop;

            double fourthX = FourthBarThirdX(g);
            double fifthX = fourthX + StairConstants.FifthBarCornerRadius;
            double fifthY = g.LandingTopY - StairConstants.FifthBarFifthBelowTop;

            var points = new List<Point3d>
            {
                new Point3d(startX, g.LandingTopY - StairConstants.FifthBarFirstBelowTop, 0),
                new Point3d(startX, g.LandingTopY - StairConstants.FifthBarSecondBelowTop, 0),
                new Point3d(thirdX, thirdY, 0),
                new Point3d(fourthX, thirdY, 0),
                new Point3d(fifthX, fifthY, 0),
                new Point3d(fifthX, fifthY - StairConstants.FifthBarLastDrop, 0)
            };

            Polyline(tr, ms, layer, points);
        }

        static void DrawFourthBar(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double drop = StairConstants.FourthBarDrop;

            int step = StairConstants.FourthBarStep;

            double cornerX = g.FlightStart + (step * g.Tread);

            double secondY = g.LandingTopY - StairConstants.FourthBarBelowTop;
            double secondX = g.BarX(secondY, drop);

            double thirdY = g.LandingSoffitY + StairConstants.BarDrop;
            double thirdX = secondX + StairConstants.FourthBarRun;

            double radius = StairConstants.FourthBarCornerRadius;

            double fourthX = thirdX + radius;
            double fourthY = thirdY + radius;

            var points = new List<Point3d>
            {
                new Point3d(
                    cornerX + (drop * g.Riser / g.StepLength),
                    g.BarY(cornerX, drop),
                    0),
                new Point3d(secondX, secondY, 0),
                new Point3d(thirdX, thirdY, 0),
                new Point3d(fourthX, fourthY, 0),
                new Point3d(fourthX, fourthY + StairConstants.FourthBarRise, 0)
            };

            Polyline(tr, ms, layer, points);
        }

        static void DrawMiddleBar(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double drop = StairConstants.MiddleBarDrop;

            double thirdY = g.LandingTopY - StairConstants.BarDrop;
            double secondY = thirdY - StairConstants.MiddleBarTurnDown;
            double secondX = g.BarX(secondY, drop);
            double firstX = secondX - StairConstants.MiddleBarStartBack;

            double thirdX = secondX + StairConstants.MiddleBarAcross;

            var points = new List<Point3d>
            {
                new Point3d(firstX, g.BarY(firstX, drop), 0),
                new Point3d(secondX, secondY, 0),
                new Point3d(thirdX, thirdY, 0),
                new Point3d(thirdX + StairConstants.MiddleBarRunAlong, thirdY, 0),
                new Point3d(
                    thirdX + StairConstants.MiddleBarRunAlong + StairConstants.MiddleBarStepAcross,
                    thirdY - StairConstants.MiddleBarStepDown,
                    0)
            };

            Polyline(tr, ms, layer, points);
        }

        static void DrawUpperBar(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            double endY = g.LandingTopY - StairConstants.BarDrop;
            double endX = g.BarX(endY, StairConstants.UpperBarDrop);
            double startX = endX - StairConstants.UpperBarRun;

            double thirdX = endX + StairConstants.UpperBarAcross;

            var points = new List<Point3d>
            {
                new Point3d(startX, g.BarY(startX, StairConstants.UpperBarDrop), 0),
                new Point3d(endX, g.BarY(endX, StairConstants.UpperBarDrop), 0),
                new Point3d(thirdX, g.BarY(endX, StairConstants.UpperBarDrop), 0),
                new Point3d(
                    thirdX + StairConstants.UpperBarTurnAcross,
                    g.BarY(endX, StairConstants.UpperBarDrop) - StairConstants.UpperBarTurnDown,
                    0)
            };

            Polyline(tr, ms, layer, points);
        }

        static Point3d Step(StairGeometry g, int k)
        {
            double cornerX = g.FlightStart + (k * g.Tread);
            double cornerY = k * g.Riser;

            return new Point3d(
                cornerX + (StairConstants.BarDrop * g.Riser / g.StepLength),
                g.BarY(cornerX, StairConstants.BarDrop),
                0);
        }

        static void PolylineBends(Transaction tr, BlockTableRecord ms, ObjectId layer, List<Point2d> pts, List<double> bulges)
        {
            var polyline = new Polyline();

            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, pts[i], bulges[i], 0, 0);
            }

            polyline.LayerId = layer;
            polyline.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.RebarColor);

            ms.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }

        static void Polyline(Transaction tr, BlockTableRecord ms, ObjectId layer, List<Point3d> points)
        {
            var polyline = new Polyline();

            foreach (Point3d point in points)
            {
                polyline.AddVertexAt(polyline.NumberOfVertices, new Point2d(point.X, point.Y), 0, 0, 0);
            }

            polyline.LayerId = layer;
            polyline.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.RebarColor);

            ms.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }
    }
}