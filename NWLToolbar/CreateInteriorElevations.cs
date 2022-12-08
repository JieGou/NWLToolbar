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
                string roomName = r.Name;
                string roomNumber = r.Number;

                //Creates elevation body
                ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, xyz, 1);

                //Creates each elevation view
                for (int i = 0; i < 4; i++)
                {
                    //Gets room boundry segments
                    IList<IList<BoundarySegment>> roomBoundry = r.GetBoundarySegments(sEBO);
                    IList<BoundarySegment> filteredBoundaries = roomBoundry.ElementAt(0);                    
                    ViewSection elevationView = marker.CreateElevation(doc, uidoc.ActiveView.Id, i);

                    //custom method to get far clipping
                    double farClipOffset = GetViewDepth(filteredBoundaries, i, xyz);

                    string letter = null;
                    if (i == 0)
                        letter = "a";
                    else if (i == 1)
                        letter = "b";
                    else if (i == 2)
                        letter = "c";
                    else if (i == 3)
                        letter = "d";                    

                    string elevationName = roomNumber + " - " + roomName + " - " + letter;

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
            double depth = 1;
            if (v1 == 0)
            {
                double depth1 = roomBoundry.First().GetCurve().GetEndPoint(0).X;
                foreach (BoundarySegment r in roomBoundry)
                {
                    double endPoint = r.GetCurve().GetEndPoint(0).X;
                    if (endPoint < depth1)
                        depth1 = endPoint;
                }
                depth = Math.Abs(roomCenter.X - depth1);
            }
            else if (v1 == 1)
            {
                double depth1 = roomBoundry.First().GetCurve().GetEndPoint(0).Y;
                foreach (BoundarySegment r in roomBoundry)
                {
                    double endPoint = r.GetCurve().GetEndPoint(0).Y;
                    if (endPoint > depth1)
                        depth1 = endPoint;
                }
                depth = Math.Abs(depth1 - roomCenter.Y);
            }
            if (v1 == 2)
            {
                double depth1 = roomBoundry.First().GetCurve().GetEndPoint(0).X;
                foreach (BoundarySegment r in roomBoundry)
                {
                    double endPoint = r.GetCurve().GetEndPoint(0).X;
                    if (endPoint > depth1)
                        depth1 = endPoint;
                }
                depth = Math.Abs(depth1 - roomCenter.X);
            }
            if (v1 == 3)
            {
                double depth1 = roomBoundry.First().GetCurve().GetEndPoint(0).Y;
                foreach (BoundarySegment r in roomBoundry)
                {
                    double endPoint = r.GetCurve().GetEndPoint(0).Y;
                    if (endPoint < depth1)
                        depth1 = endPoint;
                }
                depth = Math.Abs(roomCenter.Y - depth1);
            }            
            return depth;
        }
    }

}