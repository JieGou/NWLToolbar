#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class AlignAllSheetsToOrigin : IExternalCommand
    {
        
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            FilteredElementCollector tbCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType();
            


            Transaction t = new Transaction(doc);
            t.Start("Set Sheet to Origin");

            //Reset Sheet Origin
            foreach (Element tb in tbCollector)
            {
                LocationPoint inverse = tb.Location as LocationPoint;                
                tb.Location.Move(new XYZ(-inverse.Point.X, -inverse.Point.Y, -inverse.Point.Z));
            }
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
