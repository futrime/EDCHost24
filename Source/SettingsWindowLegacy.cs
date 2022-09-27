using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using OpenCvSharp;

namespace EdcHost;

/// <summary>
/// A settings window
/// </summary>
public partial class SettingsWindowLegacy : Form
{
    private ConfigTypeLegacy _flags;
    private Game _game;
    private MainWindow _tracker;


    public SettingsWindowLegacy(ref ConfigTypeLegacy flags, ref Game game, MainWindow tracker)
    {
        InitializeComponent();

        this._flags = flags;
        this._game = game;
        this._tracker = tracker;

        this.nudHue1L.Value = flags.LocatorConfig.MinHueVehicleA;
        this.nudHue1H.Value = flags.LocatorConfig.MaxHueVehicleA;
        this.nudHue2L.Value = flags.LocatorConfig.MinHueVehicleB;
        this.nudHue2H.Value = flags.LocatorConfig.MaxHueVehicleB;
        this.nudSat1L.Value = flags.LocatorConfig.MinSaturationVehicleA;
        this.nudSat2L.Value = flags.LocatorConfig.MinSaturationVehicleB;
        this.nudValueL.Value = flags.LocatorConfig.valueLower;
        this.nudAreaL.Value = flags.LocatorConfig.MinArea;

        cbPorts1.Items.Clear();
        cbPorts1.Items.Add("(None)");
        foreach (string port in _tracker.AvailableSerialPortList)
        {
            cbPorts1.Items.Add(port);
        }

        cbPorts2.Items.Clear();
        cbPorts2.Items.Add("(None)");
        foreach (string port in _tracker.AvailableSerialPortList)
        {
            cbPorts2.Items.Add(port);
        }


        if (_tracker.SerialPortVehicleA != null && _tracker.SerialPortVehicleA.IsOpen)
        {
            cbPorts1.Text = _tracker.SerialPortVehicleA.PortName;
            nudBaudRate.Value = _tracker.SerialPortVehicleA.BaudRate;
        }
        else
        {
            cbPorts1.Text = "(None)";
            nudBaudRate.Value = 115200;
        }
        if (_tracker.SerialPortVehicleB != null && _tracker.SerialPortVehicleB.IsOpen)
        {
            cbPorts2.Text = _tracker.SerialPortVehicleB.PortName;
            nudBaudRate.Value = _tracker.SerialPortVehicleB.BaudRate;
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
        _flags.LocatorConfig.MinHueVehicleA = (int)nudHue1L.Value;
    }

    private void nudHue1H_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MaxHueVehicleA = (int)nudHue1H.Value;
    }

