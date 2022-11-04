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
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc);
            sheetCollector.OfCategory(BuiltInCategory.OST_Sheets);
            sheetCollector.WhereElementIsNotElementType();
            
            //Get Sheet Name & Capitalize
            Transaction t = new Transaction(doc);
            t.Start("Capitalize Sheets");

            foreach (Element i in sheetCollector)
            {
               Parameter e = i.get_Parameter(BuiltInParameter.SHEET_NAME);
               
               string v = e.AsValueString();
               
               i.Name = v.ToUpper();  
                

            }

            t.Commit();


            //Success Dialog Box
            TaskDialog.Show("Success", sheetCollector.GetElementCount().ToString() + " Sheets Capitalized");

            return Result.Succeeded;
        }

       
    }

}
