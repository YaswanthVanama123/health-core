using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Geni_View_SettingTool.Models
{
    public class Device : INotifyPropertyChanged
    {
        private string _sn;
        public string SN
        {
            get { return _sn; }
            set
            {
                _sn = value;
                OnPropertyChanged("SN");
            }
        }

        private string _ssid;
        public string SSID
        {
            get { return _ssid; }
            set
            {
                _ssid = value;
                OnPropertyChanged("SSID");
            }
        }

        private string _broker;
        public string Broker
        {
            get { return _broker; }
            set
            {
                _broker = value;
                OnPropertyChanged("Broker");
            }
        }

        private bool _result;
        public bool Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        private string _createTime;
        public string CreateTime
        {
            get { return _createTime; }
            set
            {
                if (DateTime.TryParse(value, out DateTime dt))
                    _createTime = dt.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                else
                    _createTime = value;

                OnPropertyChanged("CreateTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Dashboard : INotifyPropertyChanged
    {
        public ObservableCollection<Device> Devices { get; set; }

        public Dashboard()
        {
            Devices = new ObservableCollection<Device>();
        }

        public bool AddAndClear(List<Device> devices)
        {
            // Avalonia UI thread dispatch (replaces Application.Current.Dispatcher.BeginInvoke)
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Devices.Clear();
                foreach (var item in devices)
                    Devices.Add(item);
            });

            return false;
        }

        public bool AddOrUpdate(string sn, Device data)
        {
            var device = Devices.FirstOrDefault(x => x.SN == sn);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (device == null)
                    Devices.Add(data);
                else
                {
                    int idx = Devices.IndexOf(device);
                    if (idx >= 0) Devices[idx] = data;
                }
            });

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
