#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

#endregion

namespace NWLToolbar
{
    internal class NWLToolbar : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            string tabName1 = "NWLToolbar";
            application.CreateRibbonTab(tabName1);

            //Ribbon Sections
            RibbonPanel toolsPanel = application.CreateRibbonPanel(tabName1, "Tools");            
            RibbonPanel dimensionsPanel = application.CreateRibbonPanel(tabName1, "Dimensions");
            RibbonPanel resourcesPanel = application.CreateRibbonPanel(tabName1, "Resources");
            

            string curAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string curAssemblyPath = System.IO.Path.GetDirectoryName(curAssembly);

            //Push Buttons
            PushButtonData pbd1 = new PushButtonData("Sheets to Uppercase", "Sheets Names" + "\r" + "To Uppercase", curAssembly, "NWLToolbar.CapitalizeSheets");
            PushButtonData pbd2 = new PushButtonData("Teams Link", "BIM Tools" + "\r" + "& Resources", curAssembly, "NWLToolbar.TeamsLink");
            PushButtonData pbd3 = new PushButtonData("Overridden Dims", "Select Overridden" + "\r" + "Dimensions", curAssembly, "NWLToolbar.SelectOverriddenDimensions");
            PushButtonData pbd4 = new PushButtonData("Element History", "Element" + "\r" + "History", curAssembly, "NWLToolbar.ElementHistory");
            PushButtonData pbd5 = new PushButtonData("Align Notes", "Align Notes W/ Detail Line", curAssembly, "NWLToolbar.AlignNotes");
            PushButtonData pbd6 = new PushButtonData("Renumber Views on Sheet", "Renumber Views" + "\r" + "On Sheet", curAssembly, "NWLToolbar.RenumberViewsOnSheet");
            PushButtonData pbd7 = new PushButtonData("Align Notes & Resize Text", "Align Notes W/ Detail Line & Resize", curAssembly, "NWLToolbar.AlignNotesAndResize");
            PushButtonData pbd8 = new PushButtonData("Align All Sheets To Origin", "Align All Sheets" + "\r" + "To Origin", curAssembly, "NWLToolbar.AlignAllSheetsToOrigin");
            PushButtonData pbd9 = new PushButtonData("Align Sheet To Origin", "Align Sheet" + "\r" + "To Origin", curAssembly, "NWLToolbar.AlignSheetToOrigin");

            //Pulldown Buttons
            PulldownButtonData pdbd1 = new PulldownButtonData("Align Notes Drop Down", "Align Notes");
            PulldownButtonData pdbd2 = new PulldownButtonData("Align Sheets Drop Down", "Align Sheets");
            
            
            

            //Images
            pbd1.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "aA.png")));
            pbd2.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "teams.png")));
            pbd3.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "overridden dimensions.png")));
            pbd4.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "element history.png")));
            pbd5.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "align notes.png")));
            pbd6.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "view number.png")));
            pbd7.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "align notes.png")));
            pbd8.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "view number.png")));
            pbd9.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "view number.png")));


            pdbd1.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "align notes.png")));
            pdbd2.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "view number.png")));

            //ToolTips
            pbd1.ToolTip = "Changes all sheet names to uppercase.";
            pbd2.ToolTip = "Opens up the \"BIM Tools & Resources\" page on the Microsoft Teams Application.";
            pbd3.ToolTip = "Selects all overridden dimensions in an active view.";
            pbd4.ToolTip = "Shows who created an element & who last changed it.";
            pbd5.ToolTip = "Aligns Text Notes and Keynotes to a detail line. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd6.ToolTip = "Renumbers all views on sheets based on the \"NWL_30x42\" title block.";
            pbd7.ToolTip = "Aligns Text Notes and Keynotes to a detail line & resize texts notes to match keynote width. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd8.ToolTip = "Takes all sheets and sets their origin back to (0,0,0)";

            IList<PushButtonData> alignNotesList = new List<PushButtonData>();
            alignNotesList.Add(pbd5);
            alignNotesList.Add(pbd7);

            IList<PushButtonData> alignSheetsList = new List<PushButtonData>();
            alignSheetsList.Add(pbd9);
            alignSheetsList.Add(pbd8);

            //Tools Section
            PushButton pb1 = (PushButton)toolsPanel.AddItem(pbd1);
            PulldownButton pdb1 = (PulldownButton)toolsPanel.AddItem(pdbd1);
                pdb1.AddPushButton(pbd5);
                pdb1.AddPushButton(pbd7);            
            PushButton pb6 = (PushButton)toolsPanel.AddItem(pbd6);
            PulldownButton pdb2 = (PulldownButton)toolsPanel.AddItem(pdbd2);
            foreach (PushButtonData pbd in alignSheetsList)
                pdb2.AddPushButton(pbd);
            

            //Dimensions Section
            PushButton pb3 = (PushButton)dimensionsPanel.AddItem(pbd3);

            //Resources Section
            PushButton pb4 = (PushButton)resourcesPanel.AddItem(pbd4);
            PushButton pb2 = (PushButton)resourcesPanel.AddItem(pbd2);
            


        }
        public Result OnStartup(UIControlledApplication a)
        {

            AddRibbonPanel(a);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
