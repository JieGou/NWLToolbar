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

            List<Room> roomList = new List<Room>();
            List<string> roomListName = new List<string>();
            List<Room> selectedRoomList = new List<Room>();            

            foreach (Room e in roomCollector)
            {
                bool isPlaced = e.Location != null;
                if (isPlaced == true)
                {
                    roomList.Add(e);
                    roomListName.Add(e.Number + " - " + getRoomName(e));
                }
            }
            //Dialog Box Settings
            FrmCreateInteriorElevations curForm = new FrmCreateInteriorElevations(roomListName);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedViews = curForm.GetSelectedRooms();

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

            ElementId tblock = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsElementType()
                    .FirstElement().Id;

            Transaction t = new Transaction(doc);
            t.Start("Place Elevations On Sheets");

            for (int i = 0; i < SheetsToCreate; i++)
            {
                IList<View> curElevations = new List<View>();

               
                int sPoint = i * 4;
                    
                Room curRoom = selectedRoomList[sPoint];

                Room curRoom2 = null;
                Room curRoom3 = null;
                Room curRoom4 = null;                    

                try
                {
                    curRoom2 = selectedRoomList[sPoint + 1];
                }
                catch
                { }
                    
                try
                {
                    curRoom3 = selectedRoomList[sPoint + 2];
                }
                catch
                { }   
                    
                try
                {
                    curRoom4 = selectedRoomList[sPoint + 3];
                }
                catch
                { }
                    
                foreach (View e in elevationViews)
                {
                    bool test = e.Name.Contains(curRoom.Number + " - " + getRoomName(curRoom));
                    if (test)
                    {
                        curElevations.Add(e);
                    }
                }
                foreach (View e in elevationViews)
                {
                    if (curRoom2 != null)
                    {
                        bool test = e.Name.Contains(curRoom2.Number + " - " + getRoomName(curRoom2));
                        if (test)
                        {
                            curElevations.Add(e);
                        }
                    }
                }
                foreach (View e in elevationViews)
                {
                    if (curRoom3 != null)
                    {
                        bool test = e.Name.Contains(curRoom3.Number + " - " + getRoomName(curRoom3));
                        if (test)
                        {
                            curElevations.Add(e);
                        }
                    }
                }
                foreach (View e in elevationViews)
                {
                    if (curRoom4 != null)
                    {
                        bool test = e.Name.Contains(curRoom4.Number + " - " + getRoomName(curRoom4));
                        if (test)
                        {
                            curElevations.Add(e);
                        }
                    }                        
                }                        
                
                int curViewPlaced = 0;
                ViewSheet curSheet = ViewSheet.Create(doc, tblock);
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


            if (curViewPlaced == 0)
                return boxBottomLeft;
            else if (curViewPlaced == 1)
                return new XYZ(boxBottomLeft.X + sizeX, boxBottomLeft.Y, 0);
            else if (curViewPlaced == 2)
                return new XYZ(boxBottomLeft.X + sizeX * 2, boxBottomLeft.Y, 0);
            else if (curViewPlaced == 3)
                return new XYZ(boxBottomLeft.X + sizeX * 3, boxBottomLeft.Y, 0);

            else if (curViewPlaced == 4)
                return new XYZ(boxBottomLeft.X, boxBottomLeft.Y + sizeY, 0);
            else if (curViewPlaced == 5)
                return new XYZ(boxBottomLeft.X + sizeX, boxBottomLeft.Y + sizeY, 0);
            else if (curViewPlaced == 6)
                return new XYZ(boxBottomLeft.X + sizeX * 2, boxBottomLeft.Y + sizeY, 0);
            else if (curViewPlaced == 7)
                return new XYZ(boxBottomLeft.X + sizeX * 3, boxBottomLeft.Y + sizeY, 0);

            else if (curViewPlaced == 8)
                return new XYZ(boxBottomLeft.X, boxBottomLeft.Y + sizeY * 2, 0);
            else if (curViewPlaced == 9)
                return new XYZ(boxBottomLeft.X + sizeX, boxBottomLeft.Y + sizeY * 2, 0);
            else if (curViewPlaced == 10)
                return new XYZ(boxBottomLeft.X + sizeX * 2, boxBottomLeft.Y + sizeY * 2, 0);
            else if (curViewPlaced == 11)
                return new XYZ(boxBottomLeft.X + sizeX * 3, boxBottomLeft.Y + sizeY * 2, 0);

            else if (curViewPlaced == 12)
                return new XYZ(boxBottomLeft.X, boxBottomLeft.Y + sizeY * 3, 0);
            else if (curViewPlaced == 13)
                return new XYZ(boxBottomLeft.X + sizeX, boxBottomLeft.Y + sizeY * 3, 0);
            else if (curViewPlaced == 14)
                return new XYZ(boxBottomLeft.X + sizeX * 2, boxBottomLeft.Y + sizeY * 3, 0);
            else if (curViewPlaced == 15)
                return new XYZ(boxBottomLeft.X + sizeX * 3, boxBottomLeft.Y + sizeY * 3, 0);
            else
                return new XYZ();
        }

        private string getRoomName(Room i)
        {
            return i.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString().ToString();
        }
    }

}