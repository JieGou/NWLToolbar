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
using NWLToolbar.Utils;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class RenumberViewsOnSheet : IExternalCommand
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

            //Get Title Blocks & Viewports
            FilteredElementCollector tbCollector = new FilteredElementCollector( doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType();

            FilteredElementCollector viewportCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Viewports)
                .WhereElementIsNotElementType();


            //Variables
            int arbitrarySet = 1000;
           
            XYZ boxOrigin = new XYZ(0.146874999999995, 0.0531250000000337, 0);
            XYZ boxHeight = new XYZ(0.146874999999995, 0.5322916666667, 0) - boxOrigin;
            XYZ boxWidth = new XYZ(0.667708333333328, 0.0531250000000337, 0) - boxOrigin;

            //Transaction 1 Start
            Transaction t = new Transaction(doc);
            t.Start("Set Detail Numbers");

            //Reset Sheet Origin
            foreach (Element tb in tbCollector)
            {
                LocationPoint inverse = tb.Location as LocationPoint;
                tb.Location.Move(new XYZ(-inverse.Point.X, -inverse.Point.Y, -inverse.Point.Z));
            }
                                           

            //Set Detail Numbers to Arbitrary Number
            foreach (Element vp in viewportCollector)
            {
                arbitrarySet++;
                Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                number.Set(arbitrarySet.ToString());
            }
            arbitrarySet = 1000;


            //Variables
            string horNumber;
            string vertLetter;

            List<string> rename = new List<string>();

            int failedAttempts = 0;
            int curViewport = 1;
            //Set Detail Numbers to Number Based On Location
            foreach (Viewport vp in viewportCollector)
            {                    
                    
                //Find ViewTitleHead Location
                XYZ max = vp.GetLabelOutline().MaximumPoint;
                XYZ min = vp.GetLabelOutline().MinimumPoint;
                XYZ headLocation = new XYZ(min.X, min.Y, 0)-boxOrigin;    
                double xLocation = (headLocation.X/boxWidth.X);
                double yLocation = (headLocation.Y/boxHeight.Y);

                if (xLocation > 0)
                    horNumber = (Math.Floor(xLocation)+1).ToString();
                else
                    horNumber = $"Null{curViewport}";

                if (yLocation > 0)
                    vertLetter = Char.ConvertFromUtf32(Convert.ToChar("A") + Convert.ToInt32(Math.Floor(yLocation))).ToString();
                else
                    vertLetter = "Null";

                //Get Updated Detail Numbers
                List<Viewport> updatedViewportCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                        .OfCategory(BuiltInCategory.OST_Viewports)
                        .WhereElementIsNotElementType()
                        .Cast<Viewport>()
                        .ToList();

                //Variables
                string newDetailNumber = vertLetter+horNumber;                
                List<string> newNameList = new List<string>();
                newNameList.Add(newDetailNumber);                
                List<string> names = new List<string>();

                Dictionary<string, Viewport> values = updatedViewportCollector.ToDictionary(x => x.GetDetailNumber());                   

                //If Duplicate Value Then Append Letter
                if (values.ContainsKey(newDetailNumber))
                {
                    rename.Add(newDetailNumber);
                    failedAttempts++;
                    if (failedAttempts > 0)
                        newDetailNumber = vertLetter + horNumber + Char.ConvertFromUtf32(Convert.ToChar("b")+(failedAttempts-1)).ToString();                   

                    Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                    number.Set(newDetailNumber);
                }
                else
                {
                    failedAttempts = 0;
                    Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                    number.Set(newDetailNumber);
                }
                
            }

            rename = rename.Distinct().ToList();

            List<Viewport> finalViewportCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                        .OfCategory(BuiltInCategory.OST_Viewports)
                        .WhereElementIsNotElementType()
                        .Cast<Viewport>()
                        .ToList();

            Dictionary<string, Viewport> finalValues = finalViewportCollector.ToDictionary(x => x.GetDetailNumber());

            foreach (Viewport vp in finalViewportCollector)
            {
                foreach (string s in rename)
                {
                    if (s == vp.GetDetailNumber())
                    { 
                        Parameter newNum = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                        newNum.Set(vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).AsValueString() + "a");
                    }
                }
            }

            //failedAttempts = 0;

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
