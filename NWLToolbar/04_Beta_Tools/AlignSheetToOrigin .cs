#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using System.Xml;
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
            List<Element> sheetCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WhereElementIsNotElementType()
                .Where(x => x.Category.Name != "Title Blocks" && x.Name != "<Revision Schedule>")
                .ToList();

            Element curTB = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .First();  

            //Start Transaction
            Transaction t = new Transaction(doc);
            t.Start("Set Sheet Origin");
            
            //Find offset
            XYZ offset = new XYZ();

            LocationPoint tbOrigin = curTB.Location as LocationPoint;
            offset = new XYZ(-tbOrigin.Point.X, -tbOrigin.Point.Y, 0);

            (curTB.Location as LocationPoint).Point = new XYZ();

            //Move All Elements
            foreach (Element e in sheetCollector)
            {               
                LocationPoint newLocation = e.Location as LocationPoint;
                
                if (e.Category.Name == "Revision Cloud Tags")
                {
                    IndependentTag curTag = e as IndependentTag;
                    
                    curTag.TagHeadPosition.Add(offset);
                }
                else
                {
                    e.Location.Move(offset);
                }
            }                  
            
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }        
    }
}
