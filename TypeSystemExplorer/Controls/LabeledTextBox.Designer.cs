/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

namespace TypeSystemExplorer.Controls
{
	partial class LabeledTextBox
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
			this.tbData = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lblTextBox
			// 
			this.lblTextBox.Location = new System.Drawing.Point(5, 3);
			this.lblTextBox.Name = "lblTextBox";
			this.lblTextBox.Size = new System.Drawing.Size(78, 23);
			this.lblTextBox.TabIndex = 0;
			this.lblTextBox.Text = "label1";
			this.lblTextBox.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbData
			// 
			this.tbData.Location = new System.Drawing.Point(89, 0);
			this.tbData.Name = "tbData";
			this.tbData.Size = new System.Drawing.Size(245, 20);
			this.tbData.TabIndex = 1;
			// 
			// LabeledTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tbData);
			this.Controls.Add(this.lblTextBox);
			this.Name = "LabeledTextBox";
			this.Size = new System.Drawing.Size(337, 20);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTextBox;
		private System.Windows.Forms.TextBox tbData;
	}
}
