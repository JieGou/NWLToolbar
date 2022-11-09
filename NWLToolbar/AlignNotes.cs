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
    public class AlignNotes : IExternalCommand
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

            FilteredElementCollector textNotes = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .WhereElementIsNotElementType();


            
            FilteredElementCollector lines = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfClass(typeof(CurveElement))
                .OfCategory(BuiltInCategory.OST_Lines)
                .WhereElementIsNotElementType();
            
                        
            
            
            
            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Align Notes");

            XYZ lineX = new XYZ(0, 0, 0);

            foreach (DetailLine l in lines)
            {

                Line line = l.GeometryCurve as Line;
                                
                lineX = new XYZ(line.Origin.X, 0, 0);

                break;

            }
            TaskDialog.Show("title", "new Point is " + lineX.ToString());


            foreach (TextNote e in textNotes)
            {

                XYZ lp = e.Coord as XYZ;
                
                
                ElementId id = e.Id;

                XYZ newLocation = new XYZ(lineX.X, lp.Y, lp.Z);
                

                ElementTransformUtils.MoveElement(doc, id, newLocation);                           
                               

            }
            

            //Finish Transaction
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
