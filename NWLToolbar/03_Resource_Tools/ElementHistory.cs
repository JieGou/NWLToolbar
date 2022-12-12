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
    public class ElementHistory : IExternalCommand
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

            //Filtered Eelement Collector (Collect Active Selection)
            FilteredElementCollector collector = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .WhereElementIsNotElementType();
            
            //Variables
            string creator = "";
            string lastChanged = "";

            //get Element Info
            foreach (Element e in collector)
            {
                ElementId id = e.Id;
                creator = WorksharingUtils.GetWorksharingTooltipInfo(doc, id).Creator.ToString();
                lastChanged = WorksharingUtils.GetWorksharingTooltipInfo(doc, id).LastChangedBy.ToString();
            }

            //Info Report
            TaskDialog.Show("Element History", "Creator:" + "\n" + creator + "\n \n" + "Last Changed By:" + "\n" + lastChanged);

            return Result.Succeeded;
        }
        
    }

}
