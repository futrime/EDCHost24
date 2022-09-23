using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using OpenCvSharp;

namespace EdcHost;
public partial class SetWindow : Form
{
    private MyFlags _flags;
    private Game _game;
    private Tracker _tracker;
    public SetWindow(ref MyFlags flags, ref Game game, Tracker tracker)
    {
        InitializeComponent();
        _flags = flags;
        _game = game;
        _tracker = tracker;
        nudHue1L.Value = flags.configs.hue1Lower;
        nudHue1H.Value = flags.configs.hue1Upper;
        nudHue2L.Value = flags.configs.hue2Lower;
        nudHue2H.Value = flags.configs.hue2Upper;
        nudSat1L.Value = flags.configs.saturation1Lower;
        nudSat2L.Value = flags.configs.saturation2Lower;
        nudValueL.Value = flags.configs.valueLower;
        nudAreaL.Value = flags.configs.areaLower;
        checkBox_DebugMode.Checked = game.DebugMode;

        cbPorts1.Items.Clear();
        cbPorts1.Items.Add("(None)");
        foreach (string port in _tracker.validPorts)
        {
            cbPorts1.Items.Add(port);
        }

        cbPorts2.Items.Clear();
        cbPorts2.Items.Add("(None)");
        foreach (string port in _tracker.validPorts)
        {
            cbPorts2.Items.Add(port);
        }


        if (_tracker.serial1 != null && _tracker.serial1.IsOpen)
        {
            cbPorts1.Text = _tracker.serial1.PortName;
            nudBaudRate.Value = _tracker.serial1.BaudRate;
        }
        else
        {
            cbPorts1.Text = "(None)";
            nudBaudRate.Value = 115200;
        }
        if (_tracker.serial2 != null && _tracker.serial2.IsOpen)
        {
            cbPorts2.Text = _tracker.serial2.PortName;
            nudBaudRate.Value = _tracker.serial2.BaudRate;
        }
        else
        {
            cbPorts2.Text = "(None)";
            nudBaudRate.Value = 115200;
        }

        BringToFront();
    }


