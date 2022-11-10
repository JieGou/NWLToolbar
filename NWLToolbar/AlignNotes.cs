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

            //Grab Text Notes and Lines
            FilteredElementCollector textNotes = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .WhereElementIsNotElementType();

            FilteredElementCollector keyNotes = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfCategory(BuiltInCategory.OST_KeynoteTags)
                .WhereElementIsNotElementType();

            FilteredElementCollector lines = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfClass(typeof(CurveElement))
                .OfCategory(BuiltInCategory.OST_Lines)
                .WhereElementIsNotElementType();

            FilteredElementCollector mLines = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .OfClass(typeof(CurveElement))
                .OfCategory(BuiltInCategory.OST_GenericLines)
                .WhereElementIsNotElementType();

            //Variables
            XYZ lineX = new XYZ(0, 0, 0);

            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Align Notes");

            //Grab Line Origin and Set X Coordinate
            foreach (DetailLine dl in lines)
            {

                Line line = dl.GeometryCurve as Line;

                lineX = new XYZ(line.Origin.X, 0, 0);
                break;

            }
           
            //Grab each text note and set new location
            foreach (TextNote e in textNotes)
            {
                
                XYZ origLocation = e.Coord as XYZ;               
                                
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, 0);                
                
                XYZ offset = origLocation - newLocation;

                ElementTransformUtils.MoveElement(doc, e.Id, -offset);                         

            }
            
            //Grab each text note and set new location
            foreach (IndependentTag e in keyNotes)
            {
                
                XYZ origLocation = e.TagHeadPosition as XYZ;                
                
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, 0);                

                XYZ offset = origLocation - newLocation;                

                ElementTransformUtils.MoveElement(doc, e.Id, -offset);

            }

            //Finish Transaction
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
