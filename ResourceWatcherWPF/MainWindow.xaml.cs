using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;

namespace SystemMonitor
{
    public partial class MainWindow : Window
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _memoryCounter;
        private DispatcherTimer _timer;
        private List<double> _cpuUsageHistory;

        public ChartValues<double> CpuUsageValues { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            CpuUsageValues = new ChartValues<double>();
            CpuChart.DataContext = this;
            _cpuUsageHistory = new List<double>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateSystemMetrics();
        }

        private void UpdateSystemMetrics()
        {
            float cpuUsage = _cpuCounter.NextValue();
            float availableMemory = _memoryCounter.NextValue();
            float totalMemory = GetTotalMemory();
            float usedMemory = totalMemory - availableMemory;

            CpuProgressBar.Value = cpuUsage;
            CpuUsageText.Text = $"{cpuUsage:F2}%";
            MemoryProgressBar.Value = (usedMemory / totalMemory) * 100;
            MemoryUsageText.Text = $"{usedMemory:F2} MB / {totalMemory:F2} MB";
            NetworkUsageText.Text = $"{GetNetworkUsage():F2} MB/s";

            _cpuUsageHistory.Add(cpuUsage);
            if (_cpuUsageHistory.Count > 30) _cpuUsageHistory.RemoveAt(0);
            CpuUsageValues.Clear();
            foreach (var value in _cpuUsageHistory)
            {
                CpuUsageValues.Add(value);
            }
        }

        private float GetTotalMemory()
        {
            return (float)(Environment.WorkingSet / (1024 * 1024)); 
        }

        private double GetNetworkUsage()
        {
            return 0;
        }

        private void ProcessFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filter = ProcessFilterTextBox.Text.ToLower();
            var processes = Process.GetProcesses()
                .Where(p => p.ProcessName.ToLower().Contains(filter))
                .Select(p => new { ProcessName = p.ProcessName, Id = p.Id })
                .ToList();

            ProcessListView.ItemsSource = processes;
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            UpdateSystemMetrics();
        }
    }
}
