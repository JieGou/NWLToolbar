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
    public class ReCropElevation : IExternalCommand
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

            View curView = doc.ActiveView;

            List<Room> rooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfType<Room>()
                .Cast<Room>()
                .ToList();

            List<ElevationMarker> elevMarkers = new FilteredElementCollector(doc)
                .OfClass(typeof(ElevationMarker))
                .WhereElementIsNotElementType()
                .Cast<ElevationMarker>()
                .ToList();

            List<ViewPlan> filteredPlans = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .WhereElementIsNotElementType()
                .Cast<ViewPlan>()
                .Where(x => x.ViewType.ToString() == "FloorPlan" && x.GenLevel != null)
                .DistinctBy(x => x.GenLevel.Id.IntegerValue)
                .ToList();

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToList();


            ElevationMarker curElevMarker = null;
            XYZ curMarkerXYZ = null;
            ViewPlan curViewPlan = null;
            Room curRoom = null;
            BoundingBoxXYZ bb = null;
            XYZ direction = curView.ViewDirection;
            XYZ curMax = curView.CropBox.Max;
            XYZ curMin = curView.CropBox.Min;
            Level curLevel = null;

            ElementClassFilter elFil = new ElementClassFilter(typeof(View));

            SpatialElementBoundaryOptions sEBO = new SpatialElementBoundaryOptions();
            sEBO.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;

            foreach (ElevationMarker em in elevMarkers)
            {
                List<ElementId> eIdList = em.GetDependentElements(elFil).ToList();
                List<View> viewSections = new List<View>();

                foreach (ElementId eId in eIdList)
                {
                    viewSections.Add(doc.GetElement(eId) as View);
                }

                foreach (View m in viewSections)
                {
                    if (curView.Id == m.Id)
                    {
                        curElevMarker = em;
                        break;
                    }
                }
            }

            //need to figure out how to get view elevation

            Dictionary<ElementId, ViewPlan> viewPlanDict = filteredPlans.ToDictionary(x => x.GenLevel.Id);

            Dictionary<double, Level> levelDict = levels.ToDictionary(x => x.ProjectElevation);            

            curLevel = levelDict[levelDict.Keys.OrderBy(x => Math.Abs(x - curView.get_BoundingBox(curView).Min.Y)).First()];

            curViewPlan = viewPlanDict[curLevel.Id];

            rooms = rooms.Where(x => x.LevelId == curLevel.Id).ToList();
            
            foreach (Room r in rooms)
            {                
                bb = curElevMarker.get_BoundingBox(curViewPlan);
                curMarkerXYZ = bb.Max;

                if (r.IsPointInRoom(curMarkerXYZ))
                {
                    curRoom = r;
                    break;
                }
            }

            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Re-Crop Elevations");

            Ceiling selectedClg = null;
            double roomHeight = curRoom.UnboundedHeight + curLevel.ProjectElevation;            
            BoundingBoxXYZ tempBox = curView.CropBox;

            tempBox.Max = new XYZ(curMax.X, curRoom.Level.ProjectElevation + roomHeight, curMax.Z);
            tempBox.Min = new XYZ(curMin.X, curRoom.Level.ProjectElevation, curMin.Z);     
            
            curView.CropBox = tempBox;

            doc.Regenerate();

            try
            {
                selectedClg = new FilteredElementCollector(doc, curView.Id)
                 .OfClass(typeof(Ceiling))
                 .WhereElementIsNotElementType()
                 .Cast<Ceiling>()
                 .OrderByDescending(x => x.GetHeight())
                 .First();
            }
            catch
            {

            }
            if (selectedClg != null)
            {
                roomHeight = selectedClg.GetHeight()+curLevel.ProjectElevation;
            }

            //Gets room boundry segments                    
            var filteredBoundaries = curRoom.GetBoundarySegments(sEBO).ElementAt(0);

            //custom method to get far clipping
            //double farClipOffset = RevitUtils.GetViewDepth(filteredBoundaries, i, xyz);
            XYZ max = RevitUtils.GetRoomMax(filteredBoundaries, direction, roomHeight);
            XYZ min = RevitUtils.GetRoomMin(filteredBoundaries, direction, curLevel.ProjectElevation);

            BoundingBoxXYZ newBox = curView.CropBox;
            newBox.Max = max;
            newBox.Min = min;

            curView.CropBox = newBox;

            doc.Regenerate();

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        
    }

}
