using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace StairCase
{
    public static class Dimensions
    {
        const double Half = Math.PI / 2;

        public static void Draw(Transaction tr, Database database, BlockTableRecord ms, StairParams p, StairGeometry g, ObjectId layer, Point2d ins)
        {
            ObjectId style = Style(database, tr);

            double o = StairConstants.DimOffset;
            double h = o / 2;

            Dim(tr, ms, layer, style, 0.0,
                g.FlightEndX, g.LandingTopY, g.LandingEndX, g.LandingTopY,
                0, g.LandingTopY + o, ins);

            Dim(tr, ms, layer, style, Half,
                g.LandingEndX, g.LandingSoffitY, g.LandingEndX, g.LandingTopY,
                g.LandingEndX + h, 0, ins);

            Dim(tr, ms, layer, style, Half,
                g.LandingEndX, g.BeamBottomY, g.LandingEndX, g.LandingTopY,
                g.LandingEndX + o, 0, ins);

            Dim(tr, ms, layer, style, 0.0,
                g.BeamStartX, g.BeamBottomY, g.LandingEndX, g.BeamBottomY,
                0, g.BeamBottomY - h, ins);

            Dim(tr, ms, layer, style, Half,
                0, -p.LowerThickness, 0, 0,
                -o, 0, ins);

            Dim(tr, ms, layer, style, 0.0,
                g.BottomStartX, -p.LowerThickness, g.ThickeningFlatStartX, g.ThickeningBottomY,
                0, g.ThickeningBottomY - h, ins);

            Dim(tr, ms, layer, style, 0.0,
                g.ThickeningFlatStartX, g.ThickeningBottomY, g.ThickeningFlatEndX, g.ThickeningBottomY,
                0, g.ThickeningBottomY - h, ins);

            Dim(tr, ms, layer, style, 0.0,
                g.ThickeningFlatEndX, g.ThickeningBottomY, g.ThickeningEndX, -p.LowerThickness,
                0, g.ThickeningBottomY - h, ins);

            int k = 5;

            double treadX1 = g.FlightStart + ((k - 1) * p.Tread);
            double treadX2 = g.FlightStart + (k * p.Tread);
            double riserY1 = (k - 1) * p.Riser;
            double riserY2 = k * p.Riser;

            Dim(tr, ms, layer, style, 0.0,
                treadX1, riserY2, treadX2, riserY2,
                0, riserY2 + h, ins);

            Dim(tr, ms, layer, style, Half,
                treadX1, riserY1, treadX1, riserY2,
                treadX1 - h, 0, ins);

            double waistX = g.FlightStart + (5 * p.Tread);
            double cornerY = g.StepCornerY(waistX);

            double nx = g.Riser / g.StepLength;
            double ny = -g.Tread / g.StepLength;

            double m = g.Riser / g.Tread;
            double across = g.SoffitDrop / (m + (1.0 / m));

            double waistSoffitX = waistX + across;
            double waistSoffitY = cornerY - (across / m);

            Aligned(tr, ms, layer, style,
                waistX, cornerY, waistSoffitX, waistSoffitY,
                waistSoffitX + (nx * h), waistSoffitY + (ny * h), ins);
        }

        static void Dim(Transaction tr, BlockTableRecord ms, ObjectId layer, ObjectId style, double rotation,
            double x1, double y1, double x2, double y2, double dx, double dy, Point2d ins)
        {
            var dim = new RotatedDimension
            {
                XLine1Point = new Point3d(x1 + ins.X, y1 + ins.Y, 0),
                XLine2Point = new Point3d(x2 + ins.X, y2 + ins.Y, 0),
                DimLinePoint = new Point3d(dx + ins.X, dy + ins.Y, 0),
                Rotation = rotation
            };

            Append(tr, ms, layer, style, dim);
        }

        static void Aligned(Transaction tr, BlockTableRecord ms, ObjectId layer, ObjectId style,
            double x1, double y1, double x2, double y2, double dx, double dy, Point2d ins)
        {
            var dim = new AlignedDimension
            {
                XLine1Point = new Point3d(x1 + ins.X, y1 + ins.Y, 0),
                XLine2Point = new Point3d(x2 + ins.X, y2 + ins.Y, 0),
                DimLinePoint = new Point3d(dx + ins.X, dy + ins.Y, 0)
            };

            Append(tr, ms, layer, style, dim);
        }

        static void Append(Transaction tr, BlockTableRecord ms, ObjectId layer, ObjectId style, Dimension dim)
        {
            dim.DimensionStyle = style;
            dim.Dimtxt = StairConstants.DimText;
            dim.Dimdec = 0;
            dim.LayerId = layer;
            dim.Color = Color.FromColorIndex(ColorMethod.ByLayer, StairConstants.DimColor);

            ms.AppendEntity(dim);
            tr.AddNewlyCreatedDBObject(dim, true);
        }

        static ObjectId Style(Database database, Transaction tr)
        {
            DimStyleTable table = (DimStyleTable)tr.GetObject(database.DimStyleTableId, OpenMode.ForRead);

            return table["Standard"];
        }
    }
}
