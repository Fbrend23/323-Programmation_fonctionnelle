using System.Drawing;
using System.Windows.Forms;

namespace KochSnowflake
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Panel drawingPanel;
        private NumericUpDown numDepth;
        private CheckBox chkAntialias;
        private Label lblDepth;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing); // ← nécessite que Form1 hérite bien de Form
        }

        private void InitializeComponent()
        {
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.numDepth = new System.Windows.Forms.NumericUpDown();
            this.chkAntialias = new System.Windows.Forms.CheckBox();
            this.lblDepth = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numDepth)).BeginInit();
            this.SuspendLayout();
            // 
            // drawingPanel
            // 
            this.drawingPanel.BackColor = System.Drawing.Color.White;
            this.drawingPanel.Location = new System.Drawing.Point(64, 41);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(939, 630);
            this.drawingPanel.TabIndex = 0;
            this.drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.drawingPanel_Paint);
            // 
            // numDepth
            // 
            this.numDepth.Location = new System.Drawing.Point(100, 12);
            this.numDepth.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numDepth.Name = "numDepth";
            this.numDepth.Size = new System.Drawing.Size(60, 23);
            this.numDepth.TabIndex = 1;
            this.numDepth.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numDepth.ValueChanged += new System.EventHandler(this.numDepth_ValueChanged);
            // 
            // chkAntialias
            // 
            this.chkAntialias.AutoSize = true;
            this.chkAntialias.Checked = true;
            this.chkAntialias.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAntialias.Location = new System.Drawing.Point(180, 13);
            this.chkAntialias.Name = "chkAntialias";
            this.chkAntialias.Size = new System.Drawing.Size(88, 19);
            this.chkAntialias.TabIndex = 2;
            this.chkAntialias.Text = "Antialiasing";
            this.chkAntialias.UseVisualStyleBackColor = true;
            this.chkAntialias.CheckedChanged += new System.EventHandler(this.chkAntialias_CheckedChanged);
            // 
            // lblDepth
            // 
            this.lblDepth.AutoSize = true;
            this.lblDepth.Location = new System.Drawing.Point(10, 14);
            this.lblDepth.Name = "lblDepth";
            this.lblDepth.Size = new System.Drawing.Size(73, 15);
            this.lblDepth.TabIndex = 3;
            this.lblDepth.Text = "Profondeur :";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 761);
            this.Controls.Add(this.lblDepth);
            this.Controls.Add(this.chkAntialias);
            this.Controls.Add(this.numDepth);
            this.Controls.Add(this.drawingPanel);
            this.Name = "Form1";
            this.Text = "Flocon de Koch — récursivité";
            ((System.ComponentModel.ISupportInitialize)(this.numDepth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
