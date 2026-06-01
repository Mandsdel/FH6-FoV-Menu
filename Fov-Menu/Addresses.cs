using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memory;

namespace Fov_Menu;

public class Addresses : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly Mem _mem = new()
    {
        SigScanTasks = Math.Max(1, Environment.ProcessorCount * (Environment.ProcessorCount / 2))
    };

    private bool _attached;
    private volatile bool _isUpdating;

    private UIntPtr _chaseMin;
    private UIntPtr _chaseMax;
    private UIntPtr _farChaseMin;
    private UIntPtr _farChaseMax;
    private UIntPtr _driverMin;
    private UIntPtr _driverMax;
    private UIntPtr _hoodMin;
    private UIntPtr _hoodMax;
    private UIntPtr _bumperMin;
    private UIntPtr _bumperMax;

    private Dictionary<string, UIntPtr> _addressMap = new(StringComparer.OrdinalIgnoreCase);

    public Addresses(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task OpenGameProcess(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_attached ? 1000 : 500, token);
                }
                catch (TaskCanceledException) { break; }

                var isGameProcessOpened = _mem.OpenProcess("ForzaHorizon6") || _mem.OpenProcess("ForzaHorizon5") || _mem.OpenProcess("ForzaHorizon4");

                if (!_attached && isGameProcessOpened)
                {
                    AddressesScan();
                    _attached = true;
                    Logger.LogInfo("Attached to Forza process and scanned addresses.");
                }
                else if (_attached && !isGameProcessOpened)
                {
                    _attached = false;
                    Logger.LogInfo("Detached from Forza process.");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("OpenGameProcess terminated with exception", ex);
        }
    }

    private void AddressesScan()
    {
        var bases1 = _mem.ScanForSig("90 40 CD CC 8C 40 1F 85 2B 3F 00 00 00 40").ToList();
        var bases2 = _mem.ScanForSig("CD CC 4C 3E 00 50 43 47 00 00 34 42 00 00 20").ToList();
        var foundBase3 = _mem.ScanForSig("CD ? 4C 3E ? ? ? 47 00 ? 34 ? 00 00 20 42 ? 00 A0").FirstOrDefault();

        if (!bases1.Any() || !bases2.Any() || foundBase3 == UIntPtr.Zero)
        {
            Logger.LogInfo("Signature scan did not find expected patterns; skipping address setup.");
            return;
        }

        var first1 = bases1.First();
        var last1 = bases1.Last();
        var first2 = bases2.First();
        var last2 = bases2.Last();

        _chaseMin = (UIntPtr)(first1.ToUInt64() - 10);
        _chaseMax = (UIntPtr)(first1.ToUInt64() - 10 + 4);
        _farChaseMin = (UIntPtr)(last1.ToUInt64() - 10);
        _farChaseMax = (UIntPtr)(last1.ToUInt64() - 10 + 4);
        _driverMin = (UIntPtr)(foundBase3.ToUInt64() - 0x20 - 4);
        _driverMax = (UIntPtr)(foundBase3.ToUInt64() - 0x20);
        _bumperMin = (UIntPtr)(first2.ToUInt64() - 0x20 - 4);
        _bumperMax = (UIntPtr)(first2.ToUInt64() - 0x20);
        _hoodMin = (UIntPtr)(last2.ToUInt64() - 0x20 - 4);
        _hoodMax = (UIntPtr)(last2.ToUInt64() - 0x20);

        _addressMap = new Dictionary<string, UIntPtr>(StringComparer.OrdinalIgnoreCase)
        {
            ["ChaseMin"] = _chaseMin,
            ["ChaseMax"] = _chaseMax,
            ["FarChaseMin"] = _farChaseMin,
            ["FarChaseMax"] = _farChaseMax,
            ["DriverMin"] = _driverMin,
            ["DriverMax"] = _driverMax,
            ["HoodMin"] = _hoodMin,
            ["HoodMax"] = _hoodMax,
            ["BumperMin"] = _bumperMin,
            ["BumperMax"] = _bumperMax
        };

        ReadValues();
    }

    private void ReadValues()
    {
        if (_isUpdating) return;
        _isUpdating = true;

        _mainWindow.Dispatcher.BeginInvoke((Action)delegate
        {
            try
            {
                if (_chaseMin != UIntPtr.Zero)
                    _mainWindow.ChaseMin.Value = Convert.ToDouble(_mem.ReadMemory<float>(_chaseMin));
                if (_chaseMax != UIntPtr.Zero)
                    _mainWindow.ChaseMax.Value = Convert.ToDouble(_mem.ReadMemory<float>(_chaseMax));
                if (_farChaseMin != UIntPtr.Zero)
                    _mainWindow.FarChaseMin.Value = Convert.ToDouble(_mem.ReadMemory<float>(_farChaseMin));
                if (_farChaseMax != UIntPtr.Zero)
                    _mainWindow.FarChaseMax.Value = Convert.ToDouble(_mem.ReadMemory<float>(_farChaseMax));
                if (_driverMin != UIntPtr.Zero)
                    _mainWindow.DriverMin.Value = Convert.ToDouble(_mem.ReadMemory<float>(_driverMin));
                if (_driverMax != UIntPtr.Zero)
                    _mainWindow.DriverMax.Value = Convert.ToDouble(_mem.ReadMemory<float>(_driverMax));
                if (_hoodMin != UIntPtr.Zero)
                    _mainWindow.HoodMin.Value = Convert.ToDouble(_mem.ReadMemory<float>(_hoodMin));
                if (_hoodMax != UIntPtr.Zero)
                    _mainWindow.HoodMax.Value = Convert.ToDouble(_mem.ReadMemory<float>(_hoodMax));
                if (_bumperMin != UIntPtr.Zero)
                    _mainWindow.BumperMin.Value = Convert.ToDouble(_mem.ReadMemory<float>(_bumperMin));
                if (_bumperMax != UIntPtr.Zero)
                    _mainWindow.BumperMax.Value = Convert.ToDouble(_mem.ReadMemory<float>(_bumperMax));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error reading memory values", ex);
            }
            finally
            {
                _isUpdating = false;
            }
        });
    }

    public void WriteValue(string buttonName, float value)
    {
        if (!_attached || !_addressMap.TryGetValue(buttonName, out var address) || address == UIntPtr.Zero)
            return;

        _mem.WriteMemory(address, value);
    }

    public void Dispose()
    {
        try
        {
            _attached = false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during Addresses.Dispose", ex);
        }
    }
}
