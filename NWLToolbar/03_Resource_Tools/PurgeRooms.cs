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
            FilteredElementCollector allRooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms);
                

            //Variables
            Transaction t = new Transaction(doc);
            t.Start("Purge Rooms");

            foreach (Room r in allRooms)
            {
                if (r.Location == null)
                    doc.Delete(r.Id);
            }

            t.Commit();
            t.Dispose();            

            return Result.Succeeded;
        }
       
    }

}
