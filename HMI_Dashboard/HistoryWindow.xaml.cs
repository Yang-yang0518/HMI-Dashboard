using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using System.Windows;

namespace HMI_Dashboard
{
    public partial class HistoryWindow : Window
    {
        private string _connStr =
            @"Server=.\sql2025;Database=HMI;Trusted_Connection=True;TrustServerCertificate=True;";

        private DataTable _currentData = new();

        public HistoryWindow()
        {
            InitializeComponent();
            StartDate.SelectedDate = DateTime.Today;
            EndDate.SelectedDate = DateTime.Today;
        }

        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            if (StartDate.SelectedDate == null || EndDate.SelectedDate == null)
            {
                MessageBox.Show("請選擇日期範圍");
                return;
            }

            var start = StartDate.SelectedDate.Value.Date;
            var end = EndDate.SelectedDate.Value.Date.AddDays(1);

            try
            {
                using var conn = new SqlConnection(_connStr);
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT Id, EventType, Value, OccurredAt 
                    FROM EventLog 
                    WHERE OccurredAt >= @start AND OccurredAt < @end
                    ORDER BY OccurredAt DESC", conn);

                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);

                _currentData = new DataTable();
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(_currentData);

                ResultGrid.ItemsSource = _currentData.DefaultView;

                int alarmCount = _currentData.AsEnumerable()
                    .Count(r => r["EventType"].ToString() == "ALARM");
                int warningCount = _currentData.AsEnumerable()
                    .Count(r => r["EventType"].ToString() == "WARNING");

                SummaryText.Text = $"共 {_currentData.Rows.Count} 筆記錄　｜　異常：{alarmCount} 筆　警告：{warningCount} 筆";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查詢失敗：{ex.Message}");
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData.Rows.Count == 0)
            {
                MessageBox.Show("沒有資料可以匯出，請先查詢。");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"EventLog_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".csv",
                Filter = "CSV 檔案|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var writer = new StreamWriter(dialog.FileName, false, System.Text.Encoding.UTF8);
                    writer.WriteLine("編號,事件類型,溫度數值,發生時間");

                    foreach (DataRow row in _currentData.Rows)
                        writer.WriteLine($"{row["Id"]},{row["EventType"]},{row["Value"]},{row["OccurredAt"]}");

                    MessageBox.Show($"匯出成功！\n{dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"匯出失敗：{ex.Message}");
                }
            }
        }
    }
}