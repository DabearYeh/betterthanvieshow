# 修改新增影廳 API - 加入座位配置

## 任務概述
修改 `POST /api/admin/theaters` API，使其能夠接收並處理座位配置資料（二維陣列格式）。

## 需求變更
原本：只建立 Theater 基本資料，TotalSeats = rowCount × columnCount
現在：同時建立 Theater 和 Seats，TotalSeats = 實際座位數（一般座位 + 殘障座位）

## 任務清單

### [x] Entity 層
- [x] 建立 `Seat` Entity
- [x] 在 `ApplicationDbContext` 加入 Seats DbSet
- [x] 設定 Seat 的資料庫約束
- [x] 建立並執行 Migration

### [x] DTO 層
- [x] 修改 `CreateTheaterRequestDto`，加入座位二維陣列
- [x] 加入座位陣列驗證規則

### [x] Repository 層
- [x] 加入 `CreateSeatsAsync` 方法
- [x] 加入 `GetByIdAsync` 方法

### [x] Service 層
- [x] 修改 `CreateTheaterAsync` 邏輯
- [x] 實作座位自動產生邏輯
- [x] 修正 TotalSeats 計算

### [x] 驗證
- [x] 編譯程式碼
- [x] 執行 Migration
- [x] 啟動應用程式
- [x] 確認端點顯示在 Scalar UI

## 完成狀態
✅ **所有開發任務已完成**

API 已準備好進行測試，需要：
1. 取得 Admin JWT Token
2. 在 Scalar UI 測試 POST 端點
3. 驗證座位資料正確建立