    private void nudHue1L_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.hue1Lower = (int)nudHue1L.Value;
    }

    private void nudHue1H_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.hue1Upper = (int)nudHue1H.Value;
    }

    private void nudHue2L_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.hue2Lower = (int)nudHue2L.Value;
    }

    private void nudHue2H_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.hue2Upper = (int)nudHue2H.Value;
    }


    private void nudSat1L_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.saturation1Lower = (int)nudSat1L.Value;
    }

    private void nudSat2L_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.saturation2Lower = (int)nudSat2L.Value;
    }

    private void nudValueL_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.valueLower = (int)nudValueL.Value;
    }

    private void nudAreaL_ValueChanged(object sender, EventArgs e)
    {
        _flags.configs.areaLower = (int)nudAreaL.Value;
    }

    private void checkBox_DebugMode_CheckedChanged(object sender, EventArgs e)
    {
        _game.DebugMode = checkBox_DebugMode.Checked;
    }

    private void button_ConfigSave_Click(object sender, EventArgs e)
    {
        string arrStr = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", _flags.configs.hue1Lower,
                                    _flags.configs.hue1Upper, _flags.configs.hue2Lower, _flags.configs.hue2Upper,
                                    _flags.configs.saturation1Lower, _flags.configs.saturation2Lower,
                                    _flags.configs.valueLower, _flags.configs.areaLower);
        File.WriteAllText(@"Data\data.txt", arrStr);
    }

    private void button_ConfigLoad_Click(object sender, EventArgs e)
    {
        if (File.Exists(@"Data\data.txt"))
        {
            FileStream fsRead = new FileStream(@"Data\data.txt", FileMode.Open);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            int r = fsRead.Read(heByte, 0, heByte.Length);
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] str = myStr.Split(' ');
            nudHue1L.Value = (_flags.configs.hue1Lower = Convert.ToInt32(str[0]));
            nudHue1H.Value = (_flags.configs.hue1Upper = Convert.ToInt32(str[1]));
            nudHue2L.Value = (_flags.configs.hue2Lower = Convert.ToInt32(str[2]));
            nudHue2H.Value = (_flags.configs.hue2Upper = Convert.ToInt32(str[3]));
            nudSat1L.Value = (_flags.configs.saturation1Lower = Convert.ToInt32(str[4]));
            nudSat2L.Value = (_flags.configs.saturation2Lower = Convert.ToInt32(str[5]));
            nudValueL.Value = (_flags.configs.valueLower = Convert.ToInt32(str[6]));
            nudAreaL.Value = (_flags.configs.areaLower = Convert.ToInt32(str[7]));
            fsRead.Close();
        }
    }

    private void nudCapture_ValueChanged(object sender, EventArgs e)
    {
        VideoCapture tmpCamture = new VideoCapture((int)nudCapture.Value);
        if (tmpCamture.IsOpened() && tmpCamture.FrameWidth > 0 && tmpCamture.FrameHeight > 0)
        {
            tmpCamture.Release();
            if (_tracker.capture.IsOpened())
                _tracker.capture.Release();
            _tracker.capture.Open((int)nudCapture.Value);
            _tracker.flags.cameraSize.Width = _tracker.capture.FrameWidth;
            _tracker.flags.cameraSize.Height = _tracker.capture.FrameHeight;
            _tracker.coordCvt = new CoordinateConverter(_tracker.flags);
        }
    }

    private void SetWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _tracker.flags.showMask = false;
    }

    private void checkBox_ShowMask_CheckedChanged(object sender, EventArgs e)
    {
        _tracker.flags.showMask = checkBox_ShowMask.Checked;
    }

    private void nudBaudRate_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            if (_tracker.serial1 != null)
            {
                if (_tracker.serial1.IsOpen)
                    _tracker.serial1.Close();
                _tracker.serial1.BaudRate = (int)nudBaudRate.Value;
                _tracker.serial1.Open();
            }
            else
            {
                _tracker.serial1 = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker.serial1.Open();
            }
        }
        catch (UnauthorizedAccessException)
        {

        }
        try
        {
            if (_tracker.serial2 != null)
            {
                if (_tracker.serial2.IsOpen)
                    _tracker.serial2.Close();
                _tracker.serial2.BaudRate = (int)nudBaudRate.Value;
                _tracker.serial2.Open();
            }
            else
            {
                _tracker.serial2 = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker.serial2.Open();
            }
        }
        catch (UnauthorizedAccessException)
        {

        }
    }

    private void cbPorts2_TextChanged(object sender, EventArgs e)
    {
        if (cbPorts2.Text == "(None)")
        {
            if (_tracker.serial2 != null)
            {
                if (_tracker.serial2.IsOpen)
                    _tracker.serial2.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker.serial2 != null)
                {
                    if (_tracker.serial2.IsOpen)
                        _tracker.serial2.Close();
                    _tracker.serial2.PortName = cbPorts2.Text;
                    _tracker.serial2.Open();
                }
                else
                {
                    _tracker.serial2 = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker.serial2.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker.serial2 = null;
            }
        }
    }

    private void cbPorts1_TextChanged(object sender, EventArgs e)
    {
        if (cbPorts1.Text == "(None)")
        {
            if (_tracker.serial1 != null)
            {
                if (_tracker.serial1.IsOpen)
                    _tracker.serial1.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker.serial1 != null)
                {
                    if (_tracker.serial1.IsOpen)
                        _tracker.serial1.Close();
                    _tracker.serial1.PortName = cbPorts1.Text;
                    _tracker.serial1.Open();
                }
                else
                {
                    _tracker.serial1 = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker.serial1.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker.serial1 = null;
            }
        }
    }

    private void SetWindow_Load(object sender, EventArgs e)
    {

    }

    private void cbPorts2_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void cbPorts1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void lblPort1_Click(object sender, EventArgs e)
    {

    }

    private void lblPort2_Click(object sender, EventArgs e)
    {

    }

}