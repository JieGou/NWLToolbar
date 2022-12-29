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

            FrmElevationCropThickness curForm = new FrmElevationCropThickness(vftList);
            curForm.Width = 700;
            curForm.Height = 250;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                viewType = curForm.GetSelectedElevationType().Id;
                thickness = curForm.GetThickness();
            }

            foreach (View v in selectedViews)
            {
                string a = v.get_Parameter(BuiltInParameter.VIEW_TYPE_SCHEDULES).AsValueString() ;
                string b = doc.GetElement(viewType).Name;

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
                IList<ElementId> curDepElem = i.GetDependentElements(elFil);     
                foreach (ElementId depElem in curDepElem)
                    i.SetElementOverrides(depElem, ovGS);                
            }

            //Finish Transaction
            t.Commit();
            t.Dispose();            

            return Result.Succeeded;
        }

       
    }

}
