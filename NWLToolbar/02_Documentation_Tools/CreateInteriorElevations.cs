#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NWLToolbar.Utils;
using Application = Autodesk.Revit.ApplicationServices.Application;
using View = Autodesk.Revit.DB.View;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class CreateInteriorElevations : IExternalCommand
    {
        private List<Room> roomCollector;
        private List<ViewFamilyType> vftList;
        private List<ViewPlan> filteredPlans;
        private List<Ceiling> ceilingCollector;


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
            roomCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)                
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .OrderBy(x => x.Number).ToList();

            vftList = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))                
                .WhereElementIsElementType()
                .Cast<ViewFamilyType>()                
                .ToList();

            filteredPlans = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .WhereElementIsNotElementType()
                .Cast<ViewPlan>()
                .Where(x => x.ViewType.ToString() == "FloorPlan" && x.GenLevel != null)
                .DistinctBy(x => x.GenLevel.Id.IntegerValue)
                .ToList();

            ceilingCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Ceilings)
                .WhereElementIsNotElementType()
                .Cast<Ceiling>()
                .OrderByDescending(x => x.GetHeight())
                .ToList();

            List<Room> roomList = new List<Room>();
            List<string> roomListName = new List<string>();
            List<Room> selectedRoomList = new List<Room>();           
            ElementId markerId = null;

            foreach (Room e in roomCollector)
            {                
                bool isPlaced = e.Location != null;
                if (isPlaced == true)
                {
                    roomList.Add(e);
                    roomListName.Add(e.Number + " - " + e.GetName());
                }
            }
            
            //Dialog Box Settings
            FrmCreateInteriorElevations curForm = new FrmCreateInteriorElevations(roomListName, vftList);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
           
            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedViews = curForm.GetSelectedRooms();
                foreach (ViewFamilyType vf in vftList)
                {
                    if (vf.FamilyName + ": " + vf.Name == curForm.GetSelectedElevationType())
                        markerId = vf.Id;
                }

                foreach (string s in selectedViews)
                {
                    foreach (Room i in roomList)
                    {
                        if (s == i.Number + " - " + i.GetName())
                        {
                            selectedRoomList.Add(i);
                        }
                    }
                }
            }
            

            //Needed to grab room boundry
            SpatialElementBoundaryOptions sEBO = new SpatialElementBoundaryOptions();
            sEBO.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;

            Options cOptions = new Options();
            cOptions.DetailLevel = ViewDetailLevel.Fine;
            cOptions.IncludeNonVisibleObjects = true;
            cOptions.ComputeReferences = true;
            

            ElementCategoryFilter elFil = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);
            List<string> errorRooms = new List<string>();

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Create Interior Elevations");

            //Create interior elevations per room
            foreach (Room r in selectedRoomList)
            {
                //Room information
                LocationPoint point = r.Location as LocationPoint;
                XYZ xyz = point.Point;
                ElementId roomLevelId = r.Level.Id;
                ElementId planId = null;
                string roomName = r.GetName();
                string roomNumber = r.Number;
                double roomHeight = r.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
                double roomLevelHeight = (doc.GetElement(roomLevelId) as Level).ProjectElevation;
                bool clgFound = false;

                foreach (Ceiling c in ceilingCollector)
                {
                    
                    XYZ cXYZ = (c.get_Geometry(cOptions).GetBoundingBox().Max + c.get_Geometry(cOptions).GetBoundingBox().Min)/2;                    
                    if (r.IsPointInRoom(cXYZ))
                    {
                        roomHeight = c.GetHeight();                        
                        clgFound = true;
                        break;
                    }                    
                }
                if (!clgFound)
                    errorRooms.Add($"{r.Number} - {r.GetName()}");

                planId = filteredPlans.Where(x => x.GenLevel.Id == roomLevelId).First().Id;

                //Creates elevation body
                ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, markerId, xyz, 1);                

                //Creates each elevation view
                for (int i = 0; i < 4; i++)
                {
                    //Gets room boundry segments                    
                    var filteredBoundaries = r.GetBoundarySegments(sEBO).ElementAt(0);                    
                    ViewSection elevationView = marker.CreateElevation(doc, planId, i);

                    //custom method to get far clipping
                    double farClipOffset = GetViewDepth(filteredBoundaries, i, xyz);

                    //Set elevation name
                    string elevationName = roomNumber + " - " + roomName + " - " + Char.ConvertFromUtf32('a'+i);

                    //Set elevation parameters
                    elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(farClipOffset);
                    elevationView.get_Parameter(BuiltInParameter.VIEW_NAME).Set(elevationName);                    
                    elevationView.Scale = 48;

                    doc.Regenerate();

                    BoundingBoxXYZ curbb = elevationView.CropBox;
                    XYZ curMax = curbb.Max;
                    XYZ curMin = curbb.Min;
                    curMin = new XYZ(curMin.X, roomLevelHeight, curMin.Z);
                    curbb.Min = curMin;

                    doc.Regenerate();
                    
                    XYZ newMax = new XYZ(curMax.X, curMin.Y + roomHeight, curMax.Z); 
                    curbb.Max = newMax;

                    elevationView.CropBox = curbb;
                }                                
            }           

            t.Commit();
            t.Dispose();

            if (errorRooms.Count > 0)
                TaskDialog.Show("Error", $"The following rooms could not be cropped to the ceiling. Please manually adjust them.\n\n    {string.Join("\n    ", errorRooms)} ");

            return Result.Succeeded;
        }        

        private double GetViewDepth(IList<BoundarySegment> roomBoundry, double v1, XYZ roomCenter)
        {            
            if (v1 == 0)
            {                
                double depth = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).X);                
                return Math.Abs(roomCenter.X - depth);
            }
            if (v1 == 1)
            {
                double depth = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).Y);                
                return Math.Abs(depth - roomCenter.Y);
            }
            else if (v1 == 2)
            {
                double depth = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).X);
                return Math.Abs(roomCenter.X - depth);
            }
            //(v1 == 3)
            {
                double depth = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).Y);
                return Math.Abs(depth - roomCenter.Y);
            }             
        }
    }
}