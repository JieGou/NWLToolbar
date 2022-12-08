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
    public class CreateInteriorElevations : IExternalCommand
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
            
            //Get interior elevation type
            ViewFamilyType vft = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault<ViewFamilyType>(x =>
                ViewFamily.Elevation == x.ViewFamily);

            //Needed to grab room boundry
            SpatialElementBoundaryOptions sEBO = new SpatialElementBoundaryOptions();
            sEBO.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Create Interior Elevations");

            //Create interior elevations per room
            foreach (Room r in roomCollector)
            {
                //Room information
                LocationPoint point = r.Location as LocationPoint;
                XYZ xyz = point.Point;
                Level roomLevel = r.Level;
                string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString().ToString();
                string roomNumber = r.Number;

                //Creates elevation body
                ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, xyz, 1);

                //Creates each elevation view
                for (int i = 0; i < 4; i++)
                {
                    //Gets room boundry segments                    
                    var filteredBoundaries = r.GetBoundarySegments(sEBO).ElementAt(0);                    
                    ViewSection elevationView = marker.CreateElevation(doc, uidoc.ActiveView.Id, i);

                    //custom method to get far clipping
                    double farClipOffset = GetViewDepth(filteredBoundaries, i, xyz);

                    string elevationName = roomNumber + " - " + roomName + " - " + Char.ConvertFromUtf32('a'+i);

                    //Set elevation parameters
                    elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(farClipOffset);
                    elevationView.get_Parameter(BuiltInParameter.VIEW_NAME).Set(elevationName);
                }                                
            }

            t.Commit();
            t.Dispose();

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