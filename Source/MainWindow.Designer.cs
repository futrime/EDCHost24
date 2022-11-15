namespace EdcHost;
partial class MainWindow
{
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
            Camera.Dispose();
            this._coordinateConverter.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.pictureBoxMonitor = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.labelScoreVehicleA = new System.Windows.Forms.Label();
            this.labelScoreVehicleB = new System.Windows.Forms.Label();
            this.buttonCalibration = new System.Windows.Forms.Button();
            this.buttonFoul = new System.Windows.Forms.Button();
            this.buttonEnd = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelGameTime = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBarRemainingPowerRatio = new System.Windows.Forms.ProgressBar();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelGameHalf = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMonitor)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxMonitor
            // 
            this.pictureBoxMonitor.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBoxMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMonitor.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxMonitor.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxMonitor.Name = "pictureBoxMonitor";
            this.pictureBoxMonitor.Size = new System.Drawing.Size(913, 661);
            this.pictureBoxMonitor.TabIndex = 0;
            this.pictureBoxMonitor.TabStop = false;
            this.pictureBoxMonitor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMonitor_MouseClick);
            this.pictureBoxMonitor.Resize += new System.EventHandler(this.pictureBoxMonitor_Resize);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // buttonStart
            // 
            this.buttonStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStart.Location = new System.Drawing.Point(165, 10);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(160, 40);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonPause.Enabled = false;
            this.buttonPause.Location = new System.Drawing.Point(165, 53);
            this.buttonPause.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(160, 40);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonContinue
            // 
            this.buttonContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonContinue.Enabled = false;
            this.buttonContinue.Location = new System.Drawing.Point(165, 96);
            this.buttonContinue.Margin = new System.Windows.Forms.Padding(2);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(160, 40);
            this.buttonContinue.TabIndex = 3;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // labelScoreVehicleA
            // 
            this.labelScoreVehicleA.AutoSize = true;
            this.labelScoreVehicleA.BackColor = System.Drawing.Color.Red;
            this.labelScoreVehicleA.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelScoreVehicleA.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelScoreVehicleA.Location = new System.Drawing.Point(0, 0);
            this.labelScoreVehicleA.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelScoreVehicleA.Name = "labelScoreVehicleA";
            this.labelScoreVehicleA.Size = new System.Drawing.Size(428, 65);
            this.labelScoreVehicleA.TabIndex = 8;
            this.labelScoreVehicleA.Text = "labelScoreVehicleA";
            // 
            // labelScoreVehicleB
            // 
            this.labelScoreVehicleB.AutoSize = true;
            this.labelScoreVehicleB.BackColor = System.Drawing.Color.Blue;
            this.labelScoreVehicleB.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelScoreVehicleB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelScoreVehicleB.Location = new System.Drawing.Point(0, 65);
            this.labelScoreVehicleB.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelScoreVehicleB.Name = "labelScoreVehicleB";
            this.labelScoreVehicleB.Size = new System.Drawing.Size(425, 65);
            this.labelScoreVehicleB.TabIndex = 10;
            this.labelScoreVehicleB.Text = "labelScoreVehicleB";
            // 
            // buttonCalibration
            // 
            this.buttonCalibration.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCalibration.Location = new System.Drawing.Point(0, 96);
            this.buttonCalibration.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCalibration.Name = "buttonCalibration";
            this.buttonCalibration.Size = new System.Drawing.Size(160, 40);
            this.buttonCalibration.TabIndex = 11;
            this.buttonCalibration.Text = "Calibrate";
            this.buttonCalibration.UseVisualStyleBackColor = true;
            this.buttonCalibration.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // buttonFoul
            // 
            this.buttonFoul.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonFoul.Location = new System.Drawing.Point(0, 53);
            this.buttonFoul.Margin = new System.Windows.Forms.Padding(2);
            this.buttonFoul.Name = "buttonFoul";
            this.buttonFoul.Size = new System.Drawing.Size(160, 40);
            this.buttonFoul.TabIndex = 13;
            this.buttonFoul.Text = "Foul";
            this.buttonFoul.UseVisualStyleBackColor = true;
            this.buttonFoul.Click += new System.EventHandler(this.buttonFoul_Click);
            // 
            // buttonEnd
            // 
            this.buttonEnd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonEnd.Enabled = false;
            this.buttonEnd.Location = new System.Drawing.Point(165, 141);
            this.buttonEnd.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnd.Name = "buttonEnd";
            this.buttonEnd.Size = new System.Drawing.Size(160, 40);
            this.buttonEnd.TabIndex = 14;
            this.buttonEnd.Text = "End";
            this.buttonEnd.UseVisualStyleBackColor = true;
            this.buttonEnd.Click += new System.EventHandler(this.buttonEnd_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonReset.Location = new System.Drawing.Point(0, 10);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(2);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(160, 40);
            this.buttonReset.TabIndex = 16;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonSettings
            // 
            this.buttonSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSettings.Location = new System.Drawing.Point(0, 141);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(160, 40);
            this.buttonSettings.TabIndex = 17;
            this.buttonSettings.Text = "Settings";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Blue;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(0, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(327, 65);
            this.label1.TabIndex = 6;
            this.label1.Text = "                       ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Red;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(327, 65);
            this.label2.TabIndex = 5;
            this.label2.Text = "                       ";
            // 
            // labelGameTime
            // 
            this.labelGameTime.AutoSize = true;
            this.labelGameTime.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelGameTime.Location = new System.Drawing.Point(0, 171);
            this.labelGameTime.Name = "labelGameTime";
            this.labelGameTime.Size = new System.Drawing.Size(355, 65);
            this.labelGameTime.TabIndex = 18;
            this.labelGameTime.Text = "labelGameTime";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.progressBarRemainingPowerRatio);
            this.panel1.Controls.Add(this.labelScoreVehicleB);
            this.panel1.Controls.Add(this.labelScoreVehicleA);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.labelGameTime);
            this.panel1.Controls.Add(this.labelGameHalf);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(10, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(331, 661);
            this.panel1.TabIndex = 20;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(0, 333);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(203, 32);
            this.label5.TabIndex = 23;
            this.label5.Text = "Remaining Power:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(0, 236);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 32);
            this.label4.TabIndex = 22;
            this.label4.Text = "Game Half:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label3.Location = new System.Drawing.Point(0, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 32);
            this.label3.TabIndex = 21;
            this.label3.Text = "Remaining Time:";
            // 
            // progressBarRemainingPowerRatio
            // 
            this.progressBarRemainingPowerRatio.Location = new System.Drawing.Point(0, 368);
            this.progressBarRemainingPowerRatio.Name = "progressBarRemainingPowerRatio";
            this.progressBarRemainingPowerRatio.Size = new System.Drawing.Size(325, 23);
            this.progressBarRemainingPowerRatio.TabIndex = 20;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonSettings);
            this.panel3.Controls.Add(this.buttonStart);
            this.panel3.Controls.Add(this.buttonReset);
            this.panel3.Controls.Add(this.buttonPause);
            this.panel3.Controls.Add(this.buttonContinue);
            this.panel3.Controls.Add(this.buttonEnd);
            this.panel3.Controls.Add(this.buttonCalibration);
            this.panel3.Controls.Add(this.buttonFoul);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 480);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(331, 181);
            this.panel3.TabIndex = 1;
            // 
            // labelGameHalf
            // 
            this.labelGameHalf.AutoSize = true;
            this.labelGameHalf.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelGameHalf.Location = new System.Drawing.Point(0, 268);
            this.labelGameHalf.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelGameHalf.Name = "labelGameHalf";
            this.labelGameHalf.Size = new System.Drawing.Size(337, 65);
            this.labelGameHalf.TabIndex = 4;
            this.labelGameHalf.Text = "labelGameHalf";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pictureBoxMonitor);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(341, 10);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(913, 661);
            this.panel2.TabIndex = 21;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1280, 720);
            this.Name = "MainWindow";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "EDC Host EDC_HOST_BUILD_REF";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMonitor)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.PictureBox pictureBoxMonitor;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.Button buttonPause;
    private System.Windows.Forms.Button buttonContinue;
    private System.Windows.Forms.Label labelScoreVehicleA;
    private System.Windows.Forms.Label labelScoreVehicleB;
    private System.Windows.Forms.Button buttonCalibration;
    private System.Windows.Forms.Button buttonFoul;
    private System.Windows.Forms.Button buttonEnd;
    private System.Windows.Forms.Button buttonReset;
    private System.Windows.Forms.Button buttonSettings;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelGameTime;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ProgressBar progressBarRemainingPowerRatio;
    private System.Windows.Forms.Label labelGameHalf;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
}
