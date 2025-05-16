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
    public class ElevationCropThickness : IExternalCommand
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

            // Filtered Collecter 
            List<ViewFamilyType> vftList = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .WhereElementIsElementType()
                .Cast<ViewFamilyType>()
                .ToList();

            FilteredElementCollector selectedViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();
            
            ElementId viewType = null;

            IList<View> viewsOfType = new List<View>();

            int thickness = 0;

            FrmElevationCropThickness curForm = new FrmElevationCropThickness(vftList)
            {
                Width = 700,
                Height = 250,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                viewType = curForm.GetSelectedElevationType().Id;
                thickness = curForm.GetThickness();
            }
            else
                goto Failed;

            foreach (View v in selectedViews)
            {       
                if (v.get_Parameter(BuiltInParameter.VIEW_TYPE_SCHEDULES).AsValueString() == doc.GetElement(viewType).Name)
                {
                    viewsOfType.Add(v);
                }
            }

            OverrideGraphicSettings ovGS = new OverrideGraphicSettings();            
            ovGS.SetProjectionLineWeight(thickness);

            ElementCategoryFilter elFil = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);

            //Transaction Start
            Transaction t = new Transaction(doc);
            t.Start("Thicken Views");

            //Search For Sheets & Capitalize
            foreach (View i in viewsOfType)
            {
                i.CropBoxActive = true;
                i.CropBoxVisible = true;
                IList<ElementId> curDepElem = i.GetDependentElements(elFil);     
                foreach (ElementId depElem in curDepElem)
                    i.SetElementOverrides(depElem, ovGS);                
            }

            //Finish Transaction
            t.Commit();
            t.Dispose();

            Failed:

            return Result.Succeeded;
        }

       
    }

}
