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
            IList<Element> sheetCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WhereElementIsNotElementType().Where(x => x.Name != "<Revision Schedule>").ToList();

            //Start Transaction
            Transaction t = new Transaction(doc);
            t.Start("Set Sheet Origin");
            
            //Find offset
            XYZ offset = new XYZ();

            //Get Dependent Elements
            foreach (Element e in sheetCollector)
            {                   
                //Set Offset Based On Title Block Positioning
                if (e.Category.Name == "Title Blocks")
                {
                    LocationPoint tbOrigin = e.Location as LocationPoint; 
                    offset = new XYZ(-tbOrigin.Point.X, -tbOrigin.Point.Y, 0);                    
                    break;
                } 
            }

            //Move All Elements
            foreach (Element e in sheetCollector)
            {
                string sdfas = e.Category.Name;
                LocationPoint newLocation = e.Location as LocationPoint;
                if (newLocation != null)
                {
                    if (e.Category.Name == "Title Blocks" )
                        newLocation.Point = new XYZ();
                    
                }
                else if (e.Category.Name == "Revision Cloud Tags")
                {
                    (e as IndependentTag).TagHeadPosition.Add(offset);
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
