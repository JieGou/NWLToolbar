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
            FilteredElementCollector roomCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            FilteredElementCollector tbCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType();

            List<Room> roomList = new List<Room>();
            List<string> roomListName = new List<string>();
            List<Room> selectedRoomList = new List<Room>();          
            List<FamilySymbol> tbList = new List<FamilySymbol>();

            foreach (Room e in roomCollector)
            {
                bool isPlaced = e.Location != null;
                if (isPlaced == true)
                {
                    roomList.Add(e);
                    roomListName.Add(e.Number + " - " + getRoomName(e));
                }
            }
            foreach(FamilySymbol tb in tbCollector)
            {
                tbList.Add(tb);
            }

            //Dialog Box Settings
            FrmPlaceElevationsOnSheets curForm = new FrmPlaceElevationsOnSheets(roomListName, tbList);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            ElementId tbId = null;
            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedViews = curForm.GetSelectedRooms();
                
                foreach (FamilySymbol fs in tbCollector)
                {
                    if (fs.FamilyName + ": " + fs.Name == curForm.GetSelectedTitleBlock())
                        tbId = fs.Id;
                }
                
                foreach (string s in selectedViews)
                {
                    foreach (Room i in roomList)
                    {
                        if (s == i.Number + " - " + getRoomName(i))
                        {
                            selectedRoomList.Add(i);
                        }
                    }
                }
            }
            
            double SheetsToCreate = Math.Ceiling(selectedRoomList.Count() / 4d);

            //Getting all views
            FilteredElementCollector allViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();

            //All plan views as elements
            IList<View> elevationViews = new List<View>();           

            foreach (View e in allViews)
            {
                string cat = e.ViewType.ToString();
                bool isTemplate = e.IsTemplate;
                if (cat == "Elevation" && isTemplate == false)
                {
                    elevationViews.Add(e);                   
                }
            }
            Transaction t = new Transaction(doc);
            t.Start("Place Elevations On Sheets");

            for (int i = 0; i < SheetsToCreate; i++)
            {
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

                if (selectedRoomList.Count - sPoint > 1)                
                    curRoom1 = selectedRoomList[sPoint + 1];                
                if (selectedRoomList.Count - sPoint > 2)
                    curRoom2 = selectedRoomList[sPoint + 2];                
                if (selectedRoomList.Count - sPoint > 3)                
                    curRoom3 = selectedRoomList[sPoint + 3];                

                foreach (View e in elevationViews)
                {
                    if (e.Name.Contains(curRoom0.Number + " - " + getRoomName(curRoom0)))
                    {
                        subList0.Add(e);
                    }
                    if (curIndex > 1)
                    {
                        if (e.Name.Contains(curRoom1.Number + " - " + getRoomName(curRoom1)))
                        {
                            subList1.Add(e);
                        }                        
                    }
                    if (curIndex > 2)
                    {
                        if (e.Name.Contains(curRoom2.Number + " - " + getRoomName(curRoom2)))
                        {
                            subList2.Add(e);
                        }                        
                    }
                    if (curIndex > 3)
                    {
                        if (e.Name.Contains(curRoom3.Number + " - " + getRoomName(curRoom3)))
                        {
                            subList3.Add(e);
                        }                        
                    }
                }

                curElevations = subList0.Concat(subList1).Concat(subList2).Concat(subList3).ToList();               

                int curViewPlaced = 0;
                ViewSheet curSheet = ViewSheet.Create(doc, tbId);
                curSheet.Name = "INTERIOR ELEVATIONS";

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

        private string getRoomName(Room i)
        {
            return i.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString().ToString();
        }
    }

}