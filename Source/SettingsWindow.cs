using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EdcHost;

public partial class SettingsWindow : Form
{
    #region Parameters

    private const string DefaultConfigFilePath = @"Config.yml";

    #endregion

    #region Static properties and fields

    private static readonly IDeserializer YamlDeserializer =
        new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly ISerializer YamlSerializer =
        new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .DisableAliases()
        .Build();

    #endregion


    #region Private fields

    private ConfigType _config;
    private string _configFilePath = SettingsWindow.DefaultConfigFilePath;
    private MainWindow _mainWindow;

    #endregion


    public SettingsWindow(MainWindow mainWindow)
    {
        InitializeComponent();

        this._config = mainWindow.Config;
        this._mainWindow = mainWindow;

        this.SyncConfigToForm();

        // Update available serial ports.
        this.comboBoxSerialPortVehicleA.Items.Clear();
        this.comboBoxSerialPortVehicleB.Items.Clear();
        foreach (var serialPort in SerialPort.GetPortNames())
        {
            this.comboBoxSerialPortVehicleA.Items.Add(serialPort);
            this.comboBoxSerialPortVehicleB.Items.Add(serialPort);
        }

        // Hide the label showing "Applying..."
        this.labelApplying.Hide();
    }

    private void ApplyConfig()
    {
        this._mainWindow.Config = this._config;

        // Apply the camera configurations
        var camera = this._mainWindow.Camera;
        this._mainWindow.Camera = null;
        camera.Release();
        camera = new VideoCapture();

        camera.Open(this._mainWindow.Config.Camera);

        this._mainWindow.CameraFrameSize = new OpenCvSharp.Size(
            camera.FrameWidth,
            camera.FrameHeight
        );
        this._mainWindow.CoordinateConverter = new CoordinateConverter(
            cameraFrameSize: this._mainWindow.CameraFrameSize,
            monitorFrameSize: this._mainWindow.MonitorFrameSize,
            courtSize: this._mainWindow.CourtSize,
            calibrationCorners: this._mainWindow.CoordinateConverter.CalibrationCorners
        );

        this._mainWindow.Camera = camera;

        // Apply the vehicle specific configurations
        foreach (var vehicleConfigPair in this._mainWindow.Config.Vehicles)
        {
            var camp = vehicleConfigPair.Key;
            var vehicleConfig = vehicleConfigPair.Value;

            // Apply the locator configurations.
            var locatorConfig = vehicleConfig.Locator;
            this._mainWindow.LocatorDict[camp] = new Locator(
                config: locatorConfig,
                showMask: vehicleConfig.ShowMask
            );

            // Apply the serial port configurations.
            // If the serial port is open, close it.
            if (
                this._mainWindow.SerialPortDict[camp] != null &&
                this._mainWindow.SerialPortDict[camp].IsOpen
            )
            {
                this._mainWindow.SerialPortDict[camp].Close();
            }
            // The port name should not be empty.
            if (vehicleConfig.SerialPort != "")
            {
                this._mainWindow.SerialPortDict[camp] = new SerialPort(
                    portName: vehicleConfig.SerialPort,
                    baudRate: vehicleConfig.Baudrate
                );
                try
                {
                    this._mainWindow.SerialPortDict[camp].Open();
                }
                catch (System.Exception)
                {
                    MessageBox.Show(
                        "Cannot open the serial port.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    this._mainWindow.SerialPortDict[camp] = null;
                }
                this._mainWindow.SerialPortDict[camp].ReadTimeout = 10;
            }
        }

        // Re-enable the apply button.
        if (this.buttonApply.InvokeRequired)
        {
            Action safeWrite = delegate
            {
                this.buttonApply.Enabled = true;
            };
            this.buttonApply.Invoke(safeWrite);
        }
        else
        {
            this.buttonApply.Enabled = true;
        }

        // Hide the applying label.
        if (this.labelApplying.InvokeRequired)
        {
            Action safeWrite = delegate
            {
                this.labelApplying.Hide();
            };
            this.labelApplying.Invoke(safeWrite);
        }
        else
        {
            this.labelApplying.Hide();
        }
    }

    private void LoadConfig()
    {
        if (!File.Exists(this._configFilePath))
        {
            MessageBox.Show(
                text: "No configuration file is found!",
                caption: "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var yaml = File.ReadAllText(this._configFilePath);
        this._config = SettingsWindow.YamlDeserializer.Deserialize<ConfigType>(yaml);
    }

    private void SaveConfig()
    {
        var yaml = SettingsWindow.YamlSerializer.Serialize(this._config);
        File.WriteAllText(this._configFilePath, yaml);
    }

    private void SyncFormToConfig()
    {
        try
        {
            this._config = new ConfigType
            {
                Vehicles = new Dictionary<CampType, ConfigType.PerVehicleConfigType> {
                {
                    CampType.A,
                    new ConfigType.PerVehicleConfigType
                    {
                        Locator = new LocatorConfigType
                        {
                            Hue = (
                                (int)this.numericUpDownHueLowerVehicleA.Value,
                                (int)this.numericUpDownHueUpperVehicleA.Value
                            ),
                            Saturation = (
                                (int)this.numericUpDownSaturationLowerVehicleA.Value,
                                (int)this.numericUpDownSaturationUpperVehicleA.Value
                            ),
                            Value = (
                                (int)this.numericUpDownValueLowerVehicleA.Value,
                                (int)this.numericUpDownValueUpperVehicleA.Value
                            ),
                            MinArea = this.numericUpDownMinimumAreaVehicleA.Value
                        },
                        ShowMask = this.checkBoxShowMaskVehicleA.Checked,
                        SerialPort = this.comboBoxSerialPortVehicleA.Text,
                        Baudrate = Convert.ToInt32(this.comboBoxBaudrateVehicleA.Text)
                    }
                },
                {
                    CampType.B,
                    new ConfigType.PerVehicleConfigType
                    {
                        Locator = new LocatorConfigType
                        {
                            Hue = (
                                (int)this.numericUpDownHueLowerVehicleB.Value,
                                (int)this.numericUpDownHueUpperVehicleB.Value
                            ),
                            Saturation = (
                                (int)this.numericUpDownSaturationLowerVehicleB.Value,
                                (int)this.numericUpDownSaturationUpperVehicleB.Value
                            ),
                            Value = (
                                (int)this.numericUpDownValueLowerVehicleB.Value,
                                (int)this.numericUpDownValueUpperVehicleB.Value
                            ),
                            MinArea = this.numericUpDownMinimumAreaVehicleB.Value
                        },
                        ShowMask = this.checkBoxShowMaskVehicleB.Checked,
                        SerialPort = this.comboBoxSerialPortVehicleB.Text,
                        Baudrate = Convert.ToInt32(this.comboBoxBaudrateVehicleB.Text)
                    }
                }
            },
                Camera = this.comboBoxCamera.SelectedIndex
            };
        }
        catch (System.FormatException)
        {
            MessageBox.Show(
                "Some parameters are invalid.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void SyncConfigToForm()
    {
        this.numericUpDownHueLowerVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Hue.Min;
        this.numericUpDownHueUpperVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Hue.Max;
        this.numericUpDownSaturationLowerVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Saturation.Min;
        this.numericUpDownSaturationUpperVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Saturation.Max;
        this.numericUpDownValueLowerVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Value.Min;
        this.numericUpDownValueUpperVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.Value.Max;
        this.numericUpDownMinimumAreaVehicleA.Value =
            this._config.Vehicles[CampType.A].Locator.MinArea;
        this.checkBoxShowMaskVehicleA.Checked =
            this._config.Vehicles[CampType.A].ShowMask;
        this.comboBoxSerialPortVehicleA.Text =
            this._config.Vehicles[CampType.A].SerialPort;
        this.comboBoxBaudrateVehicleA.Text =
            this._config.Vehicles[CampType.A].Baudrate.ToString();

        this.numericUpDownHueLowerVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Hue.Min;
        this.numericUpDownHueUpperVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Hue.Max;
        this.numericUpDownSaturationLowerVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Saturation.Min;
        this.numericUpDownSaturationUpperVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Saturation.Max;
        this.numericUpDownValueLowerVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Value.Min;
        this.numericUpDownValueUpperVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.Value.Max;
        this.numericUpDownMinimumAreaVehicleB.Value =
            this._config.Vehicles[CampType.B].Locator.MinArea;
        this.checkBoxShowMaskVehicleB.Checked =
            this._config.Vehicles[CampType.B].ShowMask;
        this.comboBoxSerialPortVehicleB.Text =
            this._config.Vehicles[CampType.B].SerialPort;
        this.comboBoxBaudrateVehicleB.Text =
            this._config.Vehicles[CampType.B].Baudrate.ToString();

        this.Refresh();
    }

    private void UpdateAvailableCameras()
    {
        bool isLastCameraWorking = true;
        int cameraPort = 0;
        while (isLastCameraWorking)
        {
            var camera = new VideoCapture(cameraPort);

            // Break if the last camera is not working.
            if (!camera.IsOpened())
            {
                break;
            }

            // To ensure the thread safety
            if (this.comboBoxCamera.InvokeRequired)
            {
                Action safeWrite = delegate
                {
                    this.comboBoxCamera.Items.Add(cameraPort);
                    if (this.comboBoxCamera.Items.Count > this._config.Camera)
                    {
                        this.comboBoxCamera.SelectedIndex = this._config.Camera;
                    }
                    else
                    {
                        this.comboBoxCamera.SelectedIndex = 0;
                    }
                };
                this.comboBoxCamera.Invoke(safeWrite);
            }
            else
            {
                this.comboBoxCamera.Items.Add(cameraPort);
                if (this.comboBoxCamera.Items.Count > this._config.Camera)
                {
                    this.comboBoxCamera.SelectedIndex = this._config.Camera;
                }
                else
                {
                    this.comboBoxCamera.SelectedIndex = 0;
                }
            }

            ++cameraPort;
        }

        // Hide the notice label when done.
        if (this.labelLoading.InvokeRequired)
        {
            Action safeWrite = delegate
            {
                this.labelLoading.Hide();
            };
            this.labelLoading.Invoke(safeWrite);
        }
        else
        {
            this.labelLoading.Hide();
        }

        // Re-enable the apply button.
        if (this.buttonApply.InvokeRequired)
        {
            Action safeWrite = delegate
            {
                this.buttonApply.Enabled = true;
            };
            this.buttonApply.Invoke(safeWrite);
        }
        else
        {
            this.buttonApply.Enabled = true;
        }

        // Enable the camera selection box.
        if (this.comboBoxCamera.InvokeRequired)
        {
            Action safeWrite = delegate
            {
                this.comboBoxCamera.Enabled = true;
            };
            this.comboBoxCamera.Invoke(safeWrite);
        }
        else
        {
            this.comboBoxCamera.Enabled = true;
        }
    }

    #region Windows Forms event handlers

    private void buttonApply_Click(object sender, EventArgs e)
    {
        this.SyncFormToConfig();

        this.buttonApply.Enabled = false;
        this.labelApplying.Show();

        Thread thread = new Thread(this.ApplyConfig);
        thread.IsBackground = true;
        thread.Start();
    }

    private void buttonLoad_Click(object sender, EventArgs e)
    {
        this.LoadConfig();
        this.SyncConfigToForm();
    }

    private void buttonRevert_Click(object sender, EventArgs e)
    {
        this._config = MainWindow.DefaultConfig;
        this.SyncConfigToForm();
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
        this.SyncFormToConfig();
        this.SaveConfig();
    }

    private void SettingsWindow_Load(object sender, EventArgs e)
    {
        // Update available cameras asynchronously
        Thread thread = new Thread(this.UpdateAvailableCameras);
        thread.IsBackground = true;
        thread.Start();
    }

    #endregion
}

