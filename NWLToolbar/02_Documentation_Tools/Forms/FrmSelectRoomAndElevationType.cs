using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using NWLToolbar.Utils;

namespace NWLToolbar
{
    public partial class FrmSelectRoomAndElevationType : System.Windows.Forms.Form
    {
        private List<Room> formList;
        private List<ViewFamilyType> viewFamilyList;
        public FrmSelectRoomAndElevationType(List<Room> room, List<ViewFamilyType> vftList)
        {
            InitializeComponent();

            formList = room;
            viewFamilyList = vftList;

            foreach (Room r in room)
            {
                this.RoomList.Items.Add(r.GetNumName());
            }
            foreach (ViewFamilyType vft in vftList)
            {
                if (vft.FamilyName == "Elevation" && (vft.Name.Contains("Int") || vft.Name.Contains("INT")))
                    this.comboBox1.Items.Add(vft.GetName());
            }
            this.comboBox1.SelectedIndex = 0;
        }

        private void FrmAlignPlans_Load(object sender, EventArgs e)
        {

        }

        private void Sheets_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public List<Room> GetSelectedRooms()
        {
            Dictionary<string, Room> roomDictionary = formList.ToDictionary(x => x.GetNumName());

            List<Room> selectedRooms = new List<Room>();

            foreach (string s in this.RoomList.CheckedItems)
                selectedRooms.Add(roomDictionary[s]);
                
            return selectedRooms;
        }
        public ViewFamilyType GetSelectedElevationType()
        {
            Dictionary<string, ViewFamilyType> vftDictionary = viewFamilyList.ToDictionary(x => x.GetName());
            
            return vftDictionary[this.comboBox1.SelectedItem.ToString()];
        }

        private void NWLWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("https://teams.microsoft.com/l/entity/0ae35b36-0fd7-422e-805b-d53af1579093/_djb2_msteams_prefix_3324205331?context=%7B%22subEntityId%22%3Anull%2C%22channelId%22%3A%2219%3A8975880ee31c4076978f866de88e034b%40thread.tacv2%22%7D&groupId=c3062a34-c7ff-42a1-82e5-0bcc6c312e80&tenantId=64aa4397-6684-4bc6-a7bd-5a96acdb256a&allowXTenantAccess=false");
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {            
            for (int i = 0; i < this.RoomList.Items.Count; i++)
            {
                this.RoomList.SetItemChecked(i,true);
            }
        }        
    }
}
