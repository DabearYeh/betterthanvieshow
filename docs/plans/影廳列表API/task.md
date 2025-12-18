# 影廳列表 API 開發任務

## 任務概述
開發影廳管理 API 的第一支端點：`GET /api/admin/theaters`，用於取得所有影廳列表。

## 任務清單

### [x] 資料層 (Data Layer)
- [x] 建立 `Theater` Entity 模型
- [x] 建立 `ITheaterRepository` 介面
- [x] 實作 `TheaterRepository` 類別
- [x] 建立資料庫 Migration

### [x] 服務層 (Service Layer)
- [x] 建立 `TheaterResponseDto` 回應 DTO
- [x] 建立 `ITheaterService` 介面
- [x] 實作 `TheaterService` 類別

### [x] 控制器層 (Controller Layer)
- [x] 建立 `TheaterController` 控制器
- [x] 實作 `GET /api/admin/theaters` 端點
- [x] 加入適當的授權驗證 (Admin only)

### [x] 依賴注入配置
- [x] 在 `Program.cs` 註冊 Repository
- [x] 在 `Program.cs` 註冊 Service

### [x] 程式碼驗證
- [x] 編譯程式碼確認無錯誤

### [x] 資料庫與應用程式驗證
- [x] 執行 Migration 更新資料庫
- [x] 啟動應用程式
- [x] 確認 API 端點在 Scalar UI 正確顯示

## 完成狀態

✅ **所有開發任務已完成**

API 已準備好進行功能測試，只需要：
1. 取得 Admin JWT Token
2. 在 Scalar UI 進行端點測試
