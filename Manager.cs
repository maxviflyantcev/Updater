using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public interface IManager
    {
        void GetVersion();
        void Get_Setup_Version(string[] path);
        void Install(string path);
        void Delete(string ProdactName);
    }
    public class ManagerController : IManager
    {
        public BindingList<CurrentSoftwave> _Current = new BindingList<CurrentSoftwave>();
        public BindingList<SetupSoftwave> _Setup = new BindingList<SetupSoftwave>();

        public void Install(string Path)
        {
            try
            {
                Process _Process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Path,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WorkingDirectory = Environment.SystemDirectory,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = "/s /v/qn"
                    }
                };
                _Process.Start();
                _Process.WaitForExit();
                _Process.Close();
            }
            catch (Exception ex)
            {

            }
        }
        public void Delete(string ProdactName)
        {
            try
            {
                string cov_name = "\"" + ProdactName + "\""; // Имя ПО, которые мы хотим удалить должно быть в ковычках
                                                          // wmic product name where = "Имя программы" call uninstall /nointerractive
                Process _Process = new Process()
                {
                    StartInfo = new ProcessStartInfo("WMIC", "product where name=" + cov_name + " call uninstall /nointeractive")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Environment.SystemDirectory
                    }
                };
                _Process.Start();
                _Process.WaitForExit();
                _Process.Close();
            }
            catch (Exception ex)
            {

            }
        }
        public void GetVersion()
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] reg_names = regKey.GetSubKeyNames();
            foreach (var item in reg_names)
            {
                RegistryKey Reestr = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + item);

                string Name = (string)Reestr.GetValue("DisplayName");
                string Version = (string)Reestr.GetValue("DisplayVersion");
                string Publisher = (string)Reestr.GetValue("Publisher");

                if (Publisher == "СЕДАТЭК-ИНФОРМ")
                {
                    FileVersionInfo _info = FileVersionInfo.GetVersionInfo(item);
                    CurrentSoftwave _upd = new CurrentSoftwave
                    {
                        Name = Name,
                        Version = Version,
                        Publisher = Publisher,
                        MajorVesion = _info.ProductMajorPart,
                        MinorVersion = _info.ProductMinorPart,
                        Build = _info.ProductBuildPart
                    };
                    _Current.Add(_upd);
                }
            }
        }

        public void Get_Setup_Version(string[] path)
        {
            foreach (var item in path)
            {
                FileVersionInfo _info = FileVersionInfo.GetVersionInfo(item);

                SetupSoftwave _set = new SetupSoftwave
                {
                    sName = _info.ProductName,
                    sVersion = _info.ProductVersion,
                    sMajorVesion = _info.ProductMajorPart,
                    sMinorVersion = _info.ProductMinorPart,
                    sBuild = _info.ProductBuildPart,
                    sPath = _info.FileName                    
                };                
                _Setup.Add(_set);
            }
        }
    }

    public class CurrentSoftwave : INotifyPropertyChanged
    {
        private string name;
        private string version;
        private string publisher;
        private string status;
        public int MajorVesion;
        public int MinorVersion;
        public int MicroVersion;
        public int Build;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Version
        {
            get { return version; }
            set
            {
                version = value;
                OnPropertyChanged("Version");
            }
        }
        public string Publisher
        {
            get { return publisher; }
            set
            {
                publisher = value;
                OnPropertyChanged("Publisher");
            }
        }
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class SetupSoftwave : INotifyPropertyChanged
    {
        private string sname;
        private string sversion;
        private string spath;
        public int sMajorVesion;
        public int sMinorVersion;
        public int sMicroVersion;
        public int sBuild;

        public string sName
        {
            get { return sname; }
            set
            {
                sname = value;
                OnPropertyChanged("sName");
            }
        }
        public string sVersion
        {
            get { return sversion; }
            set
            {
                sversion = value;
                OnPropertyChanged("sVersion");
            }
        }
        public string sPath
        {
            get { return spath; }
            set
            {
                spath = value;
                OnPropertyChanged("sPath");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
