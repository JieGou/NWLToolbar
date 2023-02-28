#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class CreateTiltUpElevations : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Element Collectors
            List<WallType> wallTypeList = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType().Cast<WallType>().ToList();

            List<ViewFamilyType> vftList = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .WhereElementIsElementType().Cast<ViewFamilyType>().ToList();

            FilteredElementCollector allWallsCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType();

            //Variables
            List<WallType> selectedWallTypeList = new List<WallType>();
            ElementId markerId = null;

            //Dialog Box Settings
            FrmCreateTiltUpElevations curForm = new FrmCreateTiltUpElevations(wallTypeList, vftList);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedWallTypes = curForm.GetSelectedWallTypes();
                foreach (ViewFamilyType vf in vftList)
                {
                    if (vf.GetName() == curForm.GetSelectedElevationType())
                        markerId = vf.Id;
                }

                Dictionary<string, WallType> map = wallTypeList.ToDictionary(x => x.GetName());

                foreach (string s in selectedWallTypes)
                {
                    if (map.ContainsKey(s))
                    {
                        selectedWallTypeList.Add(map[s]);
                    }
                }
            }

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Create Tilt-Up Elevations");

            //Create interior elevations per room
            foreach (WallType wt in selectedWallTypeList)
            {
                List<Wall> curWalls = allWallsCollector.Cast<Wall>().Where(x => x.WallType.Name == wt.Name).ToList();

                foreach (Wall cw in curWalls)
                {
                    //Determines which way wall is flipped
                    XYZ wallOrientation = cw.Orientation;
                    if (cw.Flipped)
                        wallOrientation = new XYZ(-cw.Orientation.X, -cw.Orientation.Y, cw.Orientation.Z);

                    Curve wallCurve = (cw.Location as LocationCurve).Curve;
                    XYZ wallStart = wallCurve.GetEndPoint(0);
                    XYZ wallEnd = wallCurve.GetEndPoint(1);
                    XYZ wallCenter = new XYZ((wallStart.X + wallEnd.X) / 2, (wallStart.Y + wallEnd.Y) / 2, wallStart.Z);
                    XYZ elevOffset = new XYZ();
                    int wallDirection = 0;
                    bool notPerp = false;
                    int viewOffset = 4;

                    //Sets values based on orientation
                    if (Math.Abs(wallOrientation.X) == 1)
                    {
                        elevOffset = new XYZ(wallOrientation.X * viewOffset, 0, 0);
                        wallDirection = wallOrientation.X == 1 ? 0 : 2;
                    }
                    else if (Math.Abs(wallOrientation.Y) == 1)
                    {
                        elevOffset = new XYZ(0, wallOrientation.Y * viewOffset, 0);
                        wallDirection = wallOrientation.Y == 1 ? 3 : 1;
                    }
                    else
                    {
                        notPerp = true;
                        int xFlag = wallOrientation.X < 0 ? -1 : 1;
                        int yFlag = wallOrientation.Y < 0 ? -1 : 1;

                        wallDirection = wallOrientation.X > 0 ? 0 : 2;
                        elevOffset = new XYZ(viewOffset * xFlag, viewOffset * yFlag, 0);
                    }

                    //Z Axis for rotating elevations in plan
                    XYZ start = new XYZ(wallCenter.X + elevOffset.X, wallCenter.Y + elevOffset.Y, wallCenter.Z);
                    XYZ end = new XYZ(wallCenter.X + elevOffset.X, wallCenter.Y + elevOffset.Y, wallCenter.Z + 1);
                    Line axis = Line.CreateBound(start, end);

                    //Create elevation marker
                    ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, markerId, wallCenter + elevOffset, 1);

                    //Create elevation view and apply name
                    ViewSection elevationView = marker.CreateElevation(doc, uidoc.ActiveView.Id, wallDirection);
                    elevationView.Name = cw.Name + " - " + cw.Id;
                    double viewdepth = viewOffset + 1;

                    if (notPerp)
                    {
                        ElementTransformUtils.RotateElement(doc, marker.Id, axis, -Math.Atan((wallStart.X - wallEnd.X) / (wallStart.Y - wallEnd.Y)));
                        viewdepth = viewOffset + 3;
                    }

                    //Get Wall Exterior Face
                    IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(cw, ShellLayerType.Exterior);
                    Face cwFace = doc.GetElement(sideFaces[0]).GetGeometryObjectFromReference(sideFaces[0]) as Face;
                    IList<CurveLoop> cwBoundary = cwFace.GetEdgesAsCurveLoops();

                    //Set view crop and other parameters for view
                    elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(viewdepth);
                    elevationView.CropBoxActive = true;
                    elevationView.GetCropRegionShapeManager().SetCropShape(cwBoundary[0]);
                }
            }

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
    }
}