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
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace NWLToolbar
{
    [Transaction(TransactionMode.Manual)]
    public class RenumberSheets : IExternalCommand
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

            //Get ViewSheets
            List<ViewSheet> sheetCollector = new FilteredElementCollector(doc)
                                .OfClass(typeof(ViewSheet))
                                .WhereElementIsNotElementType()
                                .Cast<ViewSheet>()
                                .ToList();
            try
            {
                sheetCollector = new FilteredElementCollector(doc, uidoc.Selection.GetElementIds())
                                .OfClass(typeof(ViewSheet))
                                .WhereElementIsNotElementType()
                                .Cast<ViewSheet>()
                                .OrderBy(x => x.SheetNumber)
                                .ToList();
            }
            catch
            {
                TaskDialog.Show("Error", "Please select sheets first before runing tool.");
                goto failed;
            }
            

            //Variables
            string sLetter = null;
            int sNumber = 0;
            bool sAppend = false;
            string sAppended = null;

            //Dialog Box Settings
            FrmRenumberSheets curForm = new FrmRenumberSheets();
            curForm.Width = 500;
            curForm.Height = 350;
            curForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Open Dialog Box & Add Selection to list
            if (curForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sLetter = curForm.GetSheetLetter();
                sNumber = curForm.GetSheetNumber();
                sAppend = curForm.GetSheetAppendBool();
                sAppended = curForm.GetSheetAppend();
            }

            int tracker = 0;

            //Start Transaction
            Transaction t = new Transaction(doc);
            t.Start("Renumber Sheets");

            foreach (ViewSheet s in sheetCollector)
            {                
                if (!sAppend)
                    s.SheetNumber = sLetter + (sNumber + tracker).ToString();                
                else
                    s.SheetNumber = sLetter + sNumber.ToString() + Char.ConvertFromUtf32(Convert.ToChar(sAppended) + tracker);

                tracker++;
            }

            TaskDialog.Show("Notice", "Please close and reopen project browser to see changes.\r\rTo toggle Project Browser, go to:\r\rView > User Interface > Project Browser");
            
            t.Commit();
            t.Dispose();

            failed:

            return Result.Succeeded;
        }
        
    }

}
