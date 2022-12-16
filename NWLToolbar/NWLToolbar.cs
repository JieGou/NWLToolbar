#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using NWLToolbar.Properties;

#endregion

namespace NWLToolbar
{
    internal class NWLToolbar : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            string tabName1 = "NWL Toolbar";
            application.CreateRibbonTab(tabName1);

            //Ribbon Sections
            RibbonPanel toolsPanel = application.CreateRibbonPanel(tabName1, "Modeling");
            RibbonPanel documentationPanel = application.CreateRibbonPanel(tabName1, "Documentation");
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
            PushButtonData pbd10 = new PushButtonData("AlignPlans", "Align Plans", curAssembly, "NWLToolbar.AlignPlans");
            PushButtonData pbd11 = new PushButtonData("Create Interior Elevation", "Create" + "\r" + "Interior Elevations", curAssembly, "NWLToolbar.CreateInteriorElevations");
            PushButtonData pbd12 = new PushButtonData("Place Elevations On Sheets", "Place Elevations" + "\r" + "On Sheets", curAssembly, "NWLToolbar.PlaceElevationsOnSheets");
            PushButtonData pbd13 = new PushButtonData("Create Tilt Up Elevation", "Create Tilt-Up" + "\r" + "Elevations", curAssembly, "NWLToolbar.CreateTiltUpElevations");

            //Pulldown Buttons
            PulldownButtonData pdbd1 = new PulldownButtonData("Align Notes Drop Down", "Align Notes");
            PulldownButtonData pdbd2 = new PulldownButtonData("Align Sheets Drop Down", "Align Sheets");

            //Images
            pbd1.LargeImage = BitMapToImageSource(Resources.aA);
            pbd2.LargeImage = BitMapToImageSource(Resources.teams);
            pbd3.LargeImage = BitMapToImageSource(Resources.overridden_dimensions);
            pbd4.LargeImage = BitMapToImageSource(Resources.element_history);
            pbd5.LargeImage = BitMapToImageSource(Resources.align_notes);
            pbd6.LargeImage = BitMapToImageSource(Resources.view_number);
            pbd7.LargeImage = BitMapToImageSource(Resources.align_notes);
            pbd8.LargeImage = BitMapToImageSource(Resources.align_sheets);
            pbd9.LargeImage = BitMapToImageSource(Resources.align_sheets);
            pbd10.LargeImage = BitMapToImageSource(Resources.align_plans);
            pbd11.LargeImage = BitMapToImageSource(Resources.interior_elevations);
            pbd12.LargeImage = BitMapToImageSource(Resources.int_elevations_on_sheets);
            pbd13.LargeImage = BitMapToImageSource(Resources.tilt_up_elevations);

            pdbd1.LargeImage = BitMapToImageSource(Resources.align_notes);
            pdbd2.LargeImage = BitMapToImageSource(Resources.align_sheets);

            //ToolTips
            pbd1.ToolTip = "Changes all sheet names to uppercase.";
            pbd2.ToolTip = "Opens up the \"BIM Tools & Resources\" page on the Microsoft Teams Application.";
            pbd3.ToolTip = "Selects all overridden dimensions in an active view.";
            pbd4.ToolTip = "Shows who created an element & who last changed it.";
            pbd5.ToolTip = "Aligns Text Notes and Keynotes to a detail line. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd6.ToolTip = "Renumbers all views on sheets based on the \"NWL_30x42\" title block.";
            pbd7.ToolTip = "Aligns Text Notes and Keynotes to a detail line & resize texts notes to match keynote width. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd8.ToolTip = "Takes all sheets and sets their origin back to (0,0,0)";

            pbd10.ToolTip = "Aligns plans accross multiple sheets. Select the plan to follow and then select the sheets you want to affect";
            pbd11.ToolTip = "Creates interior elevations for all rooms in the project";
            pbd12.ToolTip = "Places interior elevations on sheets based on rooms selected";
            pbd13.ToolTip = "Creates Tilt-Up Elevations based on the exterior side of the wall";

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
            PushButton pb10 = (PushButton)toolsPanel.AddItem(pbd10);

            //Documentation Section
            PushButton pb11 = (PushButton)documentationPanel.AddItem(pbd11);
            PushButton pb12 = (PushButton)documentationPanel.AddItem(pbd12);
            PushButton pb13 = (PushButton)documentationPanel.AddItem(pbd13);


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
        private static ImageSource BitMapToImageSource(Bitmap bm)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                bm.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = ms;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                return bmi;
            }
        }
    }
}
