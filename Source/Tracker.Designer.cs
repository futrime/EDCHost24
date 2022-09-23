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
        this.label_RedBG = new System.Windows.Forms.Label();
        this.label_BlueBG = new System.Windows.Forms.Label();
        this.label_CarA = new System.Windows.Forms.Label();
        this.labelAScore = new System.Windows.Forms.Label();
        this.label_CarB = new System.Windows.Forms.Label();
        this.labelBScore = new System.Windows.Forms.Label();
        this.btnReset = new System.Windows.Forms.Button();
        this.button_set = new System.Windows.Forms.Button();
        this.buttonFoul = new System.Windows.Forms.Button();
        this.buttonEnd = new System.Windows.Forms.Button();
        this.buttonSetStation = new System.Windows.Forms.Button();
        this.buttonRestart = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
        this.SuspendLayout();
        // 
        // pbCamera
        // 
        this.pbCamera.Location = new System.Drawing.Point(431, 211);
        this.pbCamera.Name = "pbCamera";
        this.pbCamera.Size = new System.Drawing.Size(358, 336);
        this.pbCamera.TabIndex = 0;
        this.pbCamera.TabStop = false;
        this.pbCamera.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbCamera_MouseClick);
        // 
        // timerMsg100ms
        // 
        this.timerMsg100ms.Tick += new System.EventHandler(this.timerMsg100ms_Tick);
        // 
        // buttonStart
        // 
        this.buttonStart.Location = new System.Drawing.Point(386, 144);
        this.buttonStart.Name = "buttonStart";
        this.buttonStart.Size = new System.Drawing.Size(94, 29);
        this.buttonStart.TabIndex = 1;
        this.buttonStart.Text = "Start";
        this.buttonStart.UseVisualStyleBackColor = true;
        this.buttonStart.Click += new System.EventHandler(this.bottonStartA_FirstHalf_Click);
        // 
        // buttonPause
        // 
        this.buttonPause.Location = new System.Drawing.Point(540, 135);
        this.buttonPause.Name = "buttonPause";
        this.buttonPause.Size = new System.Drawing.Size(94, 29);
        this.buttonPause.TabIndex = 2;
        this.buttonPause.Text = "Pause";
        this.buttonPause.UseVisualStyleBackColor = true;
        this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
        // 
        // button_Continue
        // 
        this.button_Continue.Location = new System.Drawing.Point(686, 139);
        this.button_Continue.Name = "button_Continue";
        this.button_Continue.Size = new System.Drawing.Size(94, 29);
        this.button_Continue.TabIndex = 3;
        this.button_Continue.Text = "Continue";
        this.button_Continue.UseVisualStyleBackColor = true;
        this.button_Continue.Click += new System.EventHandler(this.buttonContinue_Click);
        // 
        // label_GameCount
        // 
        this.label_GameCount.AutoSize = true;
        this.label_GameCount.Location = new System.Drawing.Point(552, 65);
        this.label_GameCount.Name = "label_GameCount";
        this.label_GameCount.Size = new System.Drawing.Size(50, 20);
        this.label_GameCount.TabIndex = 4;
        this.label_GameCount.Text = "label1";
        // 
        // label_RedBG
        // 
        this.label_RedBG.AutoSize = true;
        this.label_RedBG.Location = new System.Drawing.Point(108, 65);
        this.label_RedBG.Name = "label_RedBG";
        this.label_RedBG.Size = new System.Drawing.Size(50, 20);
        this.label_RedBG.TabIndex = 5;
        this.label_RedBG.Text = "label1";
        // 
        // label_BlueBG
        // 
        this.label_BlueBG.AutoSize = true;
        this.label_BlueBG.Location = new System.Drawing.Point(1007, 65);
        this.label_BlueBG.Name = "label_BlueBG";
        this.label_BlueBG.Size = new System.Drawing.Size(50, 20);
        this.label_BlueBG.TabIndex = 6;
        this.label_BlueBG.Text = "label1";
        // 
        // label_CarA
        // 
        this.label_CarA.AutoSize = true;
        this.label_CarA.Location = new System.Drawing.Point(108, 231);
        this.label_CarA.Name = "label_CarA";
        this.label_CarA.Size = new System.Drawing.Size(50, 20);
        this.label_CarA.TabIndex = 7;
        this.label_CarA.Text = "label1";
        // 
        // labelAScore
        // 
        this.labelAScore.AutoSize = true;
        this.labelAScore.Location = new System.Drawing.Point(108, 143);
        this.labelAScore.Name = "labelAScore";
        this.labelAScore.Size = new System.Drawing.Size(50, 20);
        this.labelAScore.TabIndex = 8;
        this.labelAScore.Text = "label1";
        // 
        // label_CarB
        // 
        this.label_CarB.AutoSize = true;
        this.label_CarB.Location = new System.Drawing.Point(1028, 144);
        this.label_CarB.Name = "label_CarB";
        this.label_CarB.Size = new System.Drawing.Size(50, 20);
        this.label_CarB.TabIndex = 9;
        this.label_CarB.Text = "label1";
        // 
        // labelBScore
        // 
        this.labelBScore.AutoSize = true;
        this.labelBScore.Location = new System.Drawing.Point(1047, 241);
        this.labelBScore.Name = "labelBScore";
        this.labelBScore.Size = new System.Drawing.Size(50, 20);
        this.labelBScore.TabIndex = 10;
        this.labelBScore.Text = "label1";
        // 
        // btnReset
        // 
        this.btnReset.Location = new System.Drawing.Point(691, 100);
        this.btnReset.Name = "btnReset";
        this.btnReset.Size = new System.Drawing.Size(94, 29);
        this.btnReset.TabIndex = 11;
        this.btnReset.Text = "Reset";
        this.btnReset.UseVisualStyleBackColor = true;
        this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
        // 
        // button_set
        // 
        this.button_set.Location = new System.Drawing.Point(424, 96);
        this.button_set.Name = "button_set";
        this.button_set.Size = new System.Drawing.Size(94, 29);
        this.button_set.TabIndex = 12;
        this.button_set.Text = "Settings";
        this.button_set.UseVisualStyleBackColor = true;
        this.button_set.Click += new System.EventHandler(this.button_set_Click);
        // 
        // buttonFoul
        // 
        this.buttonFoul.Location = new System.Drawing.Point(555, 94);
        this.buttonFoul.Name = "buttonFoul";
        this.buttonFoul.Size = new System.Drawing.Size(94, 29);
        this.buttonFoul.TabIndex = 13;
        this.buttonFoul.Text = "Foul";
        this.buttonFoul.UseVisualStyleBackColor = true;
        this.buttonFoul.Click += new System.EventHandler(this.buttonFoul_Click);
        // 
        // buttonEnd
        // 
        this.buttonEnd.Location = new System.Drawing.Point(819, 180);
        this.buttonEnd.Name = "buttonEnd";
        this.buttonEnd.Size = new System.Drawing.Size(94, 29);
        this.buttonEnd.TabIndex = 14;
        this.buttonEnd.Text = "End";
        this.buttonEnd.UseVisualStyleBackColor = true;
        this.buttonEnd.Click += new System.EventHandler(this.buttonEnd_Click);
        // 
        // buttonSetStation
        // 
        this.buttonSetStation.Location = new System.Drawing.Point(824, 251);
        this.buttonSetStation.Name = "buttonSetStation";
        this.buttonSetStation.Size = new System.Drawing.Size(94, 29);
        this.buttonSetStation.TabIndex = 15;
        this.buttonSetStation.Text = "Set Station";
        this.buttonSetStation.UseVisualStyleBackColor = true;
        this.buttonSetStation.Click += new System.EventHandler(this.buttonSetStation_Click);
        // 
        // buttonRestart
        // 
        this.buttonRestart.Location = new System.Drawing.Point(804, 296);
        this.buttonRestart.Name = "buttonRestart";
        this.buttonRestart.Size = new System.Drawing.Size(94, 29);
        this.buttonRestart.TabIndex = 16;
        this.buttonRestart.Text = "Restart";
        this.buttonRestart.UseVisualStyleBackColor = true;
        this.buttonRestart.Click += new System.EventHandler(this.buttonRestart_click);
        // 
        // Tracker
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(1204, 674);
        this.Controls.Add(this.buttonRestart);
        this.Controls.Add(this.buttonSetStation);
        this.Controls.Add(this.buttonEnd);
        this.Controls.Add(this.buttonFoul);
        this.Controls.Add(this.button_set);
        this.Controls.Add(this.btnReset);
        this.Controls.Add(this.labelBScore);
        this.Controls.Add(this.label_CarB);
        this.Controls.Add(this.labelAScore);
        this.Controls.Add(this.label_CarA);
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
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Tracker_FormClosed);
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
    private System.Windows.Forms.Label label_RedBG;
    private System.Windows.Forms.Label label_BlueBG;
    private System.Windows.Forms.Label label_CarA;
    private System.Windows.Forms.Label labelAScore;
    private System.Windows.Forms.Label label_CarB;
    private System.Windows.Forms.Label labelBScore;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Button button_set;
    private System.Windows.Forms.Button buttonFoul;
    private System.Windows.Forms.Button buttonEnd;
    private System.Windows.Forms.Button buttonSetStation;
    private System.Windows.Forms.Button buttonRestart;
}