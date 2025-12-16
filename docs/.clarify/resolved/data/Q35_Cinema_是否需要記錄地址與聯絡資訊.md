# Q35: Cinema_是否需要記錄地址與聯絡資訊

## 問題
Cinema 實體是否需要記錄地址與聯絡資訊（address、phone、email）？

## 背景
Cinema 目前只有 name 屬性。在實際業務中，若系統後續支援多影城管理，需要記錄每家影城的位置與聯絡方式供使用者查詢或管理員操作。

## 選項
- **A**：需要，新增 address、phone、email 屬性
- **B**：不需要，Cinema 只記錄名稱
- **C**：可選，提供配置選項

## 使用者選擇
**A** - 需要記錄 address, phone, email

## 理由
支援未來多影城擴展：
- 儲存影城地址便於使用者查詢
- 儲存聯絡方式便於客服或管理用途
- 為 SaaS 模式（多影城共用系統）預留設計空間

## 影響的規格檔案
- `spec/erm.dbml`: Cinema 表更新
  - address: string? [note: '影城地址']
  - phone: string? [note: '影城聯絡電話']
  - email: string? [note: '影城電郵']

## 規格變更
1. Cinema 新增 address、phone、email 三個非必填欄位
2. 這些欄位支援未來多影城擴展

## 相關決策
- 無其他決策與 Cinema 屬性相關
- Q35 是第 3 個與實體屬性相關的決策（Q7 Order、Q34 Movie 之後）
