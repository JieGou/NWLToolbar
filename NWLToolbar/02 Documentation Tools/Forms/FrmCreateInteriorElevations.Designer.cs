﻿namespace NWLToolbar
{
    partial class FrmCreateInteriorElevations
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCreateInteriorElevations));
            this.label1 = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.RoomList = new System.Windows.Forms.CheckedListBox();
            this.NWLWebsite = new System.Windows.Forms.Button();
            this.SelectAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Rooms:";
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.BackColor = System.Drawing.SystemColors.Window;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Cancel.Location = new System.Drawing.Point(565, 800);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(100, 40);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = false;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.BackColor = System.Drawing.SystemColors.Window;
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.OK.FlatAppearance.BorderSize = 3;
            this.OK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OK.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OK.Location = new System.Drawing.Point(420, 800);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(140, 40);
            this.OK.TabIndex = 2;
            this.OK.Text = "Select Rooms";
            this.OK.UseVisualStyleBackColor = false;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // RoomList
            // 
            this.RoomList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoomList.BackColor = System.Drawing.SystemColors.Window;
            this.RoomList.CheckOnClick = true;
            this.RoomList.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RoomList.FormattingEnabled = true;
            this.RoomList.Location = new System.Drawing.Point(15, 39);
            this.RoomList.Margin = new System.Windows.Forms.Padding(30);
            this.RoomList.Name = "RoomList";
            this.RoomList.Size = new System.Drawing.Size(650, 744);
            this.RoomList.TabIndex = 1;
            this.RoomList.SelectedIndexChanged += new System.EventHandler(this.Sheets_SelectedIndexChanged);
            // 
            // NWLWebsite
            // 
            this.NWLWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NWLWebsite.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NWLWebsite.BackgroundImage")));
            this.NWLWebsite.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.NWLWebsite.Location = new System.Drawing.Point(15, 800);
            this.NWLWebsite.Name = "NWLWebsite";
            this.NWLWebsite.Size = new System.Drawing.Size(40, 40);
            this.NWLWebsite.TabIndex = 3;
            this.NWLWebsite.UseVisualStyleBackColor = true;
            this.NWLWebsite.Click += new System.EventHandler(this.NWLWebsite_Click);
            // 
            // SelectAll
            // 
            this.SelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectAll.BackColor = System.Drawing.SystemColors.Window;
            this.SelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectAll.Location = new System.Drawing.Point(274, 800);
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Size = new System.Drawing.Size(140, 40);
            this.SelectAll.TabIndex = 4;
            this.SelectAll.Text = "Select All";
            this.SelectAll.UseVisualStyleBackColor = false;
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // FrmCreateInteriorElevations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(684, 861);
            this.Controls.Add(this.SelectAll);
            this.Controls.Add(this.NWLWebsite);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.RoomList);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 450);
            this.Name = "FrmCreateInteriorElevations";
            this.Text = "Create Interior Elevations";
            this.Load += new System.EventHandler(this.FrmAlignPlans_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.CheckedListBox RoomList;
        private System.Windows.Forms.Button NWLWebsite;
        private System.Windows.Forms.Button SelectAll;
    }
}