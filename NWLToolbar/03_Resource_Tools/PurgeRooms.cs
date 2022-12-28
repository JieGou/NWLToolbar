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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NWLToolbar.Utils;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeRooms : IExternalCommand
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
                .OfClass(typeof(SpatialElement))
                .OfType<Room>()
                .Where(x => !x.IsEnclosed())
                .ToList();
            
            //Variables
            int count = allRooms.Count;

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Purge Rooms");

            foreach (Room r in allRooms)
            {
                doc.Delete(r.Id);
            }           

            t.Commit();
            t.Dispose();

            if (count > 1)
                TaskDialog.Show("Deleted", $"{count} Rooms were deleted");
            else if (count == 1)
                TaskDialog.Show("Deleted", $"{count} Room was deleted");
            else
                TaskDialog.Show("Deleted", "No Rooms were deleted");

            return Result.Succeeded;
        }
       
    }

}
