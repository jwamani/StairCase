using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace StairCase
{
    public static class LayerManager
    {
        public static ObjectId GetOrCreate(Database database, Transaction tr, string name, short colorIndex)
        {
            LayerTable table = (LayerTable)tr.GetObject(database.LayerTableId, OpenMode.ForRead);

            Color color = Color.FromColorIndex(ColorMethod.ByColor, colorIndex);

            if (table.Has(name))
            {
                var existing = (LayerTableRecord)tr.GetObject(table[name], OpenMode.ForWrite);

                existing.Color = color;
                existing.LineWeight = LineWeight.ByLayer;

                return existing.ObjectId;
            }

            var layer = new LayerTableRecord
            {
                Name = name,
                Color = color,
                LineWeight = LineWeight.ByLayer
            };

            table.UpgradeOpen();

            ObjectId id = table.Add(layer);
            tr.AddNewlyCreatedDBObject(layer, true);

            return id;
        }
    }
}