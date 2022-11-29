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
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

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

            FilteredElementCollector activeView = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                .WhereElementIsNotElementType();

            foreach (Viewport e in activeView)
            {
               
            }



            //Getting all sheets
            FilteredElementCollector sheets = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets)                
                .WhereElementIsNotElementType();

            List<string> sheetNames = new List<string>();
            List<SheetInfo> sheetInfo = new List<SheetInfo>();

            foreach (ViewSheet s in sheets)
            {
                string combo = s.SheetNumber.ToString() + " - " + s.Name.ToString();
                sheetNames.Add(combo);
                 
                SheetInfo sheetInfo1 = new SheetInfo();
                sheetInfo1.number = s.SheetNumber;
                sheetInfo1.name = s.Name.ToString();
                sheetInfo1.eId = s.Id;
                sheetInfo1.combined = combo;
                sheetInfo.Add(sheetInfo1);
            }

            //Sheet Selection

            List<ElementId> selectedElementIds = new List<ElementId>();
            List<Element> selectedElements = new List<Element>();

            FrmAlignPlans curForm = new FrmAlignPlans(sheetNames);
                curForm.Width = 700;
                curForm.Height = 900;
                curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selectedSheets = curForm.GetSelectedSheets();

                foreach (string s in selectedSheets)
                {
                    foreach (SheetInfo i in sheetInfo)
                    {
                        if (s == i.combined)
                        {
                            selectedElementIds.Add(i.eId);
                        }
                    } 
                }
            }


            
            foreach (ElementId i in selectedElementIds)
                selectedElements.Add(doc.GetElement(i));

            //Variables

            IList<Type> typeList = new List<Type>();
            typeList.Add(typeof(Viewport));
            ElementMulticlassFilter dependentFilter = new ElementMulticlassFilter(typeList);

            //Transaction start
            Transaction t = new Transaction(doc);
            t.Start("Align Plans");


            foreach (Element e in selectedElements)
            {
                IList<ElementId> dependentElementIds = e.GetDependentElements(dependentFilter);
                IList<Element> dependentElement = new List<Element>();

                foreach (ElementId e1 in dependentElementIds)
                {
                    Element element = doc.GetElement(e1);
                    dependentElement.Add(element);
                    string elementName = element.Category.Name;


                    //Set Offset Based On Title Block Positioning
                    if (elementName == "Plan View")
                    {

                    }
                }
            }

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }
        public class SheetInfo
        {
            public ElementId eId = null;
            public string name = null;
            public string number = null;
            public string combined = null;
            
        }

    }
    
}