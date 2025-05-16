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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NWLToolbar.Utils;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceViewsOnSheets : IExternalCommand
    {
        List<View> allViews;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<FamilySymbol> tbCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();

            try
            {
                allViews = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                    .OfCategory(BuiltInCategory.OST_Views)
                    .WhereElementIsNotElementType()
                    .Cast<View>()
                    .ToList();
            }
            catch
            {
                TaskDialog.Show("Error", "Please select Views first before runing tool.");
                goto failed;
            }            

            //Variables            
            ElementId tbId = null;
            double SheetsToCreate = 0;

            //Dialog Box Settings
            FrmSelectTitleBlock curForm = new FrmSelectTitleBlock(tbCollector)
            {
                Width = 700,
                Height = 200,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbId = curForm.SelectedFamily().Id;
            }
            
            SheetsToCreate = Math.Ceiling(allViews.Count / 25d);
            
            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Place Views On Sheets");

            //Create Viewsheet and place Views
            for (int i = 0; i < SheetsToCreate; i++)
            {
                //Create Sheets
                ViewSheet curSheet = ViewSheet.Create(doc, tbId);
                curSheet.Name = "New Sheet - Rename Me";

                List<View> curViews = new List<View>();

                int sPoint = i * 25;
                int ePoint = sPoint + 25;
                int indexNum = Math.Abs(allViews.Count - sPoint);
                int indexPoint = ePoint;
                if (indexNum < 25)
                    indexPoint = indexNum + sPoint;

                for (int p = sPoint; p < indexPoint; p++)
                {                    
                    curViews.Add(allViews[p]);                                   
                }
                    

                //Tracks Which view has been placed
                int curViewPlaced = 0;                

                //Creates viewport and moves it to its proper location based on list order
                foreach (View v in curViews)
                {                    
                    XYZ curPoint = new XYZ();
                    Viewport curViewport = Viewport.Create(doc, curSheet.Id, curViews[curViewPlaced].Id, curPoint);

                    BoundingBoxXYZ curViewportbb = curViewport.get_BoundingBox(curSheet);
                    XYZ min = curViewportbb.Min;
                    XYZ center = curViewport.GetBoxCenter();
                    XYZ newStart = GetStartingPoint(curViewPlaced);
                    XYZ Offset = new XYZ(.04, .055, 0);
                    XYZ newCenter = center + (-min) + Offset + newStart;
                    curViewport.SetBoxCenter(newCenter);
                    curViewPlaced++;
                }
            }            

            t.Commit();
            t.Dispose();

            failed:

            return Result.Succeeded;
        }

        //Calculates lower left placement point per view
        private XYZ GetStartingPoint(int curViewPlaced)
        {
            XYZ boxBottomLeft = new XYZ(0.146874999999995, 0.0531250000000337, 0);
            XYZ boxTopRight = new XYZ(2.75104166666667, 2.44895833333333, 0);
            XYZ boxSize = boxTopRight - boxBottomLeft;
            double sizeX = boxSize.X / 5;
            double sizeY = boxSize.Y / 5;
            double remainder = curViewPlaced % 5;
            double whole = Math.Floor(Convert.ToDouble(curViewPlaced)/5);

            return new XYZ(boxBottomLeft.X + sizeX * remainder, boxBottomLeft.Y + sizeY * whole, 0);            
        }
    }
}