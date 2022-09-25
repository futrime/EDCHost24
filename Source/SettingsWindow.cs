using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using OpenCvSharp;

namespace EdcHost;
public partial class SettingsWindow : Form
{
    private MyFlags _flags;
    private Game _game;
    private MainWindow _tracker;
    public SettingsWindow(ref MyFlags flags, ref Game game, MainWindow tracker)
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

        cbPorts1.Items.Clear();
        cbPorts1.Items.Add("(None)");
        foreach (string port in _tracker._availableSerialPortList)
        {
            cbPorts1.Items.Add(port);
        }

        cbPorts2.Items.Clear();
        cbPorts2.Items.Add("(None)");
        foreach (string port in _tracker._availableSerialPortList)
        {
            cbPorts2.Items.Add(port);
        }


        if (_tracker._serialPortCarA != null && _tracker._serialPortCarA.IsOpen)
        {
            cbPorts1.Text = _tracker._serialPortCarA.PortName;
            nudBaudRate.Value = _tracker._serialPortCarA.BaudRate;
        }
        else
        {
            cbPorts1.Text = "(None)";
            nudBaudRate.Value = 115200;
        }
        if (_tracker._serialPortCarB != null && _tracker._serialPortCarB.IsOpen)
        {
            cbPorts2.Text = _tracker._serialPortCarB.PortName;
            nudBaudRate.Value = _tracker._serialPortCarB.BaudRate;
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
            if (_tracker._camera.IsOpened())
                _tracker._camera.Release();
            _tracker._camera.Open((int)nudCapture.Value);
            _tracker._flags.cameraSize.Width = _tracker._camera.FrameWidth;
            _tracker._flags.cameraSize.Height = _tracker._camera.FrameHeight;
            _tracker._coordinateConverter = new CoordinateConverter(_tracker._flags);
        }
    }

    private void SetWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _tracker._flags.showMask = false;
    }

    private void checkBox_ShowMask_CheckedChanged(object sender, EventArgs e)
    {
        _tracker._flags.showMask = checkBox_ShowMask.Checked;
    }

    private void nudBaudRate_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            if (_tracker._serialPortCarA != null)
            {
                if (_tracker._serialPortCarA.IsOpen)
                    _tracker._serialPortCarA.Close();
                _tracker._serialPortCarA.BaudRate = (int)nudBaudRate.Value;
                _tracker._serialPortCarA.Open();
            }
            else
            {
                _tracker._serialPortCarA = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker._serialPortCarA.Open();
            }
        }
        catch (UnauthorizedAccessException)
        {

        }
        try
        {
            if (_tracker._serialPortCarB != null)
            {
                if (_tracker._serialPortCarB.IsOpen)
                    _tracker._serialPortCarB.Close();
                _tracker._serialPortCarB.BaudRate = (int)nudBaudRate.Value;
                _tracker._serialPortCarB.Open();
            }
            else
            {
                _tracker._serialPortCarB = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker._serialPortCarB.Open();
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
            if (_tracker._serialPortCarB != null)
            {
                if (_tracker._serialPortCarB.IsOpen)
                    _tracker._serialPortCarB.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker._serialPortCarB != null)
                {
                    if (_tracker._serialPortCarB.IsOpen)
                        _tracker._serialPortCarB.Close();
                    _tracker._serialPortCarB.PortName = cbPorts2.Text;
                    _tracker._serialPortCarB.Open();
                }
                else
                {
                    _tracker._serialPortCarB = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker._serialPortCarB.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker._serialPortCarB = null;
            }
        }
    }

    private void cbPorts1_TextChanged(object sender, EventArgs e)
    {
        if (cbPorts1.Text == "(None)")
        {
            if (_tracker._serialPortCarA != null)
            {
                if (_tracker._serialPortCarA.IsOpen)
                    _tracker._serialPortCarA.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker._serialPortCarA != null)
                {
                    if (_tracker._serialPortCarA.IsOpen)
                        _tracker._serialPortCarA.Close();
                    _tracker._serialPortCarA.PortName = cbPorts1.Text;
                    _tracker._serialPortCarA.Open();
                }
                else
                {
                    _tracker._serialPortCarA = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker._serialPortCarA.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker._serialPortCarA = null;
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