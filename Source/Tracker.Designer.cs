namespace EdcHost;
partial class Tracker
{
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
            capture.Dispose();
            coordCvt.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.timerMsg100ms = new System.Windows.Forms.Timer(this.components);
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.button_Continue = new System.Windows.Forms.Button();
            this.label_GameCount = new System.Windows.Forms.Label();
            this.labelAScore = new System.Windows.Forms.Label();
            this.labelBScore = new System.Windows.Forms.Label();
            this.CalibrateButton = new System.Windows.Forms.Button();
            this.buttonFoul = new System.Windows.Forms.Button();
            this.buttonEnd = new System.Windows.Forms.Button();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.label_BlueBG = new System.Windows.Forms.Label();
            this.label_RedBG = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCamera
            // 
            this.pbCamera.Location = new System.Drawing.Point(345, 169);
            this.pbCamera.Margin = new System.Windows.Forms.Padding(2);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(286, 269);
            this.pbCamera.TabIndex = 0;
            this.pbCamera.TabStop = false;
            this.pbCamera.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMonitorMouseClick);
            // 
            // timerMsg100ms
            // 
            this.timerMsg100ms.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(309, 115);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.OnStartButtonClick);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(432, 108);
            this.buttonPause.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(75, 23);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.OnPauseButtonClick);
            // 
            // button_Continue
            // 
            this.button_Continue.Location = new System.Drawing.Point(549, 111);
            this.button_Continue.Margin = new System.Windows.Forms.Padding(2);
            this.button_Continue.Name = "button_Continue";
            this.button_Continue.Size = new System.Drawing.Size(75, 23);
            this.button_Continue.TabIndex = 3;
            this.button_Continue.Text = "Continue";
            this.button_Continue.UseVisualStyleBackColor = true;
            this.button_Continue.Click += new System.EventHandler(this.OnContinueButtonClick);
            // 
            // label_GameCount
            // 
            this.label_GameCount.AutoSize = true;
            this.label_GameCount.Location = new System.Drawing.Point(442, 52);
            this.label_GameCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_GameCount.Name = "label_GameCount";
            this.label_GameCount.Size = new System.Drawing.Size(38, 15);
            this.label_GameCount.TabIndex = 4;
            this.label_GameCount.Text = "label1";
            // 
            // labelAScore
            // 
            this.labelAScore.AutoSize = true;
            this.labelAScore.BackColor = System.Drawing.Color.Red;
            this.labelAScore.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelAScore.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelAScore.Location = new System.Drawing.Point(11, 9);
            this.labelAScore.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAScore.Name = "labelAScore";
            this.labelAScore.Size = new System.Drawing.Size(190, 65);
            this.labelAScore.TabIndex = 8;
            this.labelAScore.Text = "A Score";
            // 
            // labelBScore
            // 
            this.labelBScore.AutoSize = true;
            this.labelBScore.BackColor = System.Drawing.Color.Blue;
            this.labelBScore.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelBScore.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelBScore.Location = new System.Drawing.Point(742, 12);
            this.labelBScore.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelBScore.Name = "labelBScore";
            this.labelBScore.Size = new System.Drawing.Size(187, 65);
            this.labelBScore.TabIndex = 10;
            this.labelBScore.Text = "B Score";
            // 
            // CalibrateButton
            // 
            this.CalibrateButton.Location = new System.Drawing.Point(553, 80);
            this.CalibrateButton.Margin = new System.Windows.Forms.Padding(2);
            this.CalibrateButton.Name = "CalibrateButton";
            this.CalibrateButton.Size = new System.Drawing.Size(75, 23);
            this.CalibrateButton.TabIndex = 11;
            this.CalibrateButton.Text = "Calibrate";
            this.CalibrateButton.UseVisualStyleBackColor = true;
            this.CalibrateButton.Click += new System.EventHandler(this.OnCalibrateButtonClick);
            // 
            // buttonFoul
            // 
            this.buttonFoul.Location = new System.Drawing.Point(444, 75);
            this.buttonFoul.Margin = new System.Windows.Forms.Padding(2);
            this.buttonFoul.Name = "buttonFoul";
            this.buttonFoul.Size = new System.Drawing.Size(75, 23);
            this.buttonFoul.TabIndex = 13;
            this.buttonFoul.Text = "Foul";
            this.buttonFoul.UseVisualStyleBackColor = true;
            this.buttonFoul.Click += new System.EventHandler(this.OnFoulButtonClick);
            // 
            // buttonEnd
            // 
            this.buttonEnd.Location = new System.Drawing.Point(655, 144);
            this.buttonEnd.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnd.Name = "buttonEnd";
            this.buttonEnd.Size = new System.Drawing.Size(75, 23);
            this.buttonEnd.TabIndex = 14;
            this.buttonEnd.Text = "End";
            this.buttonEnd.UseVisualStyleBackColor = true;
            this.buttonEnd.Click += new System.EventHandler(this.OnEndButtonClick);
            // 
            // buttonRestart
            // 
            this.buttonRestart.Location = new System.Drawing.Point(643, 237);
            this.buttonRestart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Size = new System.Drawing.Size(75, 23);
            this.buttonRestart.TabIndex = 16;
            this.buttonRestart.Text = "Restart";
            this.buttonRestart.UseVisualStyleBackColor = true;
            this.buttonRestart.Click += new System.EventHandler(this.OnRestartButtonClick);
            // 
            // buttonSettings
            // 
            this.buttonSettings.Location = new System.Drawing.Point(332, 75);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(75, 23);
            this.buttonSettings.TabIndex = 17;
            this.buttonSettings.Text = "Settings";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.OnSettingsButtonClick);
            // 
            // label_BlueBG
            // 
            this.label_BlueBG.AutoSize = true;
            this.label_BlueBG.BackColor = System.Drawing.Color.Blue;
            this.label_BlueBG.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label_BlueBG.Location = new System.Drawing.Point(742, 12);
            this.label_BlueBG.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_BlueBG.Name = "label_BlueBG";
            this.label_BlueBG.Size = new System.Drawing.Size(210, 65);
            this.label_BlueBG.TabIndex = 6;
            this.label_BlueBG.Text = "              ";
            // 
            // label_RedBG
            // 
            this.label_RedBG.AutoSize = true;
            this.label_RedBG.BackColor = System.Drawing.Color.Red;
            this.label_RedBG.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label_RedBG.Location = new System.Drawing.Point(11, 9);
            this.label_RedBG.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_RedBG.Name = "label_RedBG";
            this.label_RedBG.Size = new System.Drawing.Size(210, 65);
            this.label_RedBG.TabIndex = 5;
            this.label_RedBG.Text = "              ";
            // 
            // Tracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(963, 539);
            this.Controls.Add(this.buttonSettings);
            this.Controls.Add(this.buttonRestart);
            this.Controls.Add(this.buttonEnd);
            this.Controls.Add(this.buttonFoul);
            this.Controls.Add(this.CalibrateButton);
            this.Controls.Add(this.labelBScore);
            this.Controls.Add(this.labelAScore);
            this.Controls.Add(this.label_BlueBG);
            this.Controls.Add(this.label_RedBG);
            this.Controls.Add(this.label_GameCount);
            this.Controls.Add(this.button_Continue);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.pbCamera);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Tracker";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.Tracker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.PictureBox pbCamera;
    private System.Windows.Forms.Timer timerMsg100ms;
    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.Button buttonPause;
    private System.Windows.Forms.Button button_Continue;
    private System.Windows.Forms.Label label_GameCount;
    private System.Windows.Forms.Label labelAScore;
    private System.Windows.Forms.Label labelBScore;
    private System.Windows.Forms.Button CalibrateButton;
    private System.Windows.Forms.Button buttonFoul;
    private System.Windows.Forms.Button buttonEnd;
    private System.Windows.Forms.Button buttonRestart;
    private System.Windows.Forms.Button buttonSettings;
    private System.Windows.Forms.Label label_BlueBG;
    private System.Windows.Forms.Label label_RedBG;
}