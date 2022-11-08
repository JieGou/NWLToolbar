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
    public class SelectOverriddenDimensions : IExternalCommand
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

            // Filtered Element Collecter 
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Dimensions)
                .WhereElementIsNotElementType();


            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Select Overriden Dimensions");

            //List Of Overridden Dimensions
            IList<ElementId> selection = new List<ElementId>();

            //Searching for Overriden Dimensions
            foreach (Element i in sheetCollector)
            {
               if (i is Dimension)
                {
                    Dimension dimension = i as Dimension;
                    string dimensionTextValue = dimension.ValueOverride;//Get Text Value
                    ElementId elementId = i.Id;

                    if (dimensionTextValue == "")
                    { 

                    }
                    else if (dimensionTextValue != null)
                        selection.Add(elementId);

                }
                
            }
            //Select List Of Overriden Dimensions
            uidoc.Selection.SetElementIds(selection);

            //Finish Transaction
            t.Commit();
            t.Dispose();



            //Success Dialog Box
            int count = selection.Count();

            if (count == 1)
                TaskDialog.Show("Success", count.ToString() + " Overriden Dimension Found In This View");
            else if (count > 1)
                TaskDialog.Show("Success", count.ToString() + " Overriden Dimensions Found In This View");
            else
                TaskDialog.Show("Success", "No Overriden Dimensions Found In This View");


            return Result.Succeeded;
        }

       
    }

}
