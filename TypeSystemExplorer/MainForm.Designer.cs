namespace TypeSystemExplorer
{
	partial class MainForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnBuildTypeDefinitions = new DevExpress.XtraEditors.SimpleButton();
			this.tbTypeDefinition = new DevExpress.XtraEditors.MemoEdit();
			this.lbRegisteredTypes = new DevExpress.XtraEditors.ListBoxControl();
			((System.ComponentModel.ISupportInitialize)(this.tbTypeDefinition.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lbRegisteredTypes)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Type Definition:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(328, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(93, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Registered Types:";
			// 
			// btnBuildTypeDefinitions
			// 
			this.btnBuildTypeDefinitions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnBuildTypeDefinitions.Location = new System.Drawing.Point(19, 382);
			this.btnBuildTypeDefinitions.Name = "btnBuildTypeDefinitions";
			this.btnBuildTypeDefinitions.Size = new System.Drawing.Size(75, 23);
			this.btnBuildTypeDefinitions.TabIndex = 2;
			this.btnBuildTypeDefinitions.Text = "Register";
			this.btnBuildTypeDefinitions.Click += new System.EventHandler(this.btnBuildTypeDefinitions_Click);
			// 
			// tbTypeDefinition
			// 
			this.tbTypeDefinition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.tbTypeDefinition.Location = new System.Drawing.Point(19, 30);
			this.tbTypeDefinition.Name = "tbTypeDefinition";
			this.tbTypeDefinition.Properties.Appearance.Options.UseTextOptions = true;
			this.tbTypeDefinition.Properties.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
			this.tbTypeDefinition.Properties.AppearanceFocused.Options.UseTextOptions = true;
			this.tbTypeDefinition.Properties.AppearanceFocused.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
			this.tbTypeDefinition.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbTypeDefinition.Properties.WordWrap = false;
			this.tbTypeDefinition.Size = new System.Drawing.Size(289, 346);
			this.tbTypeDefinition.TabIndex = 1;
			// 
			// lbRegisteredTypes
			// 
			this.lbRegisteredTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbRegisteredTypes.Location = new System.Drawing.Point(331, 32);
			this.lbRegisteredTypes.Name = "lbRegisteredTypes";
			this.lbRegisteredTypes.Size = new System.Drawing.Size(220, 173);
			this.lbRegisteredTypes.TabIndex = 4;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(563, 417);
			this.Controls.Add(this.lbRegisteredTypes);
			this.Controls.Add(this.tbTypeDefinition);
			this.Controls.Add(this.btnBuildTypeDefinitions);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "MainForm";
			this.Text = "Type System Explorer";
			((System.ComponentModel.ISupportInitialize)(this.tbTypeDefinition.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lbRegisteredTypes)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private DevExpress.XtraEditors.SimpleButton btnBuildTypeDefinitions;
		private DevExpress.XtraEditors.MemoEdit tbTypeDefinition;
		private DevExpress.XtraEditors.ListBoxControl lbRegisteredTypes;
	}
}

