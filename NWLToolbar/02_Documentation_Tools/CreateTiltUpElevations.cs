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
            var curForm = new FrmCreateTiltUpElevations(wallTypeList, vftList)
            {
                Width = 700,
                Height = 900,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

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

            //创建墙立面
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
                    XYZ elevOffsetVector = new XYZ();

                    //创建立面的标志序号 0左 1上 2右 3下
                    int elevationIndex = 0;
                    //斜墙 非垂直
                    bool isObliqueWall = false;
                    int viewOffset = 4;

                    //立面符号放置在 前进方向的左边
                    //Sets values based on orientation
                    double wallOrientationX = wallOrientation.X;
                    double wallOrientationY = wallOrientation.Y;
                    if (Math.Abs(wallOrientationX) == 1)//与Y轴平行的墙 yAxisAligned
                    {
                        elevOffsetVector = new XYZ(wallOrientationX * viewOffset, 0, 0);
                        elevationIndex = wallOrientationX == 1 ? 0 : 2;
                    }
                    else if (Math.Abs(wallOrientationY) == 1)// 与X轴平等的墙 xAxisAligned
                    {
                        elevOffsetVector = new XYZ(0, wallOrientationY * viewOffset, 0);
                        elevationIndex = wallOrientationY == 1 ? 3 : 1;
                    }
                    else//倾斜墙 不与X Y轴平行的
                    {
                        isObliqueWall = true;
                        int xFlag = wallOrientationX < 0 ? -1 : 1;
                        int yFlag = wallOrientationY < 0 ? -1 : 1;

                        elevationIndex = wallOrientationX > 0 ? 0 : 2;
                        elevOffsetVector = new XYZ(viewOffset * xFlag, viewOffset * yFlag, 0);
                        elevOffsetVector = viewOffset * wallOrientation;
                    }

                    //立面符号放置位置
                    XYZ elevMarkerOrigin = new XYZ(wallCenter.X + elevOffsetVector.X, wallCenter.Y + elevOffsetVector.Y, wallCenter.Z);
                    elevMarkerOrigin = wallCenter + elevOffsetVector;
                    XYZ end = elevMarkerOrigin + XYZ.BasisZ;
                    //Z Axis for rotating elevations in plan
                    Line axis = Line.CreateBound(elevMarkerOrigin, end);

                    //Create elevation marker
                    int scale = 1;
                    ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, markerId, elevMarkerOrigin, scale);

                    //Create elevation view and apply name
                    ViewSection elevationView = marker.CreateElevation(doc, uidoc.ActiveView.Id, elevationIndex);
                    elevationView.Name = cw.Name + " - " + cw.Id + "_墙立面";
                    double viewdepth = viewOffset + 1;

                    if (isObliqueWall)
                    {
                        double deltaX = wallStart.X - wallEnd.X;
                        double deltaY = wallStart.Y - wallEnd.Y;
                        double angle = Math.Atan(deltaX / deltaY);
                        ElementTransformUtils.RotateElement(doc, marker.Id, axis, -angle);
                    }

                    //设置视图深度
                    elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(viewdepth);

                    //Get Wall Exterior Face
                    IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(cw, ShellLayerType.Exterior);
                    Face cwFace = doc.GetElement(sideFaces[0]).GetGeometryObjectFromReference(sideFaces[0]) as Face;
                    if (cwFace != null)
                    {
                        IList<CurveLoop> cwBoundary = cwFace.GetEdgesAsCurveLoops();

                        if (cwBoundary != null)
                        {
                            //设置 立面剪裁
                            elevationView.CropBoxActive = true;
                            elevationView.GetCropRegionShapeManager().SetCropShape(cwBoundary[0]);
                        }
                    }
                }
            }

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
    }
}