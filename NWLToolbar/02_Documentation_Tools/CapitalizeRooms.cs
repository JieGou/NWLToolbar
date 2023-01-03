#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NWLToolbar.Utils;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class CapitalizeRooms : IExternalCommand
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

            //Filtered Eelement Collector (Collect Active Selection)            
            List<Room> allRooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(x => x.IsEnclosed())
                .ToList();

            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Capitalize Rooms");

            //Search For Sheets & Capitalize
            foreach (Room r in allRooms)
            {                
                r.Name = r.GetNameParam().AsValueString().ToUpper();                
            }

            //Finish Transaction
            t.Commit();
            t.Dispose();

            //Success Dialog Box
            TaskDialog.Show("Success", "All Rooms Capitalized");

            return Result.Succeeded;
        }        
    }
}
