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
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class CreateTiltUpElevationsByWalls : IExternalCommand
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

            //Element Collectors
            FilteredElementCollector vftCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))                
                .WhereElementIsElementType();

            FilteredElementCollector allWallsCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType();

            TaskDialog.Show("Selection", "Please select Tilt-Up walls to be elevated");

            IList<Reference> selectedWalls = uidoc.Selection.PickObjects(ObjectType.Element, "Select Walls to be Elevated");
            IList<Wall> curWalls = new List<Wall>();
            
            
            foreach (Reference w in selectedWalls)
            {
                if (doc.GetElement(w.ElementId).Category.Name == "Walls")
                    curWalls.Add(doc.GetElement(w.ElementId) as Wall);
            }

            //Variables            
            List<ViewFamilyType> vftList = new List<ViewFamilyType>(); 
            ElementId markerId = null;

            //Options to pass into the form (Dialog Box)            
            foreach (ViewFamilyType vft in vftCollector)
            {
                vftList.Add(vft);
            }

            //Dialog Box Settings
            FrmSelectElevationType curForm = new FrmSelectElevationType(vftList);
            curForm.Width = 700;
            curForm.Height = 200;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
           
            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {                
                foreach (ViewFamilyType vf in vftList)
                {
                    if (vf.FamilyName + ": " + vf.Name == curForm.GetSelectedElevationType())
                        markerId = vf.Id;
                }                
            }

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Create Tilt-Up Elevations");

            //Create interior elevations per room
            
            foreach (Wall cw in curWalls)
            {
                //Determines which way wall is flipped
                XYZ wallOrientation = cw.Orientation;
                if (cw.Flipped)
                    wallOrientation = new XYZ(-cw.Orientation.X, -cw.Orientation.Y, cw.Orientation.Z);
                Curve wallCurve = (cw.Location as LocationCurve).Curve;
                    
                XYZ wallStart = wallCurve.GetEndPoint(0);
                XYZ wallEnd = wallCurve.GetEndPoint(1);
                Line axis = Line.CreateBound(wallStart, wallEnd);
                XYZ wallCenter = null;
                XYZ elevOffset = new XYZ();
                int wallDirection = 0;
                bool notPerp = false; 

                //Sets values based on orientation
                if (wallOrientation.X == 1)
                {
                    wallCenter = new XYZ(wallStart.X, (wallStart.Y + wallEnd.Y)/2, wallStart.Z);
                    elevOffset = new XYZ(4, 0, 0);
                    wallDirection = 0;
                }
                else if (wallOrientation.X == -1)
                {
                    wallCenter = new XYZ(wallStart.X, (wallStart.Y + wallEnd.Y)/2, wallStart.Z);
                    elevOffset = new XYZ(-4, 0, 0);
                    wallDirection = 2;
                }
                else if (wallOrientation.Y == 1)
                {
                    wallCenter = new XYZ((wallStart.X + wallEnd.X)/2, wallStart.Y, wallStart.Z);
                    elevOffset = new XYZ(0, 4, 0);
                    wallDirection = 3;
                }
                else if (wallOrientation.Y == -1)
                {                     
                    wallCenter = new XYZ((wallStart.X + wallEnd.X)/2, wallStart.Y, wallStart.Z);
                    elevOffset = new XYZ(0, -4, 0);
                    wallDirection = 1;
                }
                else
                {
                    wallCenter = new XYZ((wallStart.X + wallEnd.X) / 2, (wallStart.Y + wallEnd.Y) / 2, wallStart.Z);
                    notPerp = true;

                    if (wallOrientation.X < 0 && wallOrientation.Y < 0)
                    {
                        if (cw.Flipped)
                        {
                            elevOffset = new XYZ(4, 4, 0);
                            wallDirection = 0;
                        }
                        else
                        {
                            elevOffset = new XYZ(-4, -4, 0);
                            wallDirection = 2;
                        }                           
                    }
                    else if (wallOrientation.X > 0 && wallOrientation.Y < 0)
                    {
                        if (cw.Flipped)
                        {
                            elevOffset = new XYZ(-4, 4, 0);
                            wallDirection = 2;
                        }
                        else
                        {
                            elevOffset = new XYZ(4, -4, 0);
                            wallDirection = 0;
                        }                            
                    }
                    else if (wallOrientation.X < 0 && wallOrientation.Y > 0)
                    {
                        if (cw.Flipped)
                        {
                            elevOffset = new XYZ(4, -4, 0);
                            wallDirection = 0;
                        }
                        else
                        {
                            elevOffset = new XYZ(-4, 4, 0);
                            wallDirection = 2;
                        }                            
                    }
                    else if (wallOrientation.X > 0 && wallOrientation.Y > 0)
                    {
                        if(cw.Flipped)
                        {
                            elevOffset = new XYZ(-4, -4, 0);
                            wallDirection = 2;
                        }
                        else
                        {
                            elevOffset = new XYZ(4, 4, 0);
                            wallDirection = 0;
                        }                            
                    }                        
                }

                //Z Axis for rotating elevations in plan
                XYZ start = new XYZ(wallCenter.X + elevOffset.X, wallCenter.Y + elevOffset.Y, wallCenter.Z);
                XYZ end = new XYZ(wallCenter.X + elevOffset.X, wallCenter.Y + elevOffset.Y, wallCenter.Z + 1);
                Line axis2 = Line.CreateBound(start, end);

                //Create elevation marker
                ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, markerId, wallCenter + elevOffset, 1);

                //Create elevation view and apply name
                ViewSection elevationView = marker.CreateElevation(doc, uidoc.ActiveView.Id, wallDirection);
                elevationView.Name = cw.Name + " - " + cw.Id;
                double viewdepth = 5;
                if (notPerp)
                    ElementTransformUtils.RotateElement(doc, marker.Id, axis2, -Math.Atan((wallStart.X - wallEnd.X) / (wallStart.Y - wallEnd.Y)));
                viewdepth = 7;

                //Get Wall Exterior Face
                IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(cw, ShellLayerType.Exterior);
                Face cwFace = doc.GetElement(sideFaces[0]).GetGeometryObjectFromReference(sideFaces[0]) as Face;
                IList<CurveLoop> cwBoundary = cwFace.GetEdgesAsCurveLoops();

                //Set view crop and other parameters for view
                elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(viewdepth);
                elevationView.CropBoxActive = true;
                elevationView.GetCropRegionShapeManager().SetCropShape(cwBoundary[0]);                    
            } 

            t.Commit();
            t.Dispose();

            failed:

            return Result.Succeeded;
        }
       
        private string GetWallTypeName(WallType i)
        {
            return i.FamilyName + ": " + i.Name;
        }        
    }
}