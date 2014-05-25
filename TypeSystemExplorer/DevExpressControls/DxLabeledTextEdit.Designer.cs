namespace TypeSystemExplorer.DevExpressControls
{
	partial class DxLabeledTextEdit
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblTextBox = new System.Windows.Forms.Label();
			this.tbData = new DevExpress.XtraEditors.TextEdit();
			this.SuspendLayout();
			// 
			// lblTextBox
			// 
			this.lblTextBox.Location = new System.Drawing.Point(5, 3);
			this.lblTextBox.Name = "lblTextBox";
			this.lblTextBox.Size = new System.Drawing.Size(78, 23);
			this.lblTextBox.TabIndex = 0;
			this.lblTextBox.Text = "label1";
			this.lblTextBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// tbData
			// 
			this.tbData.Location = new System.Drawing.Point(40, 0);
			this.tbData.Name = "tbData";
			this.tbData.Size = new System.Drawing.Size(200, 20);
			this.tbData.TabIndex = 1;
			// 
			// LabeledTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tbData);
			this.Controls.Add(this.lblTextBox);
			this.Name = "LabeledTextBox";
			this.Size = new System.Drawing.Size(245, 20);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTextBox;
		private DevExpress.XtraEditors.TextEdit tbData;
	}
}
