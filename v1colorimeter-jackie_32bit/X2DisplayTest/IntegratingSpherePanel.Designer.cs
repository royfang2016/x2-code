namespace X2DisplayTest
{
    partial class IntegratingSpherePanel
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.cbRemote = new System.Windows.Forms.CheckBox();
            this.cbOutputStatus = new System.Windows.Forms.CheckBox();
            this.btnSetSign = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbCurrent = new System.Windows.Forms.TextBox();
            this.tbVoltage = new System.Windows.Forms.TextBox();
            this.btnGetValue = new System.Windows.Forms.Button();
            this.lbCurrent = new System.Windows.Forms.Label();
            this.lbVoltage = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbPort = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbRemote
            // 
            this.cbRemote.AutoSize = true;
            this.cbRemote.Location = new System.Drawing.Point(81, 99);
            this.cbRemote.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbRemote.Name = "cbRemote";
            this.cbRemote.Size = new System.Drawing.Size(90, 16);
            this.cbRemote.TabIndex = 0;
            this.cbRemote.Text = "Open Remote";
            this.cbRemote.UseVisualStyleBackColor = true;
            // 
            // cbOutputStatus
            // 
            this.cbOutputStatus.AutoSize = true;
            this.cbOutputStatus.Location = new System.Drawing.Point(81, 132);
            this.cbOutputStatus.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbOutputStatus.Name = "cbOutputStatus";
            this.cbOutputStatus.Size = new System.Drawing.Size(102, 16);
            this.cbOutputStatus.TabIndex = 1;
            this.cbOutputStatus.Text = "Output Status";
            this.cbOutputStatus.UseVisualStyleBackColor = true;
            // 
            // btnSetSign
            // 
            this.btnSetSign.Location = new System.Drawing.Point(274, 179);
            this.btnSetSign.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSetSign.Name = "btnSetSign";
            this.btnSetSign.Size = new System.Drawing.Size(88, 32);
            this.btnSetSign.TabIndex = 2;
            this.btnSetSign.Text = "Set value";
            this.btnSetSign.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 171);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "current";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(169, 171);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "voltage";
            // 
            // tbCurrent
            // 
            this.tbCurrent.Location = new System.Drawing.Point(83, 188);
            this.tbCurrent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbCurrent.Name = "tbCurrent";
            this.tbCurrent.Size = new System.Drawing.Size(52, 21);
            this.tbCurrent.TabIndex = 5;
            // 
            // tbVoltage
            // 
            this.tbVoltage.Location = new System.Drawing.Point(169, 188);
            this.tbVoltage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbVoltage.Name = "tbVoltage";
            this.tbVoltage.Size = new System.Drawing.Size(52, 21);
            this.tbVoltage.TabIndex = 5;
            // 
            // btnGetValue
            // 
            this.btnGetValue.Location = new System.Drawing.Point(274, 247);
            this.btnGetValue.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnGetValue.Name = "btnGetValue";
            this.btnGetValue.Size = new System.Drawing.Size(88, 33);
            this.btnGetValue.TabIndex = 6;
            this.btnGetValue.Text = "Get value";
            this.btnGetValue.UseVisualStyleBackColor = true;
            this.btnGetValue.Click += new System.EventHandler(this.btnGetValue_Click);
            // 
            // lbCurrent
            // 
            this.lbCurrent.Location = new System.Drawing.Point(81, 257);
            this.lbCurrent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbCurrent.Name = "lbCurrent";
            this.lbCurrent.Size = new System.Drawing.Size(52, 10);
            this.lbCurrent.TabIndex = 7;
            this.lbCurrent.Text = "----";
            this.lbCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbVoltage
            // 
            this.lbVoltage.Location = new System.Drawing.Point(169, 257);
            this.lbVoltage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbVoltage.Name = "lbVoltage";
            this.lbVoltage.Size = new System.Drawing.Size(52, 10);
            this.lbVoltage.TabIndex = 8;
            this.lbVoltage.Text = "----";
            this.lbVoltage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(79, 61);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "port name:";
            // 
            // cbPort
            // 
            this.cbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPort.FormattingEnabled = true;
            this.cbPort.Location = new System.Drawing.Point(147, 60);
            this.cbPort.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbPort.Name = "cbPort";
            this.cbPort.Size = new System.Drawing.Size(169, 20);
            this.cbPort.TabIndex = 10;
            this.cbPort.SelectedIndexChanged += new System.EventHandler(this.cbPort_SelectedIndexChanged);
            // 
            // DCPower3005Panel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SkyBlue;
            this.Controls.Add(this.cbPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbVoltage);
            this.Controls.Add(this.lbCurrent);
            this.Controls.Add(this.btnGetValue);
            this.Controls.Add(this.tbVoltage);
            this.Controls.Add(this.tbCurrent);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSetSign);
            this.Controls.Add(this.cbOutputStatus);
            this.Controls.Add(this.cbRemote);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DCPower3005Panel";
            this.Size = new System.Drawing.Size(450, 374);
            this.Load += new System.EventHandler(this.DCPower3005Panel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbRemote;
        private System.Windows.Forms.CheckBox cbOutputStatus;
        private System.Windows.Forms.Button btnSetSign;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbCurrent;
        private System.Windows.Forms.TextBox tbVoltage;
        private System.Windows.Forms.Button btnGetValue;
        private System.Windows.Forms.Label lbCurrent;
        private System.Windows.Forms.Label lbVoltage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbPort;
    }
}
