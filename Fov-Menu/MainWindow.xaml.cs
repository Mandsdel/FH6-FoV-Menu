using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
namespace Fov_Menu;

public partial class MainWindow
{
    private readonly Addresses? _addresses;
    private const string FovSettingsFile = "fov_settings.json";


    public MainWindow()
    {
        InitializeComponent();
        _addresses = new Addresses(this);
        AutoLoadFov();
        _ = Task.Run(_addresses.OpenGameProcess);
    }



    private void AutoLoadFov()
    {
        var settings = LoadFovSettings();
        if (settings != null)
            ApplyFovSettings(settings);
    }

    private FovSettings? LoadFovSettings()
    {
        if (!File.Exists(FovSettingsFile))
            return null;
        try
        {
            var json = File.ReadAllText(FovSettingsFile);
            return JsonSerializer.Deserialize<FovSettings>(json);
        }
        catch { return null; }
    }

    private void ApplyFovSettings(FovSettings settings)
    {
        ChaseMin.Value = settings.ChaseMin;
        ChaseMax.Value = settings.ChaseMax;
        FarChaseMin.Value = settings.FarChaseMin;
        FarChaseMax.Value = settings.FarChaseMax;
        DriverMin.Value = settings.DriverMin;
        DriverMax.Value = settings.DriverMax;
        HoodMin.Value = settings.HoodMin;
        HoodMax.Value = settings.HoodMax;
        BumperMin.Value = settings.BumperMin;
        BumperMax.Value = settings.BumperMax;
    }

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void NumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_addresses == null)
        {
            return;
        }

        var numericUpDown = (MahApps.Metro.Controls.NumericUpDown)sender;

        if (numericUpDown.Value == null)
        {
            return;
        }

        var numericUpDownName = numericUpDown.Name;
        var value = Convert.ToSingle(numericUpDown.Value);
        _addresses.WriteValue(numericUpDownName, value);
    }

    private void SaveFov_Click(object sender, RoutedEventArgs e)
    {
        var settings = new FovSettings
        {
            ChaseMin = ChaseMin.Value ?? 0,
            ChaseMax = ChaseMax.Value ?? 0,
            FarChaseMin = FarChaseMin.Value ?? 0,
            FarChaseMax = FarChaseMax.Value ?? 0,
            DriverMin = DriverMin.Value ?? 0,
            DriverMax = DriverMax.Value ?? 0,
            HoodMin = HoodMin.Value ?? 0,
            HoodMax = HoodMax.Value ?? 0,
            BumperMin = BumperMin.Value ?? 0,
            BumperMax = BumperMax.Value ?? 0
        };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FovSettingsFile, json);
        MessageBox.Show("FOV settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LoadFov_Click(object sender, RoutedEventArgs e)
    {
        var settings = LoadFovSettings();
        if (settings == null)
        {
            MessageBox.Show("No saved FOV settings found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        ApplyFovSettings(settings);
        MessageBox.Show("FOV settings loaded!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
}