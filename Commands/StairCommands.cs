using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace StairCase
{
    public class StairCommands
    {
        [CommandMethod("STAIRCASE")]
        public void StairCase()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            var p = new StairParams();

            using (var form = new ParamsForm(p))
            {
                if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(form)
                    != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
            }

            try
            {
                StairBuilder.Build(doc.Database, p);
            }
            catch (System.Exception exception)
            {
                doc.Editor.WriteMessage(
                    "\nSTAIRCASE failed: " + exception.GetType().FullName + ": " + exception.Message);

                if (exception.InnerException != null)
                {
                    doc.Editor.WriteMessage("\nInner exception: " + exception.InnerException.Message);
                }

                doc.Editor.WriteMessage("\n" + exception.StackTrace);
            }
        }
    }
}