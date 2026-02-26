using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Geni_View_SettingTool.Common;
using Geni_View_SettingTool.Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geni_View_SettingTool
{
    public partial class MainWindow : Window
    {
        string _settingFile = @"./";

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            try
            {
                _logger.Info("Application Startup");

                InitializeComponent();

                Load();

                DataGrid.DataContext = Global._dashboard;
                MQTTStatus.DataContext = Global._appViewModel;

                ((appViewModel)MQTTStatus.DataContext).ConnectionStatusChanged += CheckUI;

                CheckUI(this, false);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, "Error");
            }
        }

        // ── Avalonia: show a message box (async) ──────────────────────────────

        private async void ShowError(string message, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                                     Margin = new Thickness(0, 12, 0, 0) }
                    }
                }
            };
            // Wire OK button close
            ((StackPanel)dialog.Content!).Children
                .OfType<Button>().First().Click += (_, _) => dialog.Close();

            await dialog.ShowDialog(this);
        }

        private async Task<bool> ShowYesNoDialog(string message, string title)
        {
            bool result = false;

            var tcs = new TaskCompletionSource<bool>();
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Margin = new Thickness(0, 12, 0, 0),
                            Children =
                            {
                                new Button { Content = "Yes", Margin = new Thickness(4, 0) },
                                new Button { Content = "No",  Margin = new Thickness(4, 0) }
                            }
                        }
                    }
                }
            };

            var btns = ((StackPanel)((StackPanel)dialog.Content!).Children[1]).Children.OfType<Button>().ToList();
            btns[0].Click += (_, _) => { tcs.SetResult(true);  dialog.Close(); };
            btns[1].Click += (_, _) => { tcs.SetResult(false); dialog.Close(); };

            await dialog.ShowDialog(this);
            return await tcs.Task;
        }

        // ── Settings persistence ───────────────────────────────────────────────

        private void Save()
        {
            int.TryParse(Port.Text, out int port);

            Global._setting.ClientName        = ClientName.Text ?? "";
            Global._setting.BrokerIP          = IP.Text ?? "";
            Global._setting.BrokerPort        = port;
            Global._setting.LocalSSID         = LocalWifiSSID.Text ?? "";
            Global._setting.LocalWifiPassword = LocalWifiPassword.Text ?? "";
            Global._setting.LocalBroker       = LocalBrokerIP.Text ?? "";

            Global._setting.Save(_settingFile);
        }

        private void Load()
        {
            Global._setting.Read(_settingFile);

            ClientName.Text       = Global._setting.ClientName;
            IP.Text               = Global._setting.BrokerIP;
            Port.Text             = Global._setting.BrokerPort.ToString();
            LocalWifiSSID.Text    = Global._setting.LocalSSID;
            LocalWifiPassword.Text = Global._setting.LocalWifiPassword;
            LocalBrokerIP.Text    = Global._setting.LocalBroker;
        }

        // ── Window events ──────────────────────────────────────────────────────

        protected override void OnClosed(EventArgs e)
        {
            _logger.Info("Application Closed");
            base.OnClosed(e);
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private async void Connectbtn_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                Save();

                List<string> topics = new List<string>
                {
                    Global._setting.Battery.status + "#",
                    Global._setting.Battery.wifi,
                    Global._setting.Battery.wifiResult + "#"
                };

                Global._mQTTHelper = new MQTTHelper();

                Global._mQTTHelper.Setting(
                    Global._setting.BrokerIP,
                    Global._setting.BrokerPort,
                    Global._setting.ClientName,
                    Global._setting.LocalBrokerAccount,
                    Global._setting.LocalBrokerPassword,
                    topics);

                var ret = Global._mQTTHelper.Connect();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void Disconnect_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var ret = Global._mQTTHelper.Disconnect();
                Global._devices.Clear();
                Global._dashboard.AddAndClear(new List<Device>());
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private async void Set_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (await CheckLocalSettingAsync())
                {
                    Save();
                    SendCommand();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void SelectALL_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid.SelectAll();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void CancleALL_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid.UnselectAll();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            Global._devices.Clear();
            Global._dashboard.AddAndClear(new List<Device>());
        }

        private void CmdTest_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                TestSendCommand();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void ResultTest_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                TestResult();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private void ClearTest_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                TestClear();
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        private async void ImportList_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Avalonia file picker (replaces Microsoft.Win32.OpenFileDialog)
                var topLevel = TopLevel.GetTopLevel(this)!;
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Select file",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } },
                            new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
                        }
                    });

                if (files.Count > 0)
                {
                    string filename = files[0].Path.LocalPath;

                    CSVHelper cSVHelper = new CSVHelper();
                    var str = cSVHelper.Read(filename);

                    foreach (var item in str)
                    {
                        Device device = new Device { SN = item };

                        if (Global._devices.ContainsKey(item))
                            Global._devices[device.SN] = device;
                        else
                            Global._devices.TryAdd(device.SN, device);
                    }

                    Global._dashboard.AddAndClear(Global._devices.Values.OrderBy(x => x.SN).ToList());
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        // ── UI state helper ────────────────────────────────────────────────────

        private void CheckUI(object? sender, bool isConnected)
        {
            // Avalonia UI thread dispatch
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ImportList.IsEnabled  = isConnected;
                SelectALL.IsEnabled   = isConnected;
                CancleALL.IsEnabled   = isConnected;
                Set.IsEnabled         = isConnected;
                CmdTest.IsEnabled     = isConnected;
                ResultTest.IsEnabled  = isConnected;
                ClearTest.IsEnabled   = isConnected;
                Clear.IsEnabled       = isConnected;

                Connectbtn.IsEnabled  = !isConnected;
                Disconnect.IsEnabled  = isConnected;
            });
        }

        private async Task<bool> CheckLocalSettingAsync()
        {
            bool result = true;

            if (string.IsNullOrEmpty(LocalWifiSSID.Text)
                || string.IsNullOrEmpty(LocalWifiPassword.Text)
                || string.IsNullOrEmpty(LocalBrokerIP.Text))
            {
                result = await ShowYesNoDialog("A setting is empty. Confirm to continue?", "Warning");
            }

            return result;
        }

        // ── MQTT publish helpers ───────────────────────────────────────────────

        private void SendCommand()
        {
            var items = DataGrid.SelectedItems.OfType<Device>().ToList();

            Task.Run(async () =>
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        string topic = MQTTTopic.GetLocalSetting(item.SN);

                        LocalSetting setting = new LocalSetting
                        {
                            ID            = item.SN,
                            Cmd           = "LocalSetting",
                            SSID          = Global._setting.LocalSSID,
                            PWD           = Global._setting.LocalWifiPassword,
                            Broker        = Global._setting.LocalBroker,
                            BrokerAccount = Global._setting.LocalBrokerAccount,
                            BrokerPWD     = Global._setting.LocalBrokerPassword,
                        };
                        string data = JsonConvert.SerializeObject(setting);

                        await Global._mQTTHelper.PublishAsync(topic, data);

                        Thread.Sleep(50);
                    }
                }
            });
        }

        private void TestSendCommand()
        {
            Task.Run(async () =>
            {
                int count = 4;
                TestLocalSettingAsync(count);
                TestOTA(count);
                TestDeviceStatus(count);
                TestNTP(count);
                TestLogRate(count);
                TestParameter(count);
            });
        }

        private void TestResult()
        {
            Task.Run(async () =>
            {
                int count = 100;
                TestLocalSettingResult(count);
                TestOTAResult(count);
                TestNTPResult(count);
                TestLogRateResult(count);
                TestParameterResult(count);
            });
        }

        private void TestClear()
        {
            Task.Run(async () =>
            {
                for (int i = 1; i < 100; i++)
                {
                    var setting = new LocalSettingResult { ID = i.ToString("0000") };
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/result/{setting.ID}", "");
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{setting.ID}", "");
                    Thread.Sleep(50);
                }

                int count = 4;
                TestLocalSettingAsync(count, true);
                TestOTA(count, true);
                TestDeviceStatus(count, true);
                TestNTP(count, true);
                TestLogRate(count, true);
                TestParameter(count, true);

                TestLocalSettingResult(count, true);
                TestOTAResult(count, true);
                TestNTPResult(count, true);
                TestLogRateResult(count, true);
                TestParameterResult(count, true);
            });
        }

        private async Task TestLocalSettingAsync(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var setting = new LocalSetting
                {
                    ID            = i.ToString("0000"),
                    Cmd           = "LocalSetting",
                    SSID          = Global._setting.LocalSSID,
                    PWD           = Global._setting.Password,
                    Broker        = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD     = Global._setting.LocalBrokerPassword,
                };
                string data = JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{setting.ID}", clear ? "" : data);
                Thread.Sleep(50);
            }
        }

        private async Task TestOTA(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var setting = new OTA
                {
                    ID  = i.ToString("0000"),
                    Cmd = "OTA",
                    URL = "http://192.168.10.222/Files/Device/displayboard.bin"
                };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"battery/ota/cmd/{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestDeviceStatus(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var setting = new DeviceStatus
                {
                    ID         = i.ToString("0000"),
                    Connection = "online",
                    Type       = "Battery",
                };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"device/status/{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestNTP(int count, bool clear = false)
        {
            bool ntpSwitch = false;
            for (int i = 1; i <= count; i++)
            {
                var setting = ntpSwitch
                    ? new NTP { ID = i.ToString("0000"), Cmd = "NTP", NTPURL = "time.windows.com", NTPUTC = "" }
                    : new NTP { ID = i.ToString("0000"), Cmd = "NTP", NTPURL = "",                 NTPUTC = DateTime.UtcNow.ToString("o") };
                ntpSwitch = !ntpSwitch;

                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"battery/ntp/cmd/{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestLogRate(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var setting = new LogRate { ID = i.ToString("0000"), Cmd = "LogRate", IntervalSec = 300 };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"battery/lograte/cmd/{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestParameter(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var config = new BatteryConfig
                {
                    ChargingMode    = "Parallel",
                    DischargingMode = "Sequential",
                    AlertSettings   = new Alertsettings
                    {
                        AlertType              = "All",
                        DisplayMode            = "Default",
                        SystemMode             = "Disabled",
                        LowBatteryAlertInterval = "",
                        LowBatteryAlertLevel    = "",
                    }
                };
                var setting = new BatterySetting { ID = i.ToString("0000"), Cmd = "BatteryPara", BatteryConfig = config };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"battery/para/cmd/{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestLocalSettingResult(int count, bool clear = false)
        {
            string topic = "battery/localsetting/result/";
            for (int i = 1; i <= count; i++)
            {
                var setting = new LocalSettingResult
                {
                    ID            = i.ToString("0000"),
                    Cmd           = "LocalSettingResult",
                    Result        = true,
                    SSID          = Global._setting.LocalSSID,
                    PWD           = Global._setting.Password,
                    Broker        = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD     = Global._setting.LocalBrokerPassword,
                };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestOTAResult(int count, bool clear = false)
        {
            string topic = "battery/ota/result/";
            for (uint i = 2156593311; i <= 2156593311 + (uint)count; i++)
            {
                var setting = new OTAResult
                {
                    ID     = i.ToString("0000"),
                    Cmd    = "OTAResult",
                    Result = true,
                    URL    = "http://192.168.10.222/Files/Device/displayboard.bin"
                };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestNTPResult(int count, bool clear = false)
        {
            string topic = "battery/ntp/result/";
            bool ntpSwitch = false;
            for (int i = 1; i <= count; i++)
            {
                var setting = ntpSwitch
                    ? new NTPResult { ID = i.ToString("0000"), Cmd = "NTPResult", Result = true, NTPURL = "time.windows.com", NTPUTC = "" }
                    : new NTPResult { ID = i.ToString("0000"), Cmd = "NTP",       Result = true, NTPURL = "",                 NTPUTC = DateTime.UtcNow.ToString("o") };
                ntpSwitch = !ntpSwitch;

                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestLogRateResult(int count, bool clear = false)
        {
            string topic = "battery/lograte/result/";
            for (int i = 1; i <= count; i++)
            {
                var setting = new LogRateResult { ID = i.ToString("0000"), Cmd = "LogRateResult", Result = true, IntervalSec = 300 };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                Thread.Sleep(50);
            }
        }

        private async Task TestParameterResult(int count, bool clear = false)
        {
            string topic = "battery/para/result/";
            for (int i = 1; i <= count; i++)
            {
                var config = new BatteryConfig
                {
                    ChargingMode    = "Parallel",
                    DischargingMode = "Sequential",
                    AlertSettings   = new Alertsettings
                    {
                        AlertType              = "All",
                        DisplayMode            = "Default",
                        SystemMode             = "Disabled",
                        LowBatteryAlertInterval = "",
                        LowBatteryAlertLevel    = "",
                    }
                };
                var setting = new BatterySettingResult { ID = i.ToString("0000"), Cmd = "BatteryParaResult", Result = true, BatteryConfig = config };
                string data = clear ? "" : JsonConvert.SerializeObject(setting);
                await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                Thread.Sleep(50);
            }
        }
    }
}
