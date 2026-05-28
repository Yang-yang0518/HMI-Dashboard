# 產線設備監控系統（HMI Dashboard）

以 C# WPF 開發的機台監控模擬系統，模擬 HMI 人機介面核心操作流程。

## 技術堆疊
- C# WPF (.NET 8)
- LiveChartsCore.SkiaSharpView.WPF（即時折線圖）
- SQL Server + Microsoft.Data.SqlClient

## 功能說明
- 即時溫度／壓力數值顯示與折線圖更新（每秒刷新）
- 三段狀態燈：正常（綠）／警告（黃）／異常（紅）
- 異常事件自動寫入 SQL Server EventLog 資料表
- 配方儲存：將溫度上限設定值寫入 Recipe 資料表
- 啟動自動載入上次配方：模擬工控設備斷電重啟後的狀態恢復流程

## 資料庫結構
| 資料表 | 用途 |
|--------|------|
| Recipe | 儲存機台配方設定（溫度上限、壓力上限） |
| EventLog | 記錄異常與警告事件（類型、數值、時間） |

## 畫面截圖
<img width="1107" height="745" alt="HMI 實作" src="https://github.com/user-attachments/assets/b2724b63-856d-4ae8-9132-9c7f9c3b7e6a" />
<img width="1113" height="738" alt="HMI 警告" src="https://github.com/user-attachments/assets/723d34be-ab78-4c3b-9f95-bbd0439517ba" />
<img width="1122" height="737" alt="HMI 異常" src="https://github.com/user-attachments/assets/da266bd5-1db8-4c15-825e-bbe71882e78e" />
