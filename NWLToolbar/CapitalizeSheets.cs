#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class CapitalizeSheets : IExternalCommand
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

            //1. Variables



            //2. Filtered Collecter 
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc);
            sheetCollector.OfCategory(BuiltInCategory.OST_Sheets);
            sheetCollector.WhereElementIsNotElementType();

            TaskDialog.Show("Test", sheetCollector.GetElementCount().ToString()+ " sheets found");

            
           
            return Result.Succeeded;
        }
    }

}
