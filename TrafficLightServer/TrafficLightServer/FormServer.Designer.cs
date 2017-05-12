namespace TrafficLightServer
{
    partial class FormServer
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
            this.buttonConnect = new System.Windows.Forms.Button();
            this.listBoxOutput = new System.Windows.Forms.ListBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxLightIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(35, 10);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(176, 31);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect to sort of proxy";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // listBoxOutput
            // 
            this.listBoxOutput.FormattingEnabled = true;
            this.listBoxOutput.Location = new System.Drawing.Point(35, 148);
            this.listBoxOutput.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxOutput.Name = "listBoxOutput";
            this.listBoxOutput.Size = new System.Drawing.Size(315, 251);
            this.listBoxOutput.TabIndex = 1;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(231, 17);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(50, 18);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "Status";
            // 
            // textBoxLightIP
            // 
            this.textBoxLightIP.Location = new System.Drawing.Point(113, 76);
            this.textBoxLightIP.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxLightIP.Name = "textBoxLightIP";
            this.textBoxLightIP.Size = new System.Drawing.Size(88, 20);
            this.textBoxLightIP.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 79);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP number of light";
            // 
            // FormServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 442);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxLightIP);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listBoxOutput);
            this.Controls.Add(this.buttonConnect);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormServer";
            this.Text = "Server (sort of)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormServer_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ListBox listBoxOutput;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxLightIP;
        private System.Windows.Forms.Label label1;
    }
}

