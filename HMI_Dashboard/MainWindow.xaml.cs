using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HMI_Dashboard
{
    public partial class MainWindow : Window
    {
        // Timer
        private DispatcherTimer _timer = new();
        
        private string _connStr =
    @"Server=.\sql2025;Database=HMI;Trusted_Connection=True;TrustServerCertificate=True;";

        // 圖表資料
        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<Axis> XAxes { get; set; }
        public ObservableCollection<Axis> YAxes { get; set; }
        private ObservableCollection<double> _tempValues = new();
        private List<string> _timeLabels = new();

        // 狀態
        private bool _isRunning = false;
        private Random _random = new();

        public MainWindow()
        {
            InitializeComponent();
            InitChart();
            DataContext = this;
            LoadLastRecipe();
        }

        private void InitChart()
        {
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = _tempValues,
                    Name = "溫度",
                    Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                    GeometrySize = 0,
                    Fill = null
                }
            };

            XAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = _timeLabels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 10
                }
            };

            YAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    MinLimit = 0,
                    MaxLimit = 150
                }
            };
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = true;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            StatusLight.Fill = new SolidColorBrush(Colors.LimeGreen);
            StatusText.Text = "運行中";
            AddLog("系統啟動");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            _timer.Stop();

            StatusLight.Fill = new SolidColorBrush(Colors.Gray);
            StatusText.Text = "待機";
            AddLog("系統停止");
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            double temp = _random.Next(60, 130);
            double pressure = Math.Round(_random.NextDouble() * 9 + 1, 1);
            double limit = double.TryParse(TempLimitInput.Text, out var l) ? l : 100;

            TempValue.Text = temp.ToString("F0");
            PressureValue.Text = pressure.ToString("F1");

            if (_tempValues.Count >= 30)
            {
                _tempValues.RemoveAt(0);
                _timeLabels.RemoveAt(0);
            }
            _tempValues.Add(temp);
            _timeLabels.Add(DateTime.Now.ToString("HH:mm:ss"));
            XAxes[0].Labels = new List<string>(_timeLabels);

            if (temp >= limit)
            {
                StatusLight.Fill = new SolidColorBrush(Colors.Red);
                StatusText.Text = "異常";
                AddLog($"⚠ 溫度異常：{temp}°C 超過上限 {limit}°C");
                SaveEventLog("ALARM", temp);  // 寫入DB
            }
            else if (temp >= limit - 10)
            {
                StatusLight.Fill = new SolidColorBrush(Colors.Yellow);
                StatusText.Text = "警告";
                SaveEventLog("WARNING", temp);  // 寫入DB
            }
            else
            {
                StatusLight.Fill = new SolidColorBrush(Colors.LimeGreen);
                StatusText.Text = "運行中";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            double limit = double.TryParse(TempLimitInput.Text, out var l) ? l : 100;

            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connStr);
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "INSERT INTO Recipe (MachineName, TempLimit, PressureLimit) VALUES (@name, @temp, @pressure)",
                    conn);
                cmd.Parameters.AddWithValue("@name", "機台A");
                cmd.Parameters.AddWithValue("@temp", limit);
                cmd.Parameters.AddWithValue("@pressure", 10.0);
                cmd.ExecuteNonQuery();

                AddLog($"✅ 配方已儲存：溫度上限 {limit}°C");
                MessageBox.Show("配方儲存成功！", "儲存", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}");
            }
        }

        private void SaveEventLog(string eventType, double value)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connStr);
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "INSERT INTO EventLog (EventType, Value) VALUES (@type, @value)",
                    conn);
                cmd.Parameters.AddWithValue("@type", eventType);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // 暫時改成顯示錯誤，方便診斷
                Dispatcher.Invoke(() => AddLog($"❌ DB錯誤：{ex.Message}"));
            }
        }
        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow();
            historyWindow.Show();
        } 

        private void AddLog(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            LogList.Items.Insert(0, entry);
        }
        private void LoadLastRecipe()
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connStr);
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT TOP 1 TempLimit FROM Recipe ORDER BY SavedAt DESC",
                    conn);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    TempLimitInput.Text = result.ToString();
                    AddLog($"✅ 已載入上次配方：溫度上限 {result}°C");
                }
            }
            catch { }
        }
    }
}