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
            FilteredElementCollector tbCollector = new FilteredElementCollector(doc.ActiveView.Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType();

            FilteredElementCollector viewportCollector = new FilteredElementCollector(doc.ActiveView.Document)
                .OfCategory(BuiltInCategory.OST_Viewports)
                .WhereElementIsNotElementType();


            //Variables
            int arbitrarySet = 1000;
           
            XYZ boxOrigin = new XYZ(0.517281824146976, 0.148269356955402, 0);
            XYZ boxHeight = new XYZ(0.517281824146976, 0.627436023622063, 0) - boxOrigin;
            XYZ boxWidth = new XYZ(1.03811515748031, 0.148269356955402, 0) - boxOrigin;

            //Transaction 1 Start
            Transaction t = new Transaction(doc);
            t.Start("Set Sheet to Origin");

                //Reset Sheet Origin
                foreach (Element tb in tbCollector)
                {
                    LocationPoint inverse = tb.Location as LocationPoint;
                    tb.Location.Move(new XYZ(-inverse.Point.X, -inverse.Point.Y, -inverse.Point.Z));
                }
                t.Commit();
                t.Dispose();

            //Transaction 2 Start
            Transaction t2 = new Transaction(doc);
            t2.Start("Reset Detail Numbers");

                //Set Detail Numbers to Arbitrary Number
                foreach (Element vp in viewportCollector)
                {
                    arbitrarySet++;
                    Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                    number.Set(arbitrarySet.ToString());
                }
                arbitrarySet = 1000;
                t2.Commit();
                t2.Dispose();

            //Transaction 2 Start
            Transaction t3 = new Transaction(doc);
            t3.Start("Set Detail Numbers");

                //Variables
                string horNumber = "";
                string vertLetter = "";

                int failedAttempts = 0;

                //Set Detail Numbers to Number Based On Location
                foreach (Viewport vp in viewportCollector)
                {
                    //Find ViewTitleHead Location
                    XYZ max = vp.GetLabelOutline().MaximumPoint;
                    XYZ min = vp.GetLabelOutline().MinimumPoint;
                    XYZ headLocation = new XYZ(min.X, max.Y, 0);                            
                        
                        //Set Number
                        if (headLocation.X / boxWidth.X >= 6)
                            horNumber = "6";
                        else if (headLocation.X / boxWidth.X >= 5.0)
                            horNumber = "5";
                        else if (headLocation.X / boxWidth.X >= 4.0)
                            horNumber = "4";
                        else if (headLocation.X / boxWidth.X >= 3.0)
                            horNumber = "3";
                        else if (headLocation.X / boxWidth.X >= 2.0)
                            horNumber = "2";
                        else if (headLocation.X / boxWidth.X >= 1.0)
                            horNumber = "1";
                        else
                            horNumber = "Null";

                        //Set Letter
                        if (Math.Abs(headLocation.Y) / boxHeight.Y >= 4.0)
                            vertLetter = "E";
                        else if (Math.Abs(headLocation.Y) / boxHeight.Y >= 3.0)
                            vertLetter = "D";
                        else if (Math.Abs(headLocation.Y) / boxHeight.Y >= 2.0)
                            vertLetter = "C";
                        else if (Math.Abs(headLocation.Y) / boxHeight.Y >= 1.0)
                            vertLetter = "B";
                        else if (Math.Abs(headLocation.Y) / boxHeight.Y >= 0)
                            vertLetter = "A";
                        else
                            vertLetter = "Null";

                    //Get Updated Detail Numbers
                    FilteredElementCollector updatedViewportCollector = new FilteredElementCollector(doc.ActiveView.Document)
                    .OfCategory(BuiltInCategory.OST_Viewports)
                    .WhereElementIsNotElementType();

                    //Variables
                    string newName = vertLetter+horNumber;                
                    List<string> newNameList = new List<string>();
                    newNameList.Add(newName);                
                    List<string> names = new List<string>();

                    //Add New Detail Numbers to List
                    foreach (Viewport uvp in updatedViewportCollector)
                    {
                       names.Add(uvp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).AsValueString());
                    }               

                    //If Duplicate Value Then Append Letter
                    if (names.Contains(newName))
                    {
                        failedAttempts++;
                        if (failedAttempts == 1)
                            newName = vertLetter + horNumber + "b";
                        else if (failedAttempts == 2)
                            newName = vertLetter + horNumber + "c";
                        else if (failedAttempts == 3)
                            newName = vertLetter + horNumber + "d";
                        else if (failedAttempts == 4)
                            newName = vertLetter + horNumber + "e";
                        else if (failedAttempts == 5)
                            newName = vertLetter + horNumber + "f";
                        else if (failedAttempts == 6)
                            newName = vertLetter + horNumber + "g";

                        Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                        number.Set(newName);
                    }
                    else
                    {
                        failedAttempts = 0;
                        Parameter number = vp.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                        number.Set(newName);

                    }
                
                }
                failedAttempts = 0;
            
            t3.Commit();
            t3.Dispose();

            return Result.Succeeded;
        }
        
    }

}
