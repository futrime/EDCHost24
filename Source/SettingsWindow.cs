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
        this.comboBoxVehicleASerialPort.Items.Clear();
        this.comboBoxVehicleBSerialPort.Items.Clear();
        foreach (var serialPort in SerialPort.GetPortNames())
        {
            this.comboBoxVehicleASerialPort.Items.Add(serialPort);
            this.comboBoxVehicleBSerialPort.Items.Add(serialPort);
        }
    }

    private void ApplyConfig()
    {
        this._mainWindow.Config = this._config;

        // Apply the camera configurations
        this._mainWindow.Camera.Release();
        this._mainWindow.Camera.Open(this._mainWindow.Config.Camera);

        // Apply the locator configurations
        foreach (var vehicleConfigPair in this._mainWindow.Config.Vehicles)
        {
            var camp = vehicleConfigPair.Key;
            var vehicleConfig = vehicleConfigPair.Value;
            var locatorConfig = vehicleConfig.Locator;
            this._mainWindow.LocatorDict[camp] = new Locator(
                config: locatorConfig,
                showMask: vehicleConfig.ShowMask
            );
        }
    }

    private void LoadConfig()
    {
        if (!File.Exists(this._configFilePath))
        {
            MessageBox.Show(
                text: "No configuration file found!",
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
        this._config = new ConfigType
        {
            Vehicles = new Dictionary<CampType, ConfigType.PerVehicleConfigType> {
                {
                    CampType.A,
                    new ConfigType.PerVehicleConfigType
                    {
                        Locator = new Locator.ConfigType
                        {
                            Hue = (
                                (int)this.numericUpDownVehicleAHueLower.Value,
                                (int)this.numericUpDownVehicleAHueUpper.Value
                            ),
                            Saturation = (
                                (int)this.numericUpDownVehicleASaturationLower.Value,
                                (int)this.numericUpDownVehicleASaturationUpper.Value
                            ),
                            Value = (
                                (int)this.numericUpDownVehicleAValueLower.Value,
                                (int)this.numericUpDownVehicleAValueUpper.Value
                            ),
                            MinArea = this.numericUpDownVehicleAMinimumArea.Value
                        },
                        ShowMask = this.checkBoxVehicleAShowMask.Checked,
                        SerialPort = this.comboBoxVehicleASerialPort.Text,
                        Baudrate = Convert.ToInt32(this.comboBoxVehicleABaudrate.Text)
                    }
                },
                {
                    CampType.B,
                    new ConfigType.PerVehicleConfigType
                    {
                        Locator = new Locator.ConfigType
                        {
                            Hue = (
                                (int)this.numericUpDownVehicleBHueLower.Value,
                                (int)this.numericUpDownVehicleBHueUpper.Value
                            ),
                            Saturation = (
                                (int)this.numericUpDownVehicleBSaturationLower.Value,
                                (int)this.numericUpDownVehicleBSaturationUpper.Value
                            ),
                            Value = (
                                (int)this.numericUpDownVehicleBValueLower.Value,
                                (int)this.numericUpDownVehicleBValueUpper.Value
                            ),
                            MinArea = this.numericUpDownVehicleBMinimumArea.Value
                        },
                        ShowMask = this.checkBoxVehicleBShowMask.Checked,
                        SerialPort = this.comboBoxVehicleBSerialPort.Text,
                        Baudrate = Convert.ToInt32(this.comboBoxVehicleBBaudrate.Text)
                    }
                }
            },
            Camera = Convert.ToInt32(this.comboBoxCamera.Text)
        };
    }

    private void SyncConfigToForm()
    {
        this.numericUpDownVehicleAHueLower.Value =
            this._config.Vehicles[CampType.A].Locator.Hue.Min;
        this.numericUpDownVehicleAHueUpper.Value =
            this._config.Vehicles[CampType.A].Locator.Hue.Max;
        this.numericUpDownVehicleASaturationLower.Value =
            this._config.Vehicles[CampType.A].Locator.Saturation.Min;
        this.numericUpDownVehicleASaturationUpper.Value =
            this._config.Vehicles[CampType.A].Locator.Saturation.Max;
        this.numericUpDownVehicleAValueLower.Value =
            this._config.Vehicles[CampType.A].Locator.Value.Min;
        this.numericUpDownVehicleAValueUpper.Value =
            this._config.Vehicles[CampType.A].Locator.Value.Max;
        this.numericUpDownVehicleAMinimumArea.Value =
            this._config.Vehicles[CampType.A].Locator.MinArea;
        this.checkBoxVehicleAShowMask.Checked =
            this._config.Vehicles[CampType.A].ShowMask;
        this.comboBoxVehicleASerialPort.Text =
            this._config.Vehicles[CampType.A].SerialPort;
        this.comboBoxVehicleABaudrate.Text =
            this._config.Vehicles[CampType.A].Baudrate.ToString();

        this.numericUpDownVehicleBHueLower.Value =
            this._config.Vehicles[CampType.B].Locator.Hue.Min;
        this.numericUpDownVehicleBHueUpper.Value =
            this._config.Vehicles[CampType.B].Locator.Hue.Max;
        this.numericUpDownVehicleBSaturationLower.Value =
            this._config.Vehicles[CampType.B].Locator.Saturation.Min;
        this.numericUpDownVehicleBSaturationUpper.Value =
            this._config.Vehicles[CampType.B].Locator.Saturation.Max;
        this.numericUpDownVehicleBValueLower.Value =
            this._config.Vehicles[CampType.B].Locator.Value.Min;
        this.numericUpDownVehicleBValueUpper.Value =
            this._config.Vehicles[CampType.B].Locator.Value.Max;
        this.numericUpDownVehicleBMinimumArea.Value =
            this._config.Vehicles[CampType.B].Locator.MinArea;
        this.checkBoxVehicleBShowMask.Checked =
            this._config.Vehicles[CampType.B].ShowMask;
        this.comboBoxVehicleBSerialPort.Text =
            this._config.Vehicles[CampType.B].SerialPort;
        this.comboBoxVehicleBBaudrate.Text =
            this._config.Vehicles[CampType.B].Baudrate.ToString();

        this.comboBoxCamera.Text =
            this._config.Camera.ToString();

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
                };
                this.comboBoxCamera.Invoke(safeWrite);
            }
            else
            {
                this.comboBoxCamera.Items.Add(cameraPort);
            }

            ++cameraPort;
        }
    }

    #region Windows Forms event handlers

    private void buttonApply_Click(object sender, EventArgs e)
    {
        this.SyncFormToConfig();
        Thread thread = new Thread(this.ApplyConfig);
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
        thread.Start();
    }

    #endregion
}

