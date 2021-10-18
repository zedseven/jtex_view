using System.Drawing.Drawing2D;

namespace Jtex
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
	            components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
	        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
	        this.displayBox = new InterpolationPictureBox();
	        this.autoSaveInput = new System.Windows.Forms.CheckBox();
	        ((System.ComponentModel.ISupportInitialize) (this.displayBox)).BeginInit();
	        this.SuspendLayout();
	        //
	        // pictureBox1
	        //
	        this.displayBox.InterpolationMode = InterpolationMode.NearestNeighbor;
	        this.displayBox.Location = new System.Drawing.Point(12, 35);
	        this.displayBox.Name = "displayBox";
	        this.displayBox.Size = new System.Drawing.Size(260, 260);
	        this.displayBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
	        this.displayBox.TabIndex = 0;
	        this.displayBox.TabStop = false;
	        //
	        // checkBox1
	        //
	        this.autoSaveInput.AutoSize = true;
	        this.autoSaveInput.Location = new System.Drawing.Point(12, 12);
	        this.autoSaveInput.Name = "autoSaveInput";
	        this.autoSaveInput.Size = new System.Drawing.Size(76, 17);
	        this.autoSaveInput.TabIndex = 1;
	        this.autoSaveInput.Text = "Auto Save";
	        this.autoSaveInput.UseVisualStyleBackColor = true;
	        //
	        // MainForm
	        //
	        this.AllowDrop = true;
	        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	        this.ClientSize = new System.Drawing.Size(284, 306);
	        this.Controls.Add(this.autoSaveInput);
	        this.Controls.Add(this.displayBox);
	        this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
	        this.Name = "MainForm";
	        this.Text = "jtex";
	        this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
	        this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
	        ((System.ComponentModel.ISupportInitialize) (this.displayBox)).EndInit();
	        this.ResumeLayout(false);
	        this.PerformLayout();
        }

        #endregion

        private InterpolationPictureBox displayBox;
        private System.Windows.Forms.CheckBox autoSaveInput;
    }
}

