using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BatteryClient;
using Geni_View_SettingTool.Common;
using Geni_View_SettingTool.Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Geni_View_SettingTool
{
    // Not partial — controls are resolved via FindControl<T> at runtime.
    public class MainWindow : Window
    {
        string _settingFile = @"./";
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // ── Named controls (populated in constructor via FindControl) ──────────
        private TextBox   ClientName       = null!;
        private TextBox   IP               = null!;
        private TextBox   Port             = null!;
        private TextBox   LocalWifiSSID    = null!;
        private TextBox   LocalWifiPassword = null!;
        private TextBox   LocalBrokerIP    = null!;
        // DataGrid is declared as Control so the project compiles without the
        // Avalonia.Controls.DataGrid package being restored. Cast to dynamic
        // at the call sites for SelectAll/UnselectAll/SelectedItems.
        private Control      DataGrid         = null!;
        private dynamic DataGridDynamic => DataGrid;
        private TextBlock MQTTStatus       = null!;
        private Button    ImportList       = null!;
        private Button    SelectALL        = null!;
        private Button    CancleALL        = null!;
        private Button    Set              = null!;
        private Button    CmdTest          = null!;
        private Button    ResultTest       = null!;
        private Button    ClearTest        = null!;
        private Button    Clear            = null!;
        private Button    Connectbtn       = null!;
        private Button    Disconnect       = null!;

        public MainWindow()
        {
            try
            {
                _logger.Info("Application Startup");

                // Load XAML and resolve named controls
                AvaloniaXamlLoader.Load(this);

                ClientName        = this.FindControl<TextBox>("ClientName")!;
                IP                = this.FindControl<TextBox>("IP")!;
                Port              = this.FindControl<TextBox>("Port")!;
                LocalWifiSSID     = this.FindControl<TextBox>("LocalWifiSSID")!;
                LocalWifiPassword = this.FindControl<TextBox>("LocalWifiPassword")!;
                LocalBrokerIP     = this.FindControl<TextBox>("LocalBrokerIP")!;
                DataGrid          = this.FindControl<Control>("DataGrid")!;
                MQTTStatus        = this.FindControl<TextBlock>("MQTTStatus")!;
                ImportList        = this.FindControl<Button>("ImportList")!;
                SelectALL         = this.FindControl<Button>("SelectALL")!;
                CancleALL         = this.FindControl<Button>("CancleALL")!;
                Set               = this.FindControl<Button>("Set")!;
                CmdTest           = this.FindControl<Button>("CmdTest")!;
                ResultTest        = this.FindControl<Button>("ResultTest")!;
                ClearTest         = this.FindControl<Button>("ClearTest")!;
                Clear             = this.FindControl<Button>("Clear")!;
                Connectbtn        = this.FindControl<Button>("Connectbtn")!;
                Disconnect        = this.FindControl<Button>("Disconnect")!;

                // Wire button click events
                Connectbtn.Click  += Connectbtn_Click;
                Disconnect.Click  += Disconnect_Click;
                Set.Click         += Set_Click;
                SelectALL.Click   += SelectALL_Click;
                CancleALL.Click   += CancleALL_Click;
                Clear.Click       += Clear_Click;
                ImportList.Click  += ImportList_Click;
                CmdTest.Click     += CmdTest_Click;
                ResultTest.Click  += ResultTest_Click;
                ClearTest.Click   += ClearTest_Click;

                Load();

                DataGrid.DataContext  = Global._dashboard;
                MQTTStatus.DataContext = Global._appViewModel;

                Global._appViewModel.ConnectionStatusChanged += CheckUI;

                CheckUI(this, false);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, "Error");
            }
        }

        // ── Avalonia: inline message dialogs ──────────────────────────────────

        private async void ShowError(string message, string title)
        {
            var dialog = new Window
            {
                Title  = title,
                Width  = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(0, 12, 0, 0)
                        }
                    }
                }
            };
            ((StackPanel)dialog.Content!).Children.OfType<Button>().First().Click += (_, _) => dialog.Close();
            await dialog.ShowDialog(this);
        }

        private async Task<bool> ShowYesNoDialog(string message, string title)
        {
            var tcs = new TaskCompletionSource<bool>();
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0),
                Children =
                {
                    new Button { Content = "Yes", Margin = new Thickness(4, 0) },
                    new Button { Content = "No",  Margin = new Thickness(4, 0) }
                }
            };
            var dialog = new Window
            {
                Title  = title,
                Width  = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                        btnPanel
                    }
                }
            };
            var btns = btnPanel.Children.OfType<Button>().ToList();
            btns[0].Click += (_, _) => { tcs.TrySetResult(true);  dialog.Close(); };
            btns[1].Click += (_, _) => { tcs.TrySetResult(false); dialog.Close(); };
            await dialog.ShowDialog(this);
            return await tcs.Task;
        }

        // ── Settings persistence ───────────────────────────────────────────────

        private void Save()
        {
            int.TryParse(Port.Text, out int port);

            Global._setting.ClientName        = ClientName.Text        ?? "";
            Global._setting.BrokerIP          = IP.Text                ?? "";
            Global._setting.BrokerPort        = port;
            Global._setting.LocalSSID         = LocalWifiSSID.Text     ?? "";
            Global._setting.LocalWifiPassword = LocalWifiPassword.Text ?? "";
            Global._setting.LocalBroker       = LocalBrokerIP.Text     ?? "";

            Global._setting.Save(_settingFile);
        }

        private void Load()
        {
            Global._setting.Read(_settingFile);

            ClientName.Text        = Global._setting.ClientName;
            IP.Text                = Global._setting.BrokerIP;
            Port.Text              = Global._setting.BrokerPort.ToString();
            LocalWifiSSID.Text     = Global._setting.LocalSSID;
            LocalWifiPassword.Text = Global._setting.LocalWifiPassword;
            LocalBrokerIP.Text     = Global._setting.LocalBroker;
        }

        // ── Window close ──────────────────────────────────────────────────────

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

                var topics = new List<string>
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

                Global._mQTTHelper.Connect();
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
                Global._mQTTHelper.Disconnect();
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
            try   { DataGridDynamic.SelectAll(); }
            catch (Exception ex) { ShowError(ex.CollectInnerException(), "Error"); }
        }

        private void CancleALL_Click(object? sender, RoutedEventArgs e)
        {
            try   { DataGridDynamic.UnselectAll(); }
            catch (Exception ex) { ShowError(ex.CollectInnerException(), "Error"); }
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            Global._devices.Clear();
            Global._dashboard.AddAndClear(new List<Device>());
        }

        private void CmdTest_Click(object? sender, RoutedEventArgs e)
        {
            try   { TestSendCommand(); }
            catch (Exception ex) { ShowError(ex.CollectInnerException(), "Error"); }
        }

        private void ResultTest_Click(object? sender, RoutedEventArgs e)
        {
            try   { TestResult(); }
            catch (Exception ex) { ShowError(ex.CollectInnerException(), "Error"); }
        }

        private void ClearTest_Click(object? sender, RoutedEventArgs e)
        {
            try   { TestClear(); }
            catch (Exception ex) { ShowError(ex.CollectInnerException(), "Error"); }
        }

        private async void ImportList_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this)!;
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title         = "Select file",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("CSV")       { Patterns = new[] { "*.csv" } },
                            new FilePickerFileType("All files") { Patterns = new[] { "*.*"   } }
                        }
                    });

                if (files.Count > 0)
                {
                    string filename = files[0].Path.LocalPath;
                    var csv = new CSVHelper();
                    var sns = csv.Read(filename);

                    foreach (var sn in sns)
                    {
                        var device = new Device { SN = sn };
                        if (Global._devices.ContainsKey(sn))
                            Global._devices[sn] = device;
                        else
                            Global._devices.TryAdd(sn, device);
                    }

                    Global._dashboard.AddAndClear(Global._devices.Values.OrderBy(x => x.SN).ToList());
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.CollectInnerException(), "Error");
            }
        }

        // ── UI enable/disable state ────────────────────────────────────────────

        private void CheckUI(object? sender, bool isConnected)
        {
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
            if (string.IsNullOrEmpty(LocalWifiSSID.Text)
                || string.IsNullOrEmpty(LocalWifiPassword.Text)
                || string.IsNullOrEmpty(LocalBrokerIP.Text))
            {
                return await ShowYesNoDialog("A setting is empty. Confirm to continue?", "Warning");
            }
            return true;
        }

        // ── MQTT publish ───────────────────────────────────────────────────────

        private void SendCommand()
        {
            var items = ((IEnumerable<object>)DataGridDynamic.SelectedItems).OfType<Device>().ToList();

            Task.Run(async () =>
            {
                foreach (var item in items)
                {
                    string topic = MQTTTopic.GetLocalSetting(item.SN);
                    var setting = new LocalSetting
                    {
                        ID            = item.SN,
                        Cmd           = "LocalSetting",
                        SSID          = Global._setting.LocalSSID,
                        PWD           = Global._setting.LocalWifiPassword,
                        Broker        = Global._setting.LocalBroker,
                        BrokerAccount = Global._setting.LocalBrokerAccount,
                        BrokerPWD     = Global._setting.LocalBrokerPassword,
                    };
                    await Global._mQTTHelper.PublishAsync(topic, JsonConvert.SerializeObject(setting));
                    Thread.Sleep(50);
                }
            });
        }

        // ── Test helpers ───────────────────────────────────────────────────────

        private void TestSendCommand()
        {
            Task.Run(() =>
            {
                int c = 4;
                _ = TestLocalSettingAsync(c);
                _ = TestOTA(c);
                _ = TestDeviceStatus(c);
                _ = TestNTP(c);
                _ = TestLogRate(c);
                _ = TestParameter(c);
            });
        }

        private void TestResult()
        {
            Task.Run(() =>
            {
                int c = 100;
                _ = TestLocalSettingResult(c);
                _ = TestOTAResult(c);
                _ = TestNTPResult(c);
                _ = TestLogRateResult(c);
                _ = TestParameterResult(c);
            });
        }

        private void TestClear()
        {
            Task.Run(async () =>
            {
                for (int i = 1; i < 100; i++)
                {
                    var s = new LocalSettingResult { ID = i.ToString("0000") };
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/result/{s.ID}", "");
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{s.ID}", "");
                    Thread.Sleep(50);
                }
                int c = 4;
                _ = TestLocalSettingAsync(c, true);
                _ = TestOTA(c, true);
                _ = TestDeviceStatus(c, true);
                _ = TestNTP(c, true);
                _ = TestLogRate(c, true);
                _ = TestParameter(c, true);
                _ = TestLocalSettingResult(c, true);
                _ = TestOTAResult(c, true);
                _ = TestNTPResult(c, true);
                _ = TestLogRateResult(c, true);
                _ = TestParameterResult(c, true);
            });
        }

        private async Task TestLocalSettingAsync(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new LocalSetting
                {
                    ID = i.ToString("0000"), Cmd = "LocalSetting",
                    SSID = Global._setting.LocalSSID, PWD = Global._setting.Password,
                    Broker = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD = Global._setting.LocalBrokerPassword,
                };
                await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestOTA(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new OTA { ID = i.ToString("0000"), Cmd = "OTA", URL = "http://192.168.10.222/Files/Device/displayboard.bin" };
                await Global._mQTTHelper.PublishAsync($"battery/ota/cmd/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestDeviceStatus(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new DeviceStatus { ID = i.ToString("0000"), Connection = "online", Type = "Battery" };
                await Global._mQTTHelper.PublishAsync($"device/status/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestNTP(int count, bool clear = false)
        {
            bool sw = false;
            for (int i = 1; i <= count; i++)
            {
                var s = sw
                    ? new NTP { ID = i.ToString("0000"), Cmd = "NTP", NTPURL = "time.windows.com", NTPUTC = "" }
                    : new NTP { ID = i.ToString("0000"), Cmd = "NTP", NTPURL = "", NTPUTC = DateTime.UtcNow.ToString("o") };
                sw = !sw;
                await Global._mQTTHelper.PublishAsync($"battery/ntp/cmd/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestLogRate(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new LogRate { ID = i.ToString("0000"), Cmd = "LogRate", IntervalSec = 300 };
                await Global._mQTTHelper.PublishAsync($"battery/lograte/cmd/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestParameter(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var cfg = new BatteryConfig
                {
                    ChargingMode = "Parallel", DischargingMode = "Sequential",
                    AlertSettings = new Alertsettings { AlertType = "All", DisplayMode = "Default",
                        SystemMode = "Disabled", LowBatteryAlertInterval = "", LowBatteryAlertLevel = "" }
                };
                var s = new BatterySetting { ID = i.ToString("0000"), Cmd = "BatteryPara", BatteryConfig = cfg };
                await Global._mQTTHelper.PublishAsync($"battery/para/cmd/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestLocalSettingResult(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new LocalSettingResult
                {
                    ID = i.ToString("0000"), Cmd = "LocalSettingResult", Result = true,
                    SSID = Global._setting.LocalSSID, PWD = Global._setting.Password,
                    Broker = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD = Global._setting.LocalBrokerPassword,
                };
                await Global._mQTTHelper.PublishAsync($"battery/localsetting/result/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestOTAResult(int count, bool clear = false)
        {
            for (uint i = 2156593311; i <= 2156593311 + (uint)count; i++)
            {
                var s = new OTAResult { ID = i.ToString("0000"), Cmd = "OTAResult", Result = true, URL = "http://192.168.10.222/Files/Device/displayboard.bin" };
                await Global._mQTTHelper.PublishAsync($"battery/ota/result/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestNTPResult(int count, bool clear = false)
        {
            bool sw = false;
            for (int i = 1; i <= count; i++)
            {
                var s = sw
                    ? new NTPResult { ID = i.ToString("0000"), Cmd = "NTPResult", Result = true, NTPURL = "time.windows.com", NTPUTC = "" }
                    : new NTPResult { ID = i.ToString("0000"), Cmd = "NTP",       Result = true, NTPURL = "", NTPUTC = DateTime.UtcNow.ToString("o") };
                sw = !sw;
                await Global._mQTTHelper.PublishAsync($"battery/ntp/result/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestLogRateResult(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var s = new LogRateResult { ID = i.ToString("0000"), Cmd = "LogRateResult", Result = true, IntervalSec = 300 };
                await Global._mQTTHelper.PublishAsync($"battery/lograte/result/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }

        private async Task TestParameterResult(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var cfg = new BatteryConfig
                {
                    ChargingMode = "Parallel", DischargingMode = "Sequential",
                    AlertSettings = new Alertsettings { AlertType = "All", DisplayMode = "Default",
                        SystemMode = "Disabled", LowBatteryAlertInterval = "", LowBatteryAlertLevel = "" }
                };
                var s = new BatterySettingResult { ID = i.ToString("0000"), Cmd = "BatteryParaResult", Result = true, BatteryConfig = cfg };
                await Global._mQTTHelper.PublishAsync($"battery/para/result/{s.ID}", clear ? "" : JsonConvert.SerializeObject(s));
                Thread.Sleep(50);
            }
        }
    }
}
