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
- 歷史事件查詢
- 匯出CSV
- 警報通知功能

## 資料庫結構
| 資料表 | 用途 |
|--------|------|
| Recipe | 儲存機台配方設定（溫度上限、壓力上限） |
| EventLog | 記錄異常與警告事件（類型、數值、時間） |

## 畫面截圖
<img width="1107" height="745" alt="HMI 實作" src="https://github.com/user-attachments/assets/07754ade-6c6e-48a4-bfd2-3a6987c107dc" />
<img width="971" height="526" alt="HMI歷史查詢" src="https://github.com/user-attachments/assets/f427b695-86df-403d-b184-69d8f92b25dc" />
<img width="967" height="607" alt="HMI 匯出CSV" src="https://github.com/user-attachments/assets/fa875f46-7491-4043-82b8-f3591dad9bbd" />
<img width="1733" height="852" alt="HMI 異常系統通知" src="https://github.com/user-attachments/assets/392cdc9e-f4aa-4f9d-a093-0155a69ac289" />
