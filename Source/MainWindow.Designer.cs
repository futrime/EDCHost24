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
            this.Timer = new System.Windows.Forms.Timer(this.components);
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
            this.DistanceLeftLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.MonitorPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MonitorPictureBox
            // 
            this.MonitorPictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.MonitorPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MonitorPictureBox.Location = new System.Drawing.Point(5, 5);
            this.MonitorPictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.MonitorPictureBox.Name = "MonitorPictureBox";
            this.MonitorPictureBox.Size = new System.Drawing.Size(905, 652);
            this.MonitorPictureBox.TabIndex = 0;
            this.MonitorPictureBox.TabStop = false;
            this.MonitorPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMonitorMouseClick);
            this.MonitorPictureBox.Resize += new System.EventHandler(this.OnMonitorPictureBoxResize);
            // 
            // Timer
            // 
            this.Timer.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // StartButton
            // 
            this.StartButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StartButton.Location = new System.Drawing.Point(168, 4);
            this.StartButton.Margin = new System.Windows.Forms.Padding(2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(160, 40);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.OnStartButtonClick);
            // 
            // PauseButton
            // 
            this.PauseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PauseButton.Enabled = false;
            this.PauseButton.Location = new System.Drawing.Point(168, 47);
            this.PauseButton.Margin = new System.Windows.Forms.Padding(2);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(160, 40);
            this.PauseButton.TabIndex = 2;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.OnPauseButtonClick);
            // 
            // ContinueButton
            // 
            this.ContinueButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ContinueButton.Enabled = false;
            this.ContinueButton.Location = new System.Drawing.Point(168, 90);
            this.ContinueButton.Margin = new System.Windows.Forms.Padding(2);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(160, 40);
            this.ContinueButton.TabIndex = 3;
            this.ContinueButton.Text = "Continue";
            this.ContinueButton.UseVisualStyleBackColor = true;
            this.ContinueButton.Click += new System.EventHandler(this.OnContinueButtonClick);
            // 
            // GameRoundLabel
            // 
            this.GameRoundLabel.AutoSize = true;
            this.GameRoundLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.GameRoundLabel.Location = new System.Drawing.Point(3, 195);
            this.GameRoundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.GameRoundLabel.Name = "GameRoundLabel";
            this.GameRoundLabel.Size = new System.Drawing.Size(401, 65);
            this.GameRoundLabel.TabIndex = 4;
            this.GameRoundLabel.Text = "GameRoundLabel";
            // 
            // ScoreALabel
            // 
            this.ScoreALabel.AutoSize = true;
            this.ScoreALabel.BackColor = System.Drawing.Color.Red;
            this.ScoreALabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreALabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ScoreALabel.Location = new System.Drawing.Point(2, 0);
            this.ScoreALabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreALabel.Name = "ScoreALabel";
            this.ScoreALabel.Size = new System.Drawing.Size(289, 65);
            this.ScoreALabel.TabIndex = 8;
            this.ScoreALabel.Text = "ScoreALabel";
            // 
            // ScoreBLabel
            // 
            this.ScoreBLabel.AutoSize = true;
            this.ScoreBLabel.BackColor = System.Drawing.Color.Blue;
            this.ScoreBLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreBLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ScoreBLabel.Location = new System.Drawing.Point(2, 65);
            this.ScoreBLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreBLabel.Name = "ScoreBLabel";
            this.ScoreBLabel.Size = new System.Drawing.Size(286, 65);
            this.ScoreBLabel.TabIndex = 10;
            this.ScoreBLabel.Text = "ScoreBLabel";
            // 
            // CalibrateButton
            // 
            this.CalibrateButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CalibrateButton.Location = new System.Drawing.Point(3, 90);
            this.CalibrateButton.Margin = new System.Windows.Forms.Padding(2);
            this.CalibrateButton.Name = "CalibrateButton";
            this.CalibrateButton.Size = new System.Drawing.Size(160, 40);
            this.CalibrateButton.TabIndex = 11;
            this.CalibrateButton.Text = "Calibrate";
            this.CalibrateButton.UseVisualStyleBackColor = true;
            this.CalibrateButton.Click += new System.EventHandler(this.OnCalibrateButtonClick);
            // 
            // FoulButton
            // 
            this.FoulButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.FoulButton.Location = new System.Drawing.Point(3, 47);
            this.FoulButton.Margin = new System.Windows.Forms.Padding(2);
            this.FoulButton.Name = "FoulButton";
            this.FoulButton.Size = new System.Drawing.Size(160, 40);
            this.FoulButton.TabIndex = 13;
            this.FoulButton.Text = "Foul";
            this.FoulButton.UseVisualStyleBackColor = true;
            this.FoulButton.Click += new System.EventHandler(this.OnFoulButtonClick);
            // 
            // EndButton
            // 
            this.EndButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EndButton.Enabled = false;
            this.EndButton.Location = new System.Drawing.Point(168, 135);
            this.EndButton.Margin = new System.Windows.Forms.Padding(2);
            this.EndButton.Name = "EndButton";
            this.EndButton.Size = new System.Drawing.Size(160, 40);
            this.EndButton.TabIndex = 14;
            this.EndButton.Text = "End";
            this.EndButton.UseVisualStyleBackColor = true;
            this.EndButton.Click += new System.EventHandler(this.OnEndButtonClick);
            // 
            // ResetButton
            // 
            this.ResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetButton.Location = new System.Drawing.Point(3, 4);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(2);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(160, 40);
            this.ResetButton.TabIndex = 16;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.OnResetButtonClick);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SettingsButton.Location = new System.Drawing.Point(3, 135);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(160, 40);
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
            this.ScoreBBackgroundLabel.Location = new System.Drawing.Point(2, 65);
            this.ScoreBBackgroundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreBBackgroundLabel.Name = "ScoreBBackgroundLabel";
            this.ScoreBBackgroundLabel.Size = new System.Drawing.Size(210, 65);
            this.ScoreBBackgroundLabel.TabIndex = 6;
            this.ScoreBBackgroundLabel.Text = "              ";
            // 
            // ScoreABackgroundLabel
            // 
            this.ScoreABackgroundLabel.AutoSize = true;
            this.ScoreABackgroundLabel.BackColor = System.Drawing.Color.Red;
            this.ScoreABackgroundLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreABackgroundLabel.Location = new System.Drawing.Point(2, 0);
            this.ScoreABackgroundLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreABackgroundLabel.Name = "ScoreABackgroundLabel";
            this.ScoreABackgroundLabel.Size = new System.Drawing.Size(210, 65);
            this.ScoreABackgroundLabel.TabIndex = 5;
            this.ScoreABackgroundLabel.Text = "              ";
            // 
            // GameTimeLabel
            // 
            this.GameTimeLabel.AutoSize = true;
            this.GameTimeLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.GameTimeLabel.Location = new System.Drawing.Point(3, 130);
            this.GameTimeLabel.Name = "GameTimeLabel";
            this.GameTimeLabel.Size = new System.Drawing.Size(366, 65);
            this.GameTimeLabel.TabIndex = 18;
            this.GameTimeLabel.Text = "GameTimeLabel";
            // 
            // DistanceLeftLabel
            // 
            this.DistanceLeftLabel.AutoSize = true;
            this.DistanceLeftLabel.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.DistanceLeftLabel.Location = new System.Drawing.Point(3, 260);
            this.DistanceLeftLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DistanceLeftLabel.Name = "DistanceLeftLabel";
            this.DistanceLeftLabel.Size = new System.Drawing.Size(399, 65);
            this.DistanceLeftLabel.TabIndex = 19;
            this.DistanceLeftLabel.Text = "DistanceLeftLabel";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.ScoreALabel);
            this.panel1.Controls.Add(this.ScoreABackgroundLabel);
            this.panel1.Controls.Add(this.ScoreBLabel);
            this.panel1.Controls.Add(this.ScoreBBackgroundLabel);
            this.panel1.Controls.Add(this.GameTimeLabel);
            this.panel1.Controls.Add(this.GameRoundLabel);
            this.panel1.Controls.Add(this.DistanceLeftLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(10, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(331, 662);
            this.panel1.TabIndex = 20;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.SettingsButton);
            this.panel3.Controls.Add(this.StartButton);
            this.panel3.Controls.Add(this.ResetButton);
            this.panel3.Controls.Add(this.PauseButton);
            this.panel3.Controls.Add(this.ContinueButton);
            this.panel3.Controls.Add(this.EndButton);
            this.panel3.Controls.Add(this.CalibrateButton);
            this.panel3.Controls.Add(this.FoulButton);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 481);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(331, 181);
            this.panel3.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.MonitorPictureBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(341, 10);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5);
            this.panel2.Size = new System.Drawing.Size(915, 662);
            this.panel2.TabIndex = 21;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1266, 682);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainWindow";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.MonitorPictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.PictureBox MonitorPictureBox;
    private System.Windows.Forms.Timer Timer;
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
    private System.Windows.Forms.Label DistanceLeftLabel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel2;
}