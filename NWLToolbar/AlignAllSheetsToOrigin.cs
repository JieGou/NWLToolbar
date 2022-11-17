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
    public class AlignAllSheetsToOrigin : IExternalCommand
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
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets);
            
            //Define Subcategories to move
            IList<Type> typeList = new List<Type>();
            typeList.Add(typeof(Viewport));
            typeList.Add(typeof(FamilyInstance));
            typeList.Add(typeof(Dimension));
            typeList.Add(typeof(RevisionCloud));              
            typeList.Add(typeof(TextNote));
            ElementMulticlassFilter dependentFilter = new ElementMulticlassFilter(typeList);           

            //Start Transaction
            Transaction t = new Transaction(doc);
            t.Start("Set Sheet to Origin");           

            //Get View Sheets from Collector
            foreach (ViewSheet e in sheetCollector)
            {
                //Variables
                IList<ElementId> elementIds = e.GetDependentElements(dependentFilter);
                IList<Element> dependentElement = new List<Element>();
                XYZ tempOffset = new XYZ();

                //Get Dependent Elements
                foreach (ElementId eId in elementIds)
                {
                    
                    Element tb = doc.GetElement(eId);
                    dependentElement.Add(tb);
                    string tbName = tb.Category.Name;


                    //Set Offset Based On Title Block Positioning
                    if (tbName == "Title Blocks")
                    {
                        LocationPoint inverse = tb.Location as LocationPoint;
                        XYZ offset = new XYZ(-inverse.Point.X, -inverse.Point.Y, -inverse.Point.Z);
                        tempOffset = offset;
                    } 

                }

                //Move All Elements
                foreach (Element moveElements in dependentElement)                
                    moveElements.Location.Move(tempOffset);                

            }
            
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
