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
            RibbonPanel curPanel1 = application.CreateRibbonPanel(tabName1, "Tools");            
            RibbonPanel curPanel3 = application.CreateRibbonPanel(tabName1, "Dimensions");
            RibbonPanel curPanel2 = application.CreateRibbonPanel(tabName1, "Resources");

            string curAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string curAssemblyPath = System.IO.Path.GetDirectoryName(curAssembly);

            //Tools
            PushButtonData pbd1 = new PushButtonData("Sheets to Uppercase", "Sheets Names" + "\r" + "To Uppercase", curAssembly, "NWLToolbar.CapitalizeSheets");
            PushButtonData pbd2 = new PushButtonData("Teams Link", "BIM Tools" + "\r" + "& Resources", curAssembly, "NWLToolbar.TeamsLink");
            PushButtonData pbd3 = new PushButtonData("Overridden Dims", "Select Overridden" + "\r" + "Dimensions", curAssembly, "NWLToolbar.SelectOverriddenDimensions");

            //Images
            pbd1.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "aA.png")));
            pbd2.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "teams.png")));
            pbd3.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(curAssemblyPath, "overridden dimensions.png")));

            //IList<PushButtonData> list = new List<PushButtonData>();
            //list.Add(pbd3);

            //Tools Section
            PushButton pb1 = (PushButton)curPanel1.AddItem(pbd1);            
            
            //Dimensions Section
            PushButton pb3 = (PushButton)curPanel3.AddItem(pbd3);

            //Resources Section
            PushButton pb2 = (PushButton)curPanel2.AddItem(pbd2);


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
