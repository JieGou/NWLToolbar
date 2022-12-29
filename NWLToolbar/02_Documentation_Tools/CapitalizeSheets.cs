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

            // Filtered Collecter 
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType();
            
            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Capitalize Sheets");

            //Search For Sheets & Capitalize
            foreach (ViewSheet i in sheetCollector)
            {               
               i.Name = i.Name.ToUpper();                
            }

            //Finish Transaction
            t.Commit();
            t.Dispose();

            //Success Dialog Box
            TaskDialog.Show("Success", "All Sheets Capitalized");

            return Result.Succeeded;
        }       
    }
}
