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
            //RibbonPanel ModelPanel = application.CreateRibbonPanel(tabName1, "Modeling");
            RibbonPanel documentationPanel = application.CreateRibbonPanel(tabName1, "Documentation");
            RibbonPanel dimensionsPanel = application.CreateRibbonPanel(tabName1, "Dimensions");
            RibbonPanel resourcesPanel = application.CreateRibbonPanel(tabName1, "Resources");
            RibbonPanel betaPanel = application.CreateRibbonPanel(tabName1, "Beta");

            string curAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string curAssemblyPath = System.IO.Path.GetDirectoryName(curAssembly);

            //Push Buttons
            PushButtonData pbd1 = new PushButtonData("Sheets to Uppercase", "Sheet Names\rTo Uppercase", curAssembly, "NWLToolbar.CapitalizeSheets");
            PushButtonData pbd2 = new PushButtonData("Teams Link", "BIM Tools\r& Resources", curAssembly, "NWLToolbar.TeamsLink");
            PushButtonData pbd3 = new PushButtonData("Overridden Dims", "Select Overridden\rDimensions", curAssembly, "NWLToolbar.SelectOverriddenDimensions");
            PushButtonData pbd4 = new PushButtonData("Element History", "Element\rHistory", curAssembly, "NWLToolbar.ElementHistory");
            PushButtonData pbd5 = new PushButtonData("Align Notes", "Align Notes W/ Detail Line", curAssembly, "NWLToolbar.AlignNotes");
            PushButtonData pbd6 = new PushButtonData("Renumber Views on Sheet", "Renumber Views\rOn Sheet", curAssembly, "NWLToolbar.RenumberViewsOnSheet");
            PushButtonData pbd7 = new PushButtonData("Align Notes & Resize Text", "Align Notes W/ Detail Line & Resize", curAssembly, "NWLToolbar.AlignNotesAndResize");
            PushButtonData pbd8 = new PushButtonData("Align All Sheets To Origin", "Align All Sheets\rTo Origin", curAssembly, "NWLToolbar.AlignAllSheetsToOrigin");
            PushButtonData pbd9 = new PushButtonData("Align Sheet To Origin", "Align Sheet\rTo Origin", curAssembly, "NWLToolbar.AlignSheetToOrigin");
            PushButtonData pbd10 = new PushButtonData("AlignPlans", "Align Plans", curAssembly, "NWLToolbar.AlignPlans");
            PushButtonData pbd11 = new PushButtonData("Create Interior Elevation", "Create\rInterior Elevations", curAssembly, "NWLToolbar.CreateInteriorElevations");
            PushButtonData pbd12 = new PushButtonData("Place Elevations On Sheets", "Place Elevations\rOn Sheets", curAssembly, "NWLToolbar.PlaceElevationsOnSheets");
            PushButtonData pbd13 = new PushButtonData("Create Tilt Up Elevation", "Create Tilt-Up\rElevations", curAssembly, "NWLToolbar.CreateTiltUpElevations");
            PushButtonData pbd14 = new PushButtonData("Create Tilt Up Elevation By Selection", "Create Tilt-Up\rElevations by Selection", curAssembly, "NWLToolbar.CreateTiltUpElevationsByWalls");
            PushButtonData pbd15 = new PushButtonData("Thicken Elevation Crop", "Thicken\rElevation Crop", curAssembly, "NWLToolbar.ElevationCropThickness");
            PushButtonData pbd16 = new PushButtonData("Purge Rooms", "Purge Rooms", curAssembly, "NWLToolbar.PurgeRooms");
            PushButtonData pbd17 = new PushButtonData("Renumber Sheets", "Renumber Sheets", curAssembly, "NWLToolbar.RenumberSheets");
            PushButtonData pbd18 = new PushButtonData("Place Views On Sheets", "Place Views" + "\r" + "On Sheets", curAssembly, "NWLToolbar.PlaceViewsOnSheets");
            PushButtonData pbd19 = new PushButtonData("Re Crop Elevation", "Re Crop\rElevation", curAssembly, "NWLToolbar.ReCropElevation");
            PushButtonData pbd20 = new PushButtonData("Rooms to Uppercase", "Room Names\rTo Uppercase", curAssembly, "NWLToolbar.CapitalizeRooms");

            //Pulldown Buttons
            PulldownButtonData pdbd1 = new PulldownButtonData("Align Notes Drop Down", "  Align  \r  Notes  ");
            PulldownButtonData pdbd2 = new PulldownButtonData("Place Views on Sheet", "Place Views\rOn Sheets");
            PulldownButtonData pdbd3 = new PulldownButtonData("Create Tilt Up Elevations Drop Down", "Create Tilt-Up\rElevations");
            PulldownButtonData pdbd4 = new PulldownButtonData("File Clean up Dropdown", "File\rClean-Up");
            PulldownButtonData pdbd5 = new PulldownButtonData("Sheet Tools", "  Sheet  \r  Tools  ");
            PulldownButtonData pdbd6 = new PulldownButtonData("CapitalizationTools", "Capitalization\rTools");
            PulldownButtonData pdbd7 = new PulldownButtonData("Beta Tools", "   Beta   \r   Tools   ");


            //Images
            pbd1.LargeImage = BitMapToImageSource(Resources.aA_32x32);
            pbd1.Image = BitMapToImageSource(Resources.aA_16x16);
            pbd2.LargeImage = BitMapToImageSource(Resources.Teams_icon_32x32);
            pbd2.Image = BitMapToImageSource(Resources.Teams_Icon_16x16);
            pbd3.LargeImage = BitMapToImageSource(Resources.Dimension_32x32);
            pbd3.Image = BitMapToImageSource(Resources.Dimension_16x16);
            pbd4.LargeImage = BitMapToImageSource(Resources.Element_History_32x32);
            pbd4.Image = BitMapToImageSource(Resources.Element_History_16x16);
            pbd5.LargeImage = BitMapToImageSource(Resources.Align_Notes_32x32);
            pbd5.Image = BitMapToImageSource(Resources.Align_Notes_16x16);
            pbd6.LargeImage = BitMapToImageSource(Resources.Renumber_Views_32x32);
            pbd6.Image = BitMapToImageSource(Resources.Renumber_Views_16x16);
            pbd7.LargeImage = BitMapToImageSource(Resources.Align_Notes_32x32);
            pbd7.Image = BitMapToImageSource(Resources.Align_Notes_16x16);
            pbd8.LargeImage = BitMapToImageSource(Resources.Align_Sheets_32x32);
            pbd8.Image = BitMapToImageSource(Resources.Align_Sheets_16x16);
            pbd9.LargeImage = BitMapToImageSource(Resources.Align_Sheets_32x32);
            pbd9.Image = BitMapToImageSource(Resources.Align_Sheets_16x16);
            pbd10.LargeImage = BitMapToImageSource(Resources.Stack_Sheets_32x32);
            pbd10.Image = BitMapToImageSource(Resources.Stack_Sheets_16x16);

            pbd11.LargeImage = BitMapToImageSource(Resources.Create_Interior_Elevations_32x32);
            pbd11.Image = BitMapToImageSource(Resources.Create_Interior_Elevations_16x16);
            pbd12.LargeImage = BitMapToImageSource(Resources.Interior_Elevations_On_Sheets_32x32);
            pbd12.Image = BitMapToImageSource(Resources.Interior_Elevations_On_Sheets_16x16);
            pbd13.LargeImage = BitMapToImageSource(Resources.Tilt_Up_Elevations_32x32);
            pbd13.Image = BitMapToImageSource(Resources.Tilt_Up_Elevations_16x16);
            pbd14.LargeImage = BitMapToImageSource(Resources.Tilt_Up_Elevations_32x32);
            pbd14.Image = BitMapToImageSource(Resources.Tilt_Up_Elevations_16x16);
            pbd15.LargeImage = BitMapToImageSource(Resources.Thicken_Elevations_Crop_32x32);
            pbd15.Image = BitMapToImageSource(Resources.Thicken_Elevations_Crop_16x16);
            pbd16.LargeImage = BitMapToImageSource(Resources.Purge_Unplaced_Rooms_32x32);
            pbd16.Image = BitMapToImageSource(Resources.Purge_Unplaced_Rooms_16x16);
            pbd17.LargeImage = BitMapToImageSource(Resources.Renumber_Sheets_32x32);
            pbd17.Image = BitMapToImageSource(Resources.Renumber_Sheets_16x16);
            pbd18.LargeImage = BitMapToImageSource(Resources.Place_Views_On_Sheets_32x32);
            pbd18.Image = BitMapToImageSource(Resources.Place_Views_On_Sheets_16x16);
            pbd19.LargeImage = BitMapToImageSource(Resources.Re_Crop_Elevation_32x32);
            pbd19.Image = BitMapToImageSource(Resources.Re_Crop_Elevation_16x16);
            pbd20.LargeImage = BitMapToImageSource(Resources.aA_32x32);
            pbd20.Image = BitMapToImageSource(Resources.aA_16x16);

            pdbd1.LargeImage = BitMapToImageSource(Resources.Align_Notes_32x32);
            pdbd1.Image = BitMapToImageSource(Resources.Align_Notes_16x16);
            pdbd2.LargeImage = BitMapToImageSource(Resources.Place_Views_On_Sheets_32x32);
            pdbd2.Image = BitMapToImageSource(Resources.Place_Views_On_Sheets_16x16);
            pdbd3.LargeImage = BitMapToImageSource(Resources.Tilt_Up_Elevations_32x32);
            pdbd3.Image = BitMapToImageSource(Resources.Tilt_Up_Elevations_16x16);
            pdbd4.LargeImage = BitMapToImageSource(Resources.Model_Clean_Up_32x32);
            pdbd4.Image = BitMapToImageSource(Resources.Model_Clean_Up_16x16);
            pdbd5.LargeImage = BitMapToImageSource(Resources.Sheets_32x32);
            pdbd5.Image = BitMapToImageSource(Resources.Sheets_16x16);
            pdbd6.LargeImage = BitMapToImageSource(Resources.aA_32x32);
            pdbd6.Image = BitMapToImageSource(Resources.aA_16x16);
            pdbd7.LargeImage = BitMapToImageSource(Resources.Beta_Tools_32x32);
            pdbd7.Image = BitMapToImageSource(Resources.Beta_Tools_16x16);

            //ToolTips
            pbd1.ToolTip = "Changes all sheet names to uppercase.";
            pbd2.ToolTip = "Opens up the \"BIM Tools & Resources\" page on the Microsoft Teams Application.";
            pbd3.ToolTip = "Selects all overridden dimensions in an active view.";
            pbd4.ToolTip = "Shows who created an element & who last changed it.";
            pbd5.ToolTip = "Aligns Text Notes and Keynotes to a detail line. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd6.ToolTip = "Renumbers all views on sheets based on the \"NWL_30x42\" title block.";
            pbd7.ToolTip = "Aligns Text Notes and Keynotes to a detail line & resize texts notes to match keynote width. \nAt least one detail line and one note need to be selected. Deletes detail line When Completete.";
            pbd8.ToolTip = "Takes all sheets and sets their origin back to (0,0,0).";
            pbd9.ToolTip = "Sets current sheet's origin back to (0,0,0).";
            pbd10.ToolTip = "Aligns plans accross multiple sheets. Select the plan to follow and then select the sheets you want to affect.";

            pbd11.ToolTip = "Creates interior elevations for all rooms in the project.";
            pbd12.ToolTip = "Places interior elevations on sheets based on rooms selected.";
            pbd13.ToolTip = "Creates Tilt-Up Elevations based on the exterior side of the wall.";
            pbd14.ToolTip = "Creates Tilt-Up Elevations based on a selection of walls.";
            pbd15.ToolTip = "Thickens the Crop Region of selected elevation type.";
            pbd16.ToolTip = "Purges un-placed rooms from the project.";
            pbd17.ToolTip = "Renumbers selected sheets.";
            pbd18.ToolTip = "Places Selected Views on sheets.";
            pbd19.ToolTip = "Re Crops Interior Elevation. Make sure the Interior Elevation to be affected is the active view.";
            pbd20.ToolTip = "Capitalizes room names";


            pdbd1.ToolTip = "Aligns Text Notes and Keynotes to a detail line.";
            pdbd2.ToolTip = "Places views on new sheets.";
            pdbd3.ToolTip = "Creates Tilt up elevations by wall type or by wall selection.";
            pdbd4.ToolTip = "Varying tools to assist in cleaning the file";
            pdbd5.ToolTip = "Tools to assist with Sheet information and Sheet content.";
            pdbd6.ToolTip = "Tools to assist with capitalization accross the project.";
            pdbd7.ToolTip = "Tools that are currently being worked on, or do not work as intended. Only use when directed by a BIM Coordinator.";

            //Align Notes Dropdown
            IList<PushButtonData> alignNotesList = new List<PushButtonData>();
            alignNotesList.Add(pbd5);
            alignNotesList.Add(pbd7);

            //Sheet Tools Dropdown
            IList<PushButtonData> SheetToolsList = new List<PushButtonData>();
            SheetToolsList.Add(pbd6);
            SheetToolsList.Add(pbd17);
            SheetToolsList.Add(pbd9); //align sheets

            //Tilt Elevations Dropdown
            IList<PushButtonData> tiltList = new List<PushButtonData>();
            tiltList.Add(pbd13);
            tiltList.Add(pbd14);

            //Clean-Up Tools Dropdown
            IList<PushButtonData> cleanUpList = new List<PushButtonData>();
            cleanUpList.Add(pbd16);

            //Capitilazation Tools
            IList<PushButtonData> capitalizationList = new List<PushButtonData>();
            capitalizationList.Add(pbd1);
            capitalizationList.Add(pbd20);

            //Beta Tools
            IList<PushButtonData> betaList = new List<PushButtonData>();
            //betaList.Add(pbd8); //align all sheets            
            betaList.Add(pbd10); //align plans

            //place Views Dropdown
            IList<PushButtonData> placeViewsList = new List<PushButtonData>();
            placeViewsList.Add(pbd18); //Place Views on Sheets
            placeViewsList.Add(pbd12);
            

            //Tools Section

            //Documentation Section
            PulldownButton pdb1 = (PulldownButton)documentationPanel.AddItem(pdbd1); //Align Notes
                foreach (PushButtonData pbd in alignNotesList)
                    pdb1.AddPushButton(pbd);

            documentationPanel.AddSeparator();

            PulldownButton pdb5 = (PulldownButton)documentationPanel.AddItem(pdbd5); //Sheet Tools
                foreach (PushButtonData pbd in SheetToolsList)
                    pdb5.AddPushButton(pbd);
            PulldownButton pdb2 = (PulldownButton)documentationPanel.AddItem(pdbd2); //Place Views on Sheets
                foreach (PushButtonData pbd in placeViewsList)
                    pdb2.AddPushButton(pbd);

            documentationPanel.AddSeparator();

            PushButton pb11 = (PushButton)documentationPanel.AddItem(pbd11);         //Create Int Elevations            
            PulldownButton pdb3 = (PulldownButton)documentationPanel.AddItem(pdbd3); //Tilt-Up Elevations
                foreach (PushButtonData pbd in tiltList)
                    pdb3.AddPushButton(pbd);
            PushButton pb15 = (PushButton)documentationPanel.AddItem(pbd15);         //Thicken Elevation Crop            
            PushButton pb19 = (PushButton)documentationPanel.AddItem(pbd19);         //ReCrop Elevations

            documentationPanel.AddSeparator();

            PulldownButton pdb6 = (PulldownButton)documentationPanel.AddItem(pdbd6); //Capitalization Tools
            foreach (PushButtonData pbd in capitalizationList)
                pdb6.AddPushButton(pbd);            

            //Dimensions Section
            PushButton pb3 = (PushButton)dimensionsPanel.AddItem(pbd3);              //Overridden Dimensions

            //Resources Section
            PulldownButton pdb4 = (PulldownButton)resourcesPanel.AddItem(pdbd4);     //File Cleanup
                foreach (PushButtonData pbd in cleanUpList)
                    pdb4.AddPushButton(pbd);
            PushButton pb4 = (PushButton)resourcesPanel.AddItem(pbd4);               //Element History
            PushButton pb2 = (PushButton)resourcesPanel.AddItem(pbd2);               //BIM Tools & Resources

            //Beta Section
            PulldownButton pdb7 = (PulldownButton)betaPanel.AddItem(pdbd7);          //Beta Tools
            foreach (PushButtonData pbd in betaList)
                pdb7.AddPushButton(pbd);

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
