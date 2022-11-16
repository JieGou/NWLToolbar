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
            FilteredElementCollector tbCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
                string horNumber = "";
                string vertLetter = "";

                int failedAttempts = 0;

                //Set Detail Numbers to Number Based On Location
                foreach (Viewport vp in viewportCollector)
                {
                    
                    
                    //Find ViewTitleHead Location
                    XYZ max = vp.GetLabelOutline().MaximumPoint;
                    XYZ min = vp.GetLabelOutline().MinimumPoint;
                    XYZ headLocation = new XYZ(min.X, min.Y, 0)-boxOrigin;    
                    double xLocation = (headLocation.X/boxWidth.X);
                    double yLocation = (headLocation.Y/boxHeight.Y);
                        
                        //Set Number
                        if (xLocation >= 5)
                            horNumber = "6";
                        else if (xLocation >= 4.0)
                            horNumber = "5";
                        else if (xLocation >= 3.0)
                            horNumber = "4";
                        else if (xLocation >= 2.0)
                            horNumber = "3";
                        else if (xLocation >= 1.0)
                            horNumber = "2";
                        else if (xLocation >= 0.0)
                            horNumber = "1";
                        else
                            horNumber = "Null";

                        //Set Letter
                        if (yLocation >= 4.0)
                            vertLetter = "E";
                        else if (yLocation >= 3.0)
                            vertLetter = "D";
                        else if (yLocation >= 2.0)
                            vertLetter = "C";
                        else if (yLocation >= 1.0)
                            vertLetter = "B";
                        else if (yLocation >= 0)
                            vertLetter = "A";
                        else
                            vertLetter = "Null";

                    //Get Updated Detail Numbers
                    FilteredElementCollector updatedViewportCollector = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
            
            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
