#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
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

            //Variables
            XYZ lineX = new XYZ(0, 0, 0);
            IList<ElementId> deleteId = new List<ElementId>();
            int numOfLines = 0;

            string orientation = doc.ActiveView.ViewDirection.ToString();
            TaskDialog.Show("Orientation", orientation);

            

            //Get ViewScale Offset
            double viewScale = doc.ActiveView.Scale;
            double calcOffset = 0.0068359375*viewScale;
            XYZ textOffset = new XYZ(calcOffset, 0, 0);
            XYZ textOffsetEast = new XYZ(0, calcOffset, 0);

            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Align Notes");

            //Checking For Detail Lines
            foreach (Element e in lines)
                numOfLines++;

            if (numOfLines == 1)
            { }
            else if (numOfLines == 0)
            {
                TaskDialog.Show("Failed", "No Detail Lines Selected");
                goto Failed;
            }
            else
            {
                TaskDialog.Show("Failed", "Too Many Detail Lines Selected");
                goto Failed;
            }

            //Check view orientation
            if (orientation == "(0.000000000, 0.000000000, 1.000000000)")
                goto PlanOrDetail;
            else if (orientation == "(1.000000000, 0.000000000, 0.000000000)" || orientation == "(-1.000000000, 0.000000000, 0.000000000)")
                goto EastWest;
            else if (orientation == "(0.000000000, 1.000000000, 0.000000000)" || orientation == "(0.000000000, -1.000000000, 0.000000000)")
                goto NorthSouth;
            else
                goto Failed;

            //Grab Line Origin and Set X Coordinate
        PlanOrDetail:
            foreach (DetailLine dl in lines)
            {
                Line line = dl.GeometryCurve as Line;
                lineX = new XYZ(line.Origin.X, 0, 0);
                deleteId.Add(dl.Id);
            }       
        
            doc.Delete(deleteId);

            //Grab each text note and set new location
            foreach (IndependentTag e in keyNotes)
            {
                XYZ origLocation = e.TagHeadPosition as XYZ;
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, origLocation.Z);
                XYZ offset = (origLocation - newLocation);
                ElementTransformUtils.MoveElement(doc, e.Id, -offset);
            }

            //Grab each text note and set new location
            foreach (TextNote e in textNotes)
            {                
                XYZ origLocation = e.Coord as XYZ;                                
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, origLocation.Z);
                string justification = e.HorizontalAlignment.ToString();
                
                if (justification == "Left")
                {
                    XYZ offset = (origLocation - newLocation) - textOffset;
                    ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                }
                else if (justification == "Right")
                {
                    XYZ offset = (origLocation - newLocation) + textOffset;
                    ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                }                
            }
            goto Failed;



        //Grab Line Origin and Set X Coordinate
        EastWest:
            foreach (DetailLine dl in lines)
            {
                Line line = dl.GeometryCurve as Line;
                lineX = new XYZ(0, line.Origin.Y, 0);
                deleteId.Add(dl.Id);
            }

            doc.Delete(deleteId);

            //Grab each text note and set new location
            foreach (IndependentTag e in keyNotes)
            {
                XYZ origLocation = e.TagHeadPosition as XYZ;
                XYZ newLocation = new XYZ(origLocation.X, lineX.Y, origLocation.Z);
                XYZ offset = (origLocation - newLocation);
                ElementTransformUtils.MoveElement(doc, e.Id, -offset);
            }

            //Grab each text note and set new location
            foreach (TextNote e in textNotes)
            {
                XYZ origLocation = e.Coord as XYZ;
                XYZ newLocation = new XYZ(origLocation.X, lineX.Y, origLocation.Z);
                string justification = e.HorizontalAlignment.ToString();

                if (orientation == "(1.000000000, 0.000000000, 0.000000000)")
                {
                    if (justification == "Left")
                    {
                        XYZ offset = (origLocation - newLocation) - textOffsetEast;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                    else if (justification == "Right")
                    {
                        XYZ offset = (origLocation - newLocation) + textOffsetEast;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                }
                else
                {
                    if (justification == "Left")
                    {
                        XYZ offset = (origLocation - newLocation) + textOffsetEast;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                    else if (justification == "Right")
                    {
                        XYZ offset = (origLocation - newLocation) - textOffsetEast;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                }

            }
            goto Failed;

        NorthSouth:
            foreach (DetailLine dl in lines)
            {
                Line line = dl.GeometryCurve as Line;
                lineX = new XYZ(line.Origin.X, 0, 0);
                deleteId.Add(dl.Id);
            }

            doc.Delete(deleteId);

            //Grab each text note and set new location
            foreach (IndependentTag e in keyNotes)
            {
                XYZ origLocation = e.TagHeadPosition as XYZ;
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, origLocation.Z);
                XYZ offset = (origLocation - newLocation);
                ElementTransformUtils.MoveElement(doc, e.Id, -offset);
            }

            //Grab each text note and set new location
            foreach (TextNote e in textNotes)
            {
                XYZ origLocation = e.Coord as XYZ;
                XYZ newLocation = new XYZ(lineX.X, origLocation.Y, origLocation.Z);
                string justification = e.HorizontalAlignment.ToString();

                if (orientation == "(0.000000000, -1.000000000, 0.000000000)")
                {
                    if (justification == "Left")
                    {
                        XYZ offset = (origLocation - newLocation) - textOffset;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                    else if (justification == "Right")
                    {
                        XYZ offset = (origLocation - newLocation) + textOffset;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                }
                else
                {
                    if (justification == "Left")
                    {
                        XYZ offset = (origLocation - newLocation) + textOffset;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                    else if (justification == "Right")
                    {
                        XYZ offset = (origLocation - newLocation) - textOffset;
                        ElementTransformUtils.MoveElement(doc, e.Id, -offset);
                    }
                }

            }
            goto Failed;


        Failed:

            //Finish Transaction
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
