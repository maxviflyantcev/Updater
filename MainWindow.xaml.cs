using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace Updater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ManagerController Controller = new ManagerController();
        private string _SetupPath = null;
        public BackgroundWorker worker_setup, worker_update, worker_delete;

        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("Укажите директорию, в которой находятся обновления!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
            using (var fldr = new FolderBrowserDialog())
            {
                if (fldr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] Setup_Files = Directory.GetFiles(fldr.SelectedPath);
                    _SetupPath = fldr.SelectedPath;
                    lbl_updateDir.Content = fldr.SelectedPath;
                    Controller.Get_Setup_Version(Setup_Files);
                    grid_setup.ItemsSource = Controller._Setup;
                }
            }
            this.Show();
            Controller.GetVersion();
            grid_soft.ItemsSource = Controller._Current;
            grid_soft.LoadingRow += Grid_soft_LoadingRow;
        }

        private void Grid_soft_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            foreach (var soft in Controller._Current)
            {
                var find_soft = Controller._Setup.ToList().Find(x => x.sName == soft.Name);
                if (find_soft != null && find_soft.sVersion != soft.Version)
                {
                    char split = '.';
                    string[] set_soft = find_soft.sVersion.Split(split);
                    string[] cur_soft = soft.Version.Split(split);

                    if (set_soft.Length < 3 && cur_soft.Length < 3)
                    {
                        MessageBox.Show("Ошибка в определении версии!", "Внимание!", MessageBoxButton.OK);
                        return;
                    }
                    else if (int.Parse(set_soft[0]) > int.Parse(cur_soft[0]))
                    {
                        soft.Status = "Требуется обновление";
                        e.Row.Background = Brushes.Red;
                    }
                    else if (int.Parse(set_soft[1]) > int.Parse(cur_soft[1])) 
                    {
                        soft.Status = "Возможно обновление";
                        e.Row.Background = Brushes.Red;
                    }
                    else if (int.Parse(set_soft[2]) > int.Parse(cur_soft[2]))
                    {
                        soft.Status = "Обновление не требуется";
                        e.Row.Background = Brushes.Yellow;
                    }
                    else
                    {
                        soft.Status = "Установлена актуальная версия";
                        e.Row.Background = Brushes.Green;
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        private void Setup(object list)
        {
            List<SetupSoftwave> set = list as List<SetupSoftwave>;
             
            foreach (var item in set)
            {
                if (item != null)
                {
                    Dispatcher.BeginInvoke(new Action(() => txtBlock_Info.Text = "Установка... " + item.sName)); 
                    Controller.Install(item.sPath);
                    Dispatcher.BeginInvoke(new Action(() => txtBlock_Info.Text = item.sName + " успешно установлен в систему!"));
                    worker_setup.ReportProgress( 100 / set.Count);
                    Dispatcher.BeginInvoke(new Action(() => { Controller._Current.Clear(); Controller.GetVersion(); }));
                }                
            }
        }

        private void Worker_SetupDoWork(object sender, DoWorkEventArgs e)
        {
            Setup(e.Argument as List<SetupSoftwave>);
        }

        private void Worker_SetupProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress_Bar.Value += e.ProgressPercentage;
        }

        private void Update()
        {
            
        }

        private void Worker_UpdateDoWork(object sender, DoWorkEventArgs e)
        {           
            Update();
        }

        private void Worker_UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress_Bar.Value = e.ProgressPercentage;
        }

        private void Delete()
        {

        }

        private void Worker_DeleteDoWork(object sender, DoWorkEventArgs e)
        {
            Delete();
        }

        private void Worker_DeleteProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress_Bar.Value = e.ProgressPercentage;
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_soft.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            foreach (var item in grid_soft.Items)
            {
                if (firstCol.GetCellContent(item) is CheckBox box && box.IsChecked == true)
                {
                    CurrentSoftwave current = item as CurrentSoftwave;

                    txtBlock_Info.Text = "Удаление... " + current.Name;
                    Controller.Delete(current.Name);
                    txtBlock_Info.Text = current.Name + " успешно удалён из системы!";
                }
            }
            grid_soft.UpdateLayout();
        }

        private void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_soft.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            foreach (var item in grid_soft.Items)
            {
                if (firstCol.GetCellContent(item) is CheckBox box && box.IsChecked == true)
                {
                    CurrentSoftwave current = item as CurrentSoftwave;
                    SetupSoftwave setup = Controller._Setup.ToList().Find(x => current.Name == x.sName);

                    if (setup != null)
                    {
                        Controller.Delete(current.Name);
                        Controller.Install(setup.sPath);
                        txtBlock_Info.Text = setup.sName + " успешно обновлён!";
                    }
                }
            }
            grid_soft.UpdateLayout();
        }

        private void Btn_Setup_Click(object sender, RoutedEventArgs e)
        {
            List<SetupSoftwave> setup_list = new List<SetupSoftwave>();
            var firstCol = grid_setup.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);
            foreach (var item in grid_setup.Items)
            {
                if (firstCol.GetCellContent(item) is CheckBox box && box.IsChecked == true)
                {
                    SetupSoftwave _setup = item as SetupSoftwave;
                    
                    if (_setup != null)
                    {
                        setup_list.Add(_setup);
                    }
                }
            }
            worker_setup = new BackgroundWorker();
            worker_setup.WorkerReportsProgress = true;
            worker_setup.ProgressChanged += Worker_SetupProgressChanged;
            worker_setup.DoWork += Worker_SetupDoWork;
            worker_setup.RunWorkerAsync(setup_list);

            //var firstCol = grid_setup.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            //foreach (var item in grid_setup.Items)
            //{
            //    if (firstCol.GetCellContent(item) is CheckBox box && box.IsChecked == true)
            //    {
            //        SetupSoftwave _setup = item as SetupSoftwave;

            //        if (_setup != null)
            //        {
            //            txtBlock_Info.Text = "Установка... " + _setup.sName;
            //            Controller.Install(_setup.sPath);
            //            txtBlock_Info.Text = _setup.sName + " успешно установлен в систему!";

            //        }
            //    }
            //}
            //grid_soft.UpdateLayout();
            //Controller._Current.Clear();
            //Controller.GetVersion();
        }
        private void CheckBox_CurrentChecked(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_soft.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            if (!(sender is CheckBox chkSelectAll) || firstCol == null || grid_soft?.Items == null)
            {
                return;
            }

            foreach (var item in grid_soft.Items)
            {
                if (!(firstCol.GetCellContent(item) is CheckBox chBx))
                {
                    continue;
                }
                chBx.IsChecked = chkSelectAll.IsChecked;
            }
        }

        private void CheckBox_CurrentUnchecked(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_soft.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            if (!(sender is CheckBox chkSelectAll) || firstCol == null || grid_soft?.Items == null)
            {
                return;
            }

            foreach (var item in grid_soft.Items)
            {
                if (!(firstCol.GetCellContent(item) is CheckBox chBx))
                {
                    continue;
                }
                chBx.IsChecked = chkSelectAll.IsChecked;
            }
        }
        private void CheckBox_SetupChecked(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_setup.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            if (!(sender is CheckBox chkSelectAll) || firstCol == null || grid_setup?.Items == null)
            {
                return;
            }

            foreach (var item in grid_setup.Items)
            {
                if (!(firstCol.GetCellContent(item) is CheckBox chBx))
                {
                    continue;
                }
                chBx.IsChecked = chkSelectAll.IsChecked;
            }
        }

        private void CheckBox_SetupUnchecked(object sender, RoutedEventArgs e)
        {
            var firstCol = grid_setup.Columns.OfType<DataGridCheckBoxColumn>().FirstOrDefault(c => c.DisplayIndex == 0);

            if (!(sender is CheckBox chkSelectAll) || firstCol == null || grid_setup?.Items == null)
            {
                return;
            }

            foreach (var item in grid_setup.Items)
            {
                if (!(firstCol.GetCellContent(item) is CheckBox chBx))
                {
                    continue;
                }
                chBx.IsChecked = chkSelectAll.IsChecked;
            }
        }

        private void btn_changeSetupPath_Click(object sender, RoutedEventArgs e)
        {
            using (var fldr = new FolderBrowserDialog())
            {
                if (fldr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] Setup_Files = Directory.GetFiles(fldr.SelectedPath);
                    _SetupPath = fldr.SelectedPath;
                    lbl_updateDir.Content = fldr.SelectedPath;
                    Controller.Get_Setup_Version(Setup_Files);
                    grid_setup.ItemsSource = Controller._Setup;
                }
            }
        }
    }
}