    private void nudHue2L_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MinHueVehicleB = (int)nudHue2L.Value;
    }

    private void nudHue2H_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MaxHueVehicleB = (int)nudHue2H.Value;
    }


    private void nudSat1L_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MinSaturationVehicleA = (int)nudSat1L.Value;
    }

    private void nudSat2L_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MinSaturationVehicleB = (int)nudSat2L.Value;
    }

    private void nudValueL_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.valueLower = (int)nudValueL.Value;
    }

    private void nudAreaL_ValueChanged(object sender, EventArgs e)
    {
        _flags.LocatorConfig.MinArea = (int)nudAreaL.Value;
    }

    private void button_ConfigSave_Click(object sender, EventArgs e)
    {
        string arrStr = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", _flags.LocatorConfig.MinHueVehicleA,
                                    _flags.LocatorConfig.MaxHueVehicleA, _flags.LocatorConfig.MinHueVehicleB, _flags.LocatorConfig.MaxHueVehicleB,
                                    _flags.LocatorConfig.MinSaturationVehicleA, _flags.LocatorConfig.MinSaturationVehicleB,
                                    _flags.LocatorConfig.valueLower, _flags.LocatorConfig.MinArea);
        File.WriteAllText(@"Data/data.txt", arrStr);
    }

    private void button_ConfigLoad_Click(object sender, EventArgs e)
    {
        if (File.Exists(@"Data/data.txt"))
        {
            FileStream fsRead = new FileStream(@"Data/data.txt", FileMode.Open);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            int r = fsRead.Read(heByte, 0, heByte.Length);
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] str = myStr.Split(' ');
            nudHue1L.Value = (_flags.LocatorConfig.MinHueVehicleA = Convert.ToInt32(str[0]));
            nudHue1H.Value = (_flags.LocatorConfig.MaxHueVehicleA = Convert.ToInt32(str[1]));
            nudHue2L.Value = (_flags.LocatorConfig.MinHueVehicleB = Convert.ToInt32(str[2]));
            nudHue2H.Value = (_flags.LocatorConfig.MaxHueVehicleB = Convert.ToInt32(str[3]));
            nudSat1L.Value = (_flags.LocatorConfig.MinSaturationVehicleA = Convert.ToInt32(str[4]));
            nudSat2L.Value = (_flags.LocatorConfig.MinSaturationVehicleB = Convert.ToInt32(str[5]));
            nudValueL.Value = (_flags.LocatorConfig.valueLower = Convert.ToInt32(str[6]));
            nudAreaL.Value = (_flags.LocatorConfig.MinArea = Convert.ToInt32(str[7]));
            fsRead.Close();
        }
    }

    private void nudCapture_ValueChanged(object sender, EventArgs e)
    {
        VideoCapture tmpCamture = new VideoCapture((int)nudCapture.Value);
        if (tmpCamture.IsOpened() && tmpCamture.FrameWidth > 0 && tmpCamture.FrameHeight > 0)
        {
            tmpCamture.Release();
            if (_tracker.Camera.IsOpened())
                _tracker.Camera.Release();
            _tracker.Camera.Open((int)nudCapture.Value);
            _tracker.Flags.CameraFrameSize.Width = _tracker.Camera.FrameWidth;
            _tracker.Flags.CameraFrameSize.Height = _tracker.Camera.FrameHeight;
            _tracker.CoordinateConverter = new CoordinateConverter(_tracker.Flags);
        }
    }

    private void SetWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _tracker.Flags.ShowMask = false;
    }

    private void checkBox_ShowMask_CheckedChanged(object sender, EventArgs e)
    {
        _tracker.Flags.ShowMask = checkBox_ShowMask.Checked;
    }

    private void nudBaudRate_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            if (_tracker.SerialPortVehicleA != null)
            {
                if (_tracker.SerialPortVehicleA.IsOpen)
                    _tracker.SerialPortVehicleA.Close();
                _tracker.SerialPortVehicleA.BaudRate = (int)nudBaudRate.Value;
                _tracker.SerialPortVehicleA.Open();
            }
            else
            {
                _tracker.SerialPortVehicleA = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker.SerialPortVehicleA.Open();
            }
        }
        catch (UnauthorizedAccessException)
        {

        }
        try
        {
            if (_tracker.SerialPortVehicleB != null)
            {
                if (_tracker.SerialPortVehicleB.IsOpen)
                    _tracker.SerialPortVehicleB.Close();
                _tracker.SerialPortVehicleB.BaudRate = (int)nudBaudRate.Value;
                _tracker.SerialPortVehicleB.Open();
            }
            else
            {
                _tracker.SerialPortVehicleB = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                _tracker.SerialPortVehicleB.Open();
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
            if (_tracker.SerialPortVehicleB != null)
            {
                if (_tracker.SerialPortVehicleB.IsOpen)
                    _tracker.SerialPortVehicleB.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker.SerialPortVehicleB != null)
                {
                    if (_tracker.SerialPortVehicleB.IsOpen)
                        _tracker.SerialPortVehicleB.Close();
                    _tracker.SerialPortVehicleB.PortName = cbPorts2.Text;
                    _tracker.SerialPortVehicleB.Open();
                }
                else
                {
                    _tracker.SerialPortVehicleB = new SerialPort(cbPorts2.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker.SerialPortVehicleB.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker.SerialPortVehicleB = null;
            }
        }
    }

    private void cbPorts1_TextChanged(object sender, EventArgs e)
    {
        if (cbPorts1.Text == "(None)")
        {
            if (_tracker.SerialPortVehicleA != null)
            {
                if (_tracker.SerialPortVehicleA.IsOpen)
                    _tracker.SerialPortVehicleA.Close();
            }
        }
        else
        {
            try
            {
                if (_tracker.SerialPortVehicleA != null)
                {
                    if (_tracker.SerialPortVehicleA.IsOpen)
                        _tracker.SerialPortVehicleA.Close();
                    _tracker.SerialPortVehicleA.PortName = cbPorts1.Text;
                    _tracker.SerialPortVehicleA.Open();
                }
                else
                {
                    _tracker.SerialPortVehicleA = new SerialPort(cbPorts1.Text, (int)nudBaudRate.Value, Parity.None, 8, StopBits.One);
                    _tracker.SerialPortVehicleA.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("串口设置无效！");
                _tracker.SerialPortVehicleA = null;
            }
        }
    }
}