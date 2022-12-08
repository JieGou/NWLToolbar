#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Application = Autodesk.Revit.ApplicationServices.Application;
using View = Autodesk.Revit.DB.View;
#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class AlignPlans : IExternalCommand
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

            //View Origin Positioning
            IList<Element> selectedView = new List<Element>();
            TaskDialog.Show("Selection", "Please select a plan to use to align other plans");
            ElementId elementPickId = uidoc.Selection.PickObject(ObjectType.Element, "Select Element").ElementId;
            Element elementPick = doc.GetElement(elementPickId);
            selectedView.Add(elementPick);


            //Check for correct amount of views selected
            int numOfElementsSelected = 0;

            foreach (Element e in selectedView)
            {
                string cat = e.Category.Name.ToString();
                if (cat == "Viewports")
                {
                    numOfElementsSelected++;
                }
            }

            if (numOfElementsSelected != 1)
            {
                TaskDialog.Show("Failed", "Error, please select a plan view on a sheet");
                goto Failed;
            }

            //Viewport shape and location
            IList<CurveLoop> selectedViewCurveLoopList = new List<CurveLoop>();            
            XYZ selectedViewBoxCenter = new XYZ();           

            

            //Get Annotation Crop Array
            foreach (Viewport e in selectedView)
            {                
                View associatedView = doc.GetElement(e.ViewId) as View;
                selectedViewCurveLoopList = associatedView
                    .GetCropRegionShapeManager().GetCropShape();
                selectedViewBoxCenter = e.GetBoxCenter();
                
                
            }

            //Getting all views
            FilteredElementCollector allViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();
            //Getting all views
            FilteredElementCollector allSheets = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType();

            //All plan views as elements
            IList<Element> planViews = new List<Element>();

            //to pass into the Dialog Box
            List<String> planViewNames = new List<String>();

            foreach (View e in allViews)
            {
                string cat = e.ViewType.ToString();
                bool isTemplate = e.IsTemplate;
                if (cat == "FloorPlan" && isTemplate == false)
                {
                    planViews.Add(e);
                    planViewNames.Add(e.Name);
                }
            }
            
            //Sheet Selection
            List<Element> selectedElements = new List<Element>();

            //Dialog Box Settings
            FrmAlignPlans curForm = new FrmAlignPlans(planViewNames);
            curForm.Width = 700;
            curForm.Height = 900;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Filter for Viewports
            IList<Type> typeList = new List<Type>();
            typeList.Add(typeof(Viewport));
            ElementMulticlassFilter dependentFilterPlan = new ElementMulticlassFilter(typeList);

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedViews = curForm.GetSelectedSheets();

                foreach (string s in selectedViews)
                {
                    foreach (Element i in planViews)
                    {
                        if (s == i.Name)
                        {
                            selectedElements.Add(i);
                        }
                    }
                }
            }
            else
                goto Failed;


            //Get Sheets

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Align Plans");

            foreach (View v in selectedElements)
            {
                List<Element> sheetElement = new List<Element>();
                string sheetNum = v.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsValueString();
                
                foreach (ViewSheet vs in allSheets)
                {
                    if (vs.SheetNumber == sheetNum)
                    {
                        sheetElement.Add(vs);
                    }
                }
                foreach (ViewSheet svs in sheetElement)
                {
                    IList<ElementId> dependentElementsIds = svs.GetDependentElements(dependentFilterPlan);                    

                    foreach (ElementId eid in dependentElementsIds)
                    {
                        IList<Element> dependentElements = new List<Element>();
                        dependentElements.Add(doc.GetElement(eid));                        
                        
                        foreach (Viewport e in dependentElements)
                        {                            
                            string viewportName = e.get_Parameter(BuiltInParameter.VIEW_NAME).AsValueString();
                            string oViewportName = v.Name;
                            if (viewportName == oViewportName)
                            {                                
                                IList<CurveLoop> originalViewCurveLoop = new List<CurveLoop>();                                    
                                originalViewCurveLoop = v.GetCropRegionShapeManager().GetCropShape();
                                    
                                v.CropBoxActive = true;
                                v.CropBoxVisible = false;
                                v.GetCropRegionShapeManager().SetCropShape(selectedViewCurveLoopList[0]);
                                e.SetBoxCenter(selectedViewBoxCenter);
                                v.GetCropRegionShapeManager().SetCropShape(originalViewCurveLoop[0]);
                                v.get_BoundingBox(v);
                            }
                        }
                    }                    
                }
            }

           
            t.Commit();
            t.Dispose();
            
            Failed:

            return Result.Succeeded;
        }
    }
    
}