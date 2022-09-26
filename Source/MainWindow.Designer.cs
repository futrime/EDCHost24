namespace EdcHost;
partial class MainWindow
{
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
            Camera.Dispose();
            CoordinateConverter.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.MonitorPictureBox = new System.Windows.Forms.PictureBox();
            this.timerMsg100ms = new System.Windows.Forms.Timer(this.components);
            this.StartButton = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.ContinueButton = new System.Windows.Forms.Button();
            this.GameRoundLabel = new System.Windows.Forms.Label();
            this.ScoreALabel = new System.Windows.Forms.Label();
            this.ScoreBLabel = new System.Windows.Forms.Label();
            this.CalibrateButton = new System.Windows.Forms.Button();
            this.FoulButton = new System.Windows.Forms.Button();
            this.EndButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.ScoreBBackgroundLabel = new System.Windows.Forms.Label();
            this.ScoreABackgroundLabel = new System.Windows.Forms.Label();
            this.GameTimeLabel = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.MonitorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MonitorPictureBox
            // 
            this.MonitorPictureBox.Location = new System.Drawing.Point(277, 11);
            this.MonitorPictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.MonitorPictureBox.Name = "MonitorPictureBox";
            this.MonitorPictureBox.Size = new System.Drawing.Size(916, 650);
            this.MonitorPictureBox.TabIndex = 0;
            this.MonitorPictureBox.TabStop = false;
            this.MonitorPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMonitorMouseClick);
            // 
            // timerMsg100ms
            // 
            this.timerMsg100ms.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(112, 531);
            this.StartButton.Margin = new System.Windows.Forms.Padding(2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(94, 29);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.OnStartButtonClick);
            // 
            // PauseButton
            // 
            this.PauseButton.Enabled = false;
            this.PauseButton.Location = new System.Drawing.Point(112, 564);
            this.PauseButton.Margin = new System.Windows.Forms.Padding(2);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(94, 29);
            this.PauseButton.TabIndex = 2;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.OnPauseButtonClick);
            // 
            // ContinueButton
            // 
            this.ContinueButton.Enabled = false;
            this.ContinueButton.Location = new System.Drawing.Point(112, 597);
            this.ContinueButton.Margin = new System.Windows.Forms.Padding(2);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(94, 29);
            this.ContinueButton.TabIndex = 3;
            this.ContinueButton.Text = "Continue";
            this.ContinueButton.UseVisualStyleBackColor = true;
            this.ContinueButton.Click += new System.EventHandler(this.OnContinueButtonClick);
            // 
            // GameRoundLabel
            // 
            this.GameRoundLabel.AutoSize = true;
            this.GameRoundLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.GameRoundLabel.Location = new System.Drawing.Point(14, 273);
            this.GameRoundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.GameRoundLabel.Name = "GameRoundLabel";
            this.GameRoundLabel.Size = new System.Drawing.Size(502, 81);
            this.GameRoundLabel.TabIndex = 4;
            this.GameRoundLabel.Text = "GameRoundLabel";
            // 
            // ScoreALabel
            // 
            this.ScoreALabel.AutoSize = true;
            this.ScoreALabel.BackColor = System.Drawing.Color.Red;
            this.ScoreALabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreALabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ScoreALabel.Location = new System.Drawing.Point(14, 11);
            this.ScoreALabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreALabel.Name = "ScoreALabel";
            this.ScoreALabel.Size = new System.Drawing.Size(361, 81);
            this.ScoreALabel.TabIndex = 8;
            this.ScoreALabel.Text = "ScoreALabel";
            // 
            // ScoreBLabel
            // 
            this.ScoreBLabel.AutoSize = true;
            this.ScoreBLabel.BackColor = System.Drawing.Color.Blue;
            this.ScoreBLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreBLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ScoreBLabel.Location = new System.Drawing.Point(14, 100);
            this.ScoreBLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreBLabel.Name = "ScoreBLabel";
            this.ScoreBLabel.Size = new System.Drawing.Size(356, 81);
            this.ScoreBLabel.TabIndex = 10;
            this.ScoreBLabel.Text = "ScoreBLabel";
            // 
            // CalibrateButton
            // 
            this.CalibrateButton.Location = new System.Drawing.Point(14, 597);
            this.CalibrateButton.Margin = new System.Windows.Forms.Padding(2);
            this.CalibrateButton.Name = "CalibrateButton";
            this.CalibrateButton.Size = new System.Drawing.Size(94, 29);
            this.CalibrateButton.TabIndex = 11;
            this.CalibrateButton.Text = "Calibrate";
            this.CalibrateButton.UseVisualStyleBackColor = true;
            this.CalibrateButton.Click += new System.EventHandler(this.OnCalibrateButtonClick);
            // 
            // FoulButton
            // 
            this.FoulButton.Location = new System.Drawing.Point(14, 564);
            this.FoulButton.Margin = new System.Windows.Forms.Padding(2);
            this.FoulButton.Name = "FoulButton";
            this.FoulButton.Size = new System.Drawing.Size(94, 29);
            this.FoulButton.TabIndex = 13;
            this.FoulButton.Text = "Foul";
            this.FoulButton.UseVisualStyleBackColor = true;
            this.FoulButton.Click += new System.EventHandler(this.OnFoulButtonClick);
            // 
            // EndButton
            // 
            this.EndButton.Enabled = false;
            this.EndButton.Location = new System.Drawing.Point(113, 632);
            this.EndButton.Margin = new System.Windows.Forms.Padding(2);
            this.EndButton.Name = "EndButton";
            this.EndButton.Size = new System.Drawing.Size(94, 29);
            this.EndButton.TabIndex = 14;
            this.EndButton.Text = "End";
            this.EndButton.UseVisualStyleBackColor = true;
            this.EndButton.Click += new System.EventHandler(this.OnEndButtonClick);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(14, 531);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(2);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(94, 29);
            this.ResetButton.TabIndex = 16;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.OnResetButtonClick);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Location = new System.Drawing.Point(13, 632);
            this.SettingsButton.Margin = new System.Windows.Forms.Padding(4);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(94, 29);
            this.SettingsButton.TabIndex = 17;
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.UseVisualStyleBackColor = true;
            this.SettingsButton.Click += new System.EventHandler(this.OnSettingsButtonClick);
            // 
            // ScoreBBackgroundLabel
            // 
            this.ScoreBBackgroundLabel.AutoSize = true;
            this.ScoreBBackgroundLabel.BackColor = System.Drawing.Color.Blue;
            this.ScoreBBackgroundLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreBBackgroundLabel.Location = new System.Drawing.Point(14, 100);
            this.ScoreBBackgroundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreBBackgroundLabel.Name = "ScoreBBackgroundLabel";
            this.ScoreBBackgroundLabel.Size = new System.Drawing.Size(259, 81);
            this.ScoreBBackgroundLabel.TabIndex = 6;
            this.ScoreBBackgroundLabel.Text = "              ";
            // 
            // ScoreABackgroundLabel
            // 
            this.ScoreABackgroundLabel.AutoSize = true;
            this.ScoreABackgroundLabel.BackColor = System.Drawing.Color.Red;
            this.ScoreABackgroundLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreABackgroundLabel.Location = new System.Drawing.Point(14, 11);
            this.ScoreABackgroundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreABackgroundLabel.Name = "ScoreABackgroundLabel";
            this.ScoreABackgroundLabel.Size = new System.Drawing.Size(259, 81);
            this.ScoreABackgroundLabel.TabIndex = 5;
            this.ScoreABackgroundLabel.Text = "              ";
            // 
            // GameTimeLabel
            // 
            this.GameTimeLabel.AutoSize = true;
            this.GameTimeLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.GameTimeLabel.Location = new System.Drawing.Point(14, 192);
            this.GameTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.GameTimeLabel.Name = "GameTimeLabel";
            this.GameTimeLabel.Size = new System.Drawing.Size(459, 81);
            this.GameTimeLabel.TabIndex = 18;
            this.GameTimeLabel.Text = "GameTimeLabel";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(453, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(250, 27);
            this.dateTimePicker1.TabIndex = 19;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1204, 674);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.SettingsButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.EndButton);
            this.Controls.Add(this.FoulButton);
            this.Controls.Add(this.CalibrateButton);
            this.Controls.Add(this.ContinueButton);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.MonitorPictureBox);
            this.Controls.Add(this.GameTimeLabel);
            this.Controls.Add(this.GameRoundLabel);
            this.Controls.Add(this.ScoreALabel);
            this.Controls.Add(this.ScoreABackgroundLabel);
            this.Controls.Add(this.ScoreBLabel);
            this.Controls.Add(this.ScoreBBackgroundLabel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainWindow";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnLoad);
            ((System.ComponentModel.ISupportInitialize)(this.MonitorPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.PictureBox MonitorPictureBox;
    private System.Windows.Forms.Timer timerMsg100ms;
    private System.Windows.Forms.Button StartButton;
    private System.Windows.Forms.Button PauseButton;
    private System.Windows.Forms.Button ContinueButton;
    private System.Windows.Forms.Label GameRoundLabel;
    private System.Windows.Forms.Label ScoreALabel;
    private System.Windows.Forms.Label ScoreBLabel;
    private System.Windows.Forms.Button CalibrateButton;
    private System.Windows.Forms.Button FoulButton;
    private System.Windows.Forms.Button EndButton;
    private System.Windows.Forms.Button ResetButton;
    private System.Windows.Forms.Button SettingsButton;
    private System.Windows.Forms.Label ScoreBBackgroundLabel;
    private System.Windows.Forms.Label ScoreABackgroundLabel;
    private System.Windows.Forms.Label GameTimeLabel;
    private System.Windows.Forms.DateTimePicker dateTimePicker1;
}