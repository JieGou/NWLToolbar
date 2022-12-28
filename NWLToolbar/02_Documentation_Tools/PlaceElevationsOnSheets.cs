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
    public class PlaceElevationsOnSheets : IExternalCommand
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

            //Get all rooms
            List<Room> roomCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(x => x.IsEnclosed())
                .OrderBy(x => x.Number)
                .ToList();

            List<FamilySymbol> tbCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();

            List<View> allViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(x => x.ViewType.ToString() == "Elevation" || x.IsTemplate == false)
                .ToList();

            //Variables
            List<Room> selectedRoomList = new List<Room>();
            ElementId tbId = null;
            double SheetsToCreate;

            //Dialog Box Settings
            FrmPlaceElevationsOnSheets curForm = new FrmPlaceElevationsOnSheets(roomCollector, tbCollector);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            
            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedViews = curForm.GetSelectedRooms();
                
                foreach (FamilySymbol fs in tbCollector)
                {
                    if (fs.FamilyName + ": " + fs.Name == curForm.GetSelectedTitleBlock())
                        tbId = fs.Id;
                }

                Dictionary<string, Room> roomDict = roomCollector.ToDictionary(x => x.GetNumName());

                foreach (string s in selectedViews)
                {
                    if (roomDict.ContainsKey(s))
                        selectedRoomList.Add(roomDict[s]);
                }
            }
            
            SheetsToCreate = Math.Ceiling(selectedRoomList.Count() / 4d);
            
            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Place Elevations On Sheets");

            //Create Viewsheet and place Views
            for (int i = 0; i < SheetsToCreate; i++)
            {
                //Create Sheets
                ViewSheet curSheet = ViewSheet.Create(doc, tbId);
                curSheet.Name = "INTERIOR ELEVATIONS";

                //Variables
                IList<View> curElevations = new List<View>();
                IList<View> subList0 = new List<View>();
                IList<View> subList1 = new List<View>();
                IList<View> subList2 = new List<View>();
                IList<View> subList3 = new List<View>();

                int sPoint = i * 4;
                int curIndex = selectedRoomList.Count - sPoint;

                Room curRoom0 = selectedRoomList[sPoint];
                Room curRoom1 = null;
                Room curRoom2 = null;
                Room curRoom3 = null;

                //Finds if How many rooms will be placed on sheet
                if (selectedRoomList.Count - sPoint > 1)                
                    curRoom1 = selectedRoomList[sPoint + 1];                
                if (selectedRoomList.Count - sPoint > 2)
                    curRoom2 = selectedRoomList[sPoint + 2];                
                if (selectedRoomList.Count - sPoint > 3)                
                    curRoom3 = selectedRoomList[sPoint + 3];                
                
                //Populates Sublists to ensure room elevation order
                foreach (View e in allViews)
                {                    
                    if (e.Name.Contains(curRoom0.GetNumName()))
                    {
                        subList0.Add(e);
                    }
                    if (curIndex > 1)
                    {
                        if (e.Name.Contains(curRoom1.GetNumName()))
                        {
                            subList1.Add(e);
                        }                        
                    }
                    if (curIndex > 2)
                    {
                        if (e.Name.Contains(curRoom2.GetNumName()))
                        {
                            subList2.Add(e);
                        }                        
                    }
                    if (curIndex > 3)
                    {
                        if (e.Name.Contains(curRoom3.GetNumName()))
                        {
                            subList3.Add(e);
                        }                        
                    }
                }

                //Combines lists
                curElevations = subList0.Concat(subList1).Concat(subList2).Concat(subList3).ToList();

                //Tracks Which view has been placed
                int curViewPlaced = 0;                

                //Creates viewport and moves it to its proper location based on list order
                foreach (View v in curElevations)
                {                    
                    XYZ curPoint = new XYZ();
                    Viewport curViewport = Viewport.Create(doc, curSheet.Id, curElevations[curViewPlaced].Id, curPoint);

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

            return Result.Succeeded;
        }

        //Calculates lower left placement point per view
        private XYZ GetStartingPoint(int curViewPlaced)
        {
            XYZ boxBottomLeft = new XYZ(0.146874999999995, 0.0531250000000337, 0);
            XYZ boxTopRight = new XYZ(2.75104166666667, 2.44895833333333, 0);
            XYZ boxSize = boxTopRight - boxBottomLeft;
            double sizeX = boxSize.X / 4;
            double sizeY = boxSize.Y / 4;
            double remainder = curViewPlaced % 4;
            double whole = Math.Floor(Convert.ToDouble(curViewPlaced)/4);

            return new XYZ(boxBottomLeft.X + sizeX * remainder, boxBottomLeft.Y + sizeY * whole, 0);            
        }
    }
}