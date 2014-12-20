using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfVisualizer
{
    /// <summary>
    /// SelectAppWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectAppWindow : Window
    {
        private List<ProcessVm> Processes { get; set; }

        public ProcessVm SelectedProcess { get; set; }

        public SelectAppWindow()
        {
            InitializeComponent();
            Loaded += SelectAppWindow_Loaded;
        }

        void SelectAppWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            Processes = new List<ProcessVm>();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                {
                    var vm = new ProcessVm
                    {
                        Process = p
                    };
                    Processes.Add(vm);
                }
            }
            Processes = Processes.OrderBy(p => p.Process.ProcessName).ToList();
            DataContext = Processes;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedIndex == -1)
            {
                MessageBox.Show("Select target app");
                return;
            }
            SelectedProcess = ProcessList.SelectedItem as ProcessVm;
            Debug.Assert(SelectedProcess != null);
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }
    }
}
