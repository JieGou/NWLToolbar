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

namespace NWLToolbar
{
    public partial class FrmRenumberSheets : System.Windows.Forms.Form
    {
        public FrmRenumberSheets()
        {
            InitializeComponent();
            this.SheetLetter.Text = "A";
            this.SheetNumber.Value = 100;
            this.AppendedLetter.Enabled = false;
            this.AppendedLetter.Text = "a";

        }

        private void FrmRenumberSheets_Load(object sender, EventArgs e)
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
        
        private void NWLWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("https://teams.microsoft.com/l/entity/0ae35b36-0fd7-422e-805b-d53af1579093/_djb2_msteams_prefix_3324205331?context=%7B%22subEntityId%22%3Anull%2C%22channelId%22%3A%2219%3A8975880ee31c4076978f866de88e034b%40thread.tacv2%22%7D&groupId=c3062a34-c7ff-42a1-82e5-0bcc6c312e80&tenantId=64aa4397-6684-4bc6-a7bd-5a96acdb256a&allowXTenantAccess=false");
        }
        public string GetSheetLetter()
        {     
            return this.SheetLetter.Text;
        }
        public int GetSheetNumber()
        {
            return Convert.ToInt32(this.SheetNumber.Value);
        }
        public string GetSheetAppend()
        {
            return this.AppendedLetter.Text;
        }
        public bool GetSheetAppendBool()
        {
            return this.AppendLetter.Checked;
        }

        private void AppendLetter_CheckedChanged(object sender, EventArgs e)
        {
            if (this.AppendLetter.Checked)
                this.AppendedLetter.Enabled = true;
            else
                this.AppendedLetter.Enabled = false;
        }       
    }
}
