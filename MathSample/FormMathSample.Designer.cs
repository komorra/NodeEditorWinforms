namespace MathSample
{
    partial class FormMathSample
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
            this.controlNodeEditor = new SampleCommon.ControlNodeEditor();
            this.SuspendLayout();
            // 
            // controlNodeEditor
            // 
            this.controlNodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlNodeEditor.Location = new System.Drawing.Point(0, 0);
            this.controlNodeEditor.Name = "controlNodeEditor";
            this.controlNodeEditor.Size = new System.Drawing.Size(957, 510);
            this.controlNodeEditor.TabIndex = 0;
            // 
            // FormMathSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(957, 510);
            this.Controls.Add(this.controlNodeEditor);
            this.Name = "FormMathSample";
            this.Text = "NodeEditor WinForms - Math Sample";
            this.Load += new System.EventHandler(this.FormMathSample_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private SampleCommon.ControlNodeEditor controlNodeEditor;
    }
}

