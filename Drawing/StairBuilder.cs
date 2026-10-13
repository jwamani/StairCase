using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace StairCase
{
    public static class StairBuilder
    {
        public static void Build(Database database, StairParams p)
        {
            StairGeometry g = new StairGeometry(p);

            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(
                    blockTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite);

                ObjectId hatchLayer = LayerManager.GetOrCreate(
                    database, tr, StairConstants.HatchLayer, StairConstants.HatchColor);

                ObjectId hatch = CreateHatch(tr, modelSpace, hatchLayer);

                ObjectId layer = LayerManager.GetOrCreate(
                    database, tr, StairConstants.ViewPortLayer, StairConstants.ViewPortColor);

                ObjectId outline = DrawOutline(tr, modelSpace, layer, p, g);

                FillHatch(tr, hatch, outline);

                ObjectId leaderLayer = LayerManager.GetOrCreate(
                    database, tr, StairConstants.LeaderLayer, StairConstants.LeaderColor);

                DrawBreakLine(tr, modelSpace, leaderLayer, g.LowerLandingEndX, 1);
                DrawBreakLine(tr, modelSpace, leaderLayer, 0, -1);

                ObjectId rebarLayer = LayerManager.GetOrCreate(
                    database, tr, StairConstants.RebarLayer, StairConstants.RebarColor);

                Reinforcement.Draw(tr, modelSpace, p, g, rebarLayer);

                LayerTable layers = (LayerTable)tr.GetObject(database.LayerTableId, OpenMode.ForRead);

                Dimensions.Draw(tr, database, modelSpace, p, g, layers["0"]);

                tr.Commit();
            }
        }

        static ObjectId DrawOutline(Transaction tr, BlockTableRecord ms, ObjectId layer, StairParams p, StairGeometry g)
        {
            var points = new List<Point2d>();

            points.Add(new Point2d(0, 0));
            points.Add(new Point2d(g.FlightStart, 0));
            points.Add(new Point2d(g.FlightStart, p.Riser));

            for (int k = 1; k <= StairConstants.Treads; k++)
            {
                points.Add(new Point2d(g.FlightStart + (k * p.Tread), k * p.Riser));
                points.Add(new Point2d(g.FlightStart + (k * p.Tread), (k + 1) * p.Riser));
            }

            points.Add(new Point2d(g.LandingEndX, g.LandingTopY)); // top right
            points.Add(new Point2d(g.LandingEndX, g.BeamBottomY)); // topbottom right
            points.Add(new Point2d(g.BeamStartX, g.BeamBottomY)); // beam topbottom left
            points.Add(new Point2d(g.BeamStartX, g.LandingSoffitY)); // beam topleft

            points.Add(new Point2d(g.SoffitX(g.LandingSoffitY), g.LandingSoffitY)); // soffit top
            points.Add(new Point2d(g.ThickeningTopEndX, g.SoffitY(g.ThickeningTopEndX))); // soffit end
            points.Add(new Point2d(g.ThickeningTopEndX, 0));
            points.Add(new Point2d(g.LowerLandingEndX, 0));
            points.Add(new Point2d(g.LowerLandingEndX, -45));
            points.Add(new Point2d(g.LowerLandingEndX - 35, -60));
            points.Add(new Point2d(g.LowerLandingEndX + 35, -85));
            points.Add(new Point2d(g.LowerLandingEndX, -100));
            points.Add(new Point2d(g.LowerLandingEndX, -p.LowerThickness));
            points.Add(new Point2d(g.ThickeningEndX, -p.LowerThickness));
            points.Add(new Point2d(g.ThickeningFlatEndX, g.ThickeningBottomY));
            points.Add(new Point2d(g.ThickeningFlatStartX, g.ThickeningBottomY));
            points.Add(new Point2d(g.BottomStartX , -p.LowerThickness));
            points.Add(new Point2d(0, -p.LowerThickness));
            points.Add(new Point2d(0, -100));
            points.Add(new Point2d(-35, -85));
            points.Add(new Point2d(35, -60));
            points.Add(new Point2d(0, -45));

            var polyline = new Polyline();

            foreach (Point2d point in points)
            {
                polyline.AddVertexAt(polyline.NumberOfVertices, point, 0, 0, 0);
            }

            polyline.Closed = true;
            polyline.LayerId = layer;
            polyline.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.ViewPortColor);

            ms.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);

            return polyline.ObjectId;
        }

        static void DrawBreakLine(Transaction tr, BlockTableRecord ms, ObjectId layer, double x, double swing)
        {
            double reach = 35 * swing;

            var points = new List<Point2d>
            {
                new Point2d(x, 45),
                new Point2d(x, -45),
                new Point2d(x - reach, -60),
                new Point2d(x + reach, -85),
                new Point2d(x, -100),
                new Point2d(x, -190)
            };

            var polyline = new Polyline();

            foreach (Point2d point in points)
            {
                polyline.AddVertexAt(polyline.NumberOfVertices, point, 0, 0, 0);
            }

            polyline.LayerId = layer;
            polyline.Color = Color.FromColorIndex(ColorMethod.ByColor, StairConstants.LeaderColor);

            ms.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }

        static ObjectId CreateHatch(Transaction tr, BlockTableRecord ms, ObjectId layer)
        {
            Hatch hatch = new Hatch();

            hatch.SetDatabaseDefaults();

            hatch.LayerId = layer;
            hatch.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);

            hatch.SetHatchPattern(HatchPatternType.PreDefined, StairConstants.HatchPattern);
            hatch.Associative = false;

            ms.AppendEntity(hatch);
            tr.AddNewlyCreatedDBObject(hatch, true);

            return hatch.ObjectId;
        }

        static void FillHatch(Transaction tr, ObjectId hatch, ObjectId outline)
        {
            Hatch entity = (Hatch)tr.GetObject(hatch, OpenMode.ForWrite);

            var boundary = new ObjectIdCollection { outline };

            entity.AppendLoop(HatchLoopTypes.Default, boundary);
            entity.EvaluateHatch(true);
        }
    }
}