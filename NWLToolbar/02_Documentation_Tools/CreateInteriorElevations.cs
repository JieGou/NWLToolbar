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

        public static bool SetParameter(Element elem, BuiltInParameter index, ElementId value)
        {
            Parameter parameter = GetParameter(elem, index);
            if (parameter == null)
            {
                return false;
            }
            if (parameter.IsReadOnly)
            {
                return false;
            }
            StorageType storageType = parameter.StorageType;
            return StorageType.ElementId == storageType && parameter.Set(value);
        }

        public static Parameter GetParameter(Element elem, BuiltInParameter index)
        {
            return elem.get_Parameter(index);
        }

        public static View GetElevationViewTemplate(Document doc, string viewName)
        {
            IList<Autodesk.Revit.DB.View> source = FilterElements<View>(doc);
            List<Autodesk.Revit.DB.View> views = source.ToList<Autodesk.Revit.DB.View>();
            if (views == null || views.Count < 1) return null;
            foreach (View view in views)
            {
                if (view == null || !view.IsTemplate) continue;
                if (view.ViewType != ViewType.Elevation) continue;
                //templateViews.Add(view);
                if (view.Name == viewName) return view;
            }
            return null;
        }

        public static List<T> FilterElements<T>(Document doc, ElementFilter filter = null) where T : class
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
            if (filter != null)
            {
                return filteredElementCollector.WherePasses(filter).OfClass(typeof(T)).OfType<T>().ToList<T>();
            }
            return filteredElementCollector.OfClass(typeof(T)).OfType<T>().ToList<T>();
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Collectors
            roomCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(x => x.Area > 0 && x.Location != null)
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
                .Where(x => x.ViewType == ViewType.FloorPlan && x.GenLevel != null)
                .DistinctBy(x => x.GenLevel.Id.IntegerValue)
                .ToList();

            ceilingCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Ceilings)
                .WhereElementIsNotElementType()
                .Cast<Ceiling>()
                .OrderByDescending(x => x.GetHeight())
                .ToList();

            //Variables
            List<Room> selectedRoomList = new List<Room>();
            ElementId markerId = null;

            //Dialog Box Settings
            var curForm = new FrmSelectRoomAndElevationType(roomCollector, vftList)
            {
                Width = 700,
                Height = 900,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                markerId = curForm.GetSelectedElevationType().Id;
                selectedRoomList = curForm.GetSelectedRooms();
            }

            //Needed to grab room boundry
            var sEBO = new SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary
            };

            var cOptions = new Options
            {
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = true,
                ComputeReferences = true
            };

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
                XYZ roomCenterPt = point.Point;
                ElementId roomLevelId = r.Level.Id;
                ElementId planId = null;
                string roomName = r.GetName();
                string roomNumber = r.Number;
                double roomHeight = r.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
                double roomLevelHeight = (doc.GetElement(roomLevelId) as Level).ProjectElevation;

                //是否找到房间吊顶
                bool clgFound = false;
                foreach (Ceiling c in ceilingCollector)
                {
                    BoundingBoxXYZ ceilingBounding = c.get_Geometry(cOptions).GetBoundingBox();
                    XYZ ceilingCenterPt = (ceilingBounding.Max + ceilingBounding.Min) / 2;
                    if (r.IsPointInRoom(ceilingCenterPt))
                    {
                        roomHeight = c.GetHeight();
                        clgFound = true;
                        break;
                    }
                }

                planId = filteredPlans.First(x => x.GenLevel.Id == roomLevelId).Id;

                //测试 偏移(-1000,1000,0)
                //xyz = new XYZ(xyz.X - 1000 / 304.8, xyz.Y + 1000 / 304.8, xyz.Z);
                //Creates elevation body
                ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, markerId, roomCenterPt, 1);

                //获取视图样板
                View template = GetElevationViewTemplate(doc, "20出图_NS_立面");
                //Creates each elevation view
                for (int i = 0; i < 4; i++)
                {
                    //Gets room boundry segments
                    var filteredBoundaries = r.GetBoundarySegments(sEBO).ElementAt(0);
                    ViewSection elevView = marker.CreateElevation(doc, planId, i);

                    //设定视图样板:20出图_NS_⽴⾯
                    if (template != null) SetParameter(elevView, BuiltInParameter.VIEW_TEMPLATE, template.Id);

                    //custom method to get far clipping
                    double farClipOffset = RevitUtils.GetViewDepth(filteredBoundaries, i, roomCenterPt);

                    //Set elevation name
                    string elevationName = $"{roomNumber} - {roomName} - {i + 1}";

                    //Set elevation parameters
                    //elevView.DetailLevel = ViewDetailLevel.Fine;

                    //设置远裁剪偏移
                    elevView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(farClipOffset /*+ 1000 / 304.8*/);
                    elevView.get_Parameter(BuiltInParameter.VIEW_NAME).Set(elevationName);
                    elevView.Scale = 48;

                    doc.Regenerate();

                    BoundingBoxXYZ curbb = elevView.CropBox;
                    XYZ curMax = curbb.Max;
                    XYZ curMin = curbb.Min;
                    curMin = new XYZ(curMin.X, roomLevelHeight, curMin.Z);
                    curbb.Min = curMin;

                    doc.Regenerate();

                    XYZ newMax = new XYZ(curMax.X, curMin.Y + roomHeight, curMax.Z);
                    curbb.Max = newMax;

                    elevView.CropBox = curbb;

                    if (!clgFound)
                    {
                        Ceiling selectedClg = null;
                        try
                        {
                            selectedClg = new FilteredElementCollector(doc, elevView.Id)
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
                            roomHeight = selectedClg.GetHeight();
                            clgFound = true;
                            curMin = new XYZ(curMin.X, roomLevelHeight, curMin.Z);
                            curbb.Min = curMin;

                            doc.Regenerate();

                            newMax = new XYZ(curMax.X, curMin.Y + roomHeight, curMax.Z);
                            curbb.Max = newMax;

                            elevView.CropBox = curbb;
                        }
                    }
                }
                if (!clgFound)
                    errorRooms.Add(r.GetNumName());
            }

            t.Commit();
            t.Dispose();

            if (errorRooms.Count > 0)
                TaskDialog.Show("Error", $"The following rooms could not be cropped to the ceiling. Please manually adjust them.\n\n    {string.Join("\n    ", errorRooms)} ");

            return Result.Succeeded;
        }
    }
}