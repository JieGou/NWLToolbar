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
    public class AlignSheetToOrigin : IExternalCommand
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

            //Get ViewSheets
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)                           
                .WhereElementIsNotElementType();

            //Start Transaction
            Transaction t = new Transaction(doc);
            t.Start("Set Sheet Origin"); 
            
            IList<Element> dependentElement = new List<Element>();                
            XYZ tempOffset = new XYZ();

            //Get Dependent Elements
            foreach (Element e in sheetCollector)
            { 

                  string tbName = e.Category.Name;                    

                  //Set Offset Based On Title Block Positioning
                  if (tbName == "Title Blocks")
                  {
                      LocationPoint inverse = e.Location as LocationPoint;
                      XYZ offset = new XYZ(-inverse.Point.X, -inverse.Point.Y, -inverse.Point.Z);
                      tempOffset = offset;
                      dependentElement.Add(e);
                  } 
                  else if (tbName != "Schedule Graphics")
                       dependentElement.Add(e);

             }

            //Move All Elements
            foreach (Element moveElements in dependentElement)                
                  moveElements.Location.Move(tempOffset);
            
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
