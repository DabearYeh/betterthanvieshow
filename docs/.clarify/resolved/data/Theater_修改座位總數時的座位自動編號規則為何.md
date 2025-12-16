# ✅ 釐清決議：Theater 修改座位總數時的座位自動編號規則為何

**決議日期**：2025-12-11  
**決議狀態**：✅ 已完成  
**優先級**：High (Phase 1)

---

## 📌 決議內容

### 選擇方案
**選項 C 完整版 — 網格配置 + Empty 初始化 + 禁止減少有訂票座位**

### 完整規則

#### 1. 座位配置方式（網格制）

**管理員操作**：
```
設定 row_count（排數）和 column_count（列數）
↓
系統自動生成：row_count × column_count 個座位網格
↓
座位編號：按行優先（row → column）
  第一行：A1, A2, A3, ..., A{column_count}
  第二行：B1, B2, B3, ..., B{column_count}
  ...
  最後行：{row_letter}1, {row_letter}2, ..., {row_letter}{column_count}

範例（6 排 × 10 列）：
  A1  A2  A3  A4  A5  A6  A7  A8  A9  A10
  B1  B2  B3  B4  B5  B6  B7  B8  B9  B10
  C1  C2  C3  C4  C5  C6  C7  C8  C9  C10
  D1  D2  D3  D4  D5  D6  D7  D8  D9  D10
  E1  E2  E3  E4  E5  E6  E7  E8  E9  E10
  F1  F2  F3  F4  F5  F6  F7  F8  F9  F10
```

#### 2. 座位類型與配置邏輯

**初始狀態**：
```
管理員設定 row_count、column_count 後
↓
所有座位格子初始化：
  seat_type = "Empty"（未配置）
  is_valid = false（不可用）
```

**座位類型及可用性**：

| seat_type | 含義 | 可坐 | is_valid | 說明 |
|-----------|------|------|---------|------|
| **Empty** | 未配置 | ❌ 否 | false | 管理員未選擇的格子 |
| **一般座位** | 普通座位 | ✅ 是 | true | 可供訂票 |
| **殘障座位** | 殘障專用座 | ✅ 是 | true | 可供訂票 |
| **走道** | 通道/過道 | ❌ 否 | false | 不可訂票 |

**配置流程**：
```
管理員進入座位配置頁面
↓
看到 6×10 的網格，所有格子初始為 Empty
↓
逐個點擊格子，選擇類型：
  ① A1 選「一般座位」→ seat_type = "一般座位", is_valid = true
  ② A2 選「走道」→ seat_type = "走道", is_valid = false
  ③ A3 保持「Empty」→ seat_type = "Empty", is_valid = false
  ④ ...依此類推
↓
保存配置
↓
計算 total_seats = 統計 is_valid = true 的座位數
```

#### 3. 增加座位的規則

**操作**：管理員修改 row_count 或 column_count，導致座位網格擴大

**系統自動處理**：
```
新增座位的初始狀態：
  ✓ seat_type = "Empty"（未配置）
  ✓ is_valid = false（不可用）

管理員需要：
  進入座位配置頁面，逐個設置新座位的類型
  （與初始化配置相同）

原有座位：
  ✓ 保持不變（配置和狀態不變）
```

**例子**：
```
原始：5×10 = 50 座位（已配置完成）
修改為：6×10 = 60 座位

新增座位（F1-F10）：
  初始為 Empty，is_valid = false
  管理員需進入配置頁面設置 F1-F10 的類型
```

#### 4. 減少座位的規則

**操作**：管理員修改 row_count 或 column_count，導致座位網格縮小

**系統檢查**：
```
識別被刪除的座位（通常是末尾的座位）

檢查這些座位是否有有效訂票：
  SELECT COUNT(*) FROM Ticket
  WHERE seat_id IN (要刪除的座位 ID)
  AND status IN ('待支付', '未使用', '已使用')
```

**處理邏輯**：
```
情況 1：被刪除的座位 無 有效訂票
  ✅ 允許操作
  執行刪除
  更新 total_seats

情況 2：被刪除的座位 有 有效訂票
  ❌ 拒絕操作
  提示管理員：「該座位已有訂票記錄，無法減少座位」
  要求取消訂單或等待票券過期後再操作
```

**例子**：
```
原始：6×10 = 60 座位
修改為：5×10 = 50 座位

被刪除的座位：F1-F10

檢查：F1-F10 中是否有有效訂票？
  ✅ 無訂票 → 允許刪除
  ❌ 有訂票 → 拒絕刪除，提示管理員
```

---

## 📝 設計理由

### 1. 網格配置的優勢
- 視覺清晰（WYSIWYG — 所見即所得）
- 管理員可直觀看到座位分布
- 支持複雜的座位配置（如中間有走道）

### 2. Empty 初始化的優勢
- 強制管理員檢查每個座位
- 防止誤配置
- 確保 total_seats 與實際有效座位數一致

### 3. 禁止減少有訂票座位
- 保護用戶的訂票權益
- 避免 Ticket 記錄孤立
- 降低系統風險

### 4. 座位類型的清晰性
- 明確區分「可坐」和「不可坐」
- Empty、走道、柱子等都標記為 is_valid = false
- 計算 total_seats 時只統計 is_valid = true

---

## 🔧 實施影響

### 受影響的規格檔案

1. **`spec/erm.dbml`** ✅ 待更新
   - Theater 表說明：明確座位配置為網格制
   - Seat 表說明：seat_type 四種值及 is_valid 含義
   - Theater.total_seats：計算為 is_valid = true 的座位數

2. **Feature 檔案**（待檢查）
   - `編輯影廳.feature` — 修改 row_count、column_count 的規則
   - `設定座位配置.feature` — 座位類型選擇和 Empty 初始化
   - 新增規則：減少座位時檢查有效訂票

3. **API/實施文件**（待實施）
   - 編輯影廳 API：修改 row_count、column_count 時的邏輯
   - 座位配置 API：保存座位類型時計算 total_seats
   - 減少座位 API：檢查有效訂票的邏輯

4. **數據庫邏輯**
   ```sql
   -- 計算 total_seats
   UPDATE Theater
   SET total_seats = (
     SELECT COUNT(*) FROM Seat
     WHERE theater_id = ? AND is_valid = true
   )
   
   -- 檢查減少座位是否可行
   SELECT COUNT(*) FROM Ticket t
   JOIN Seat s ON t.seat_id = s.id
   WHERE s.theater_id = ? 
   AND s.row_name = 'F'  -- 被刪除的排
   AND t.status IN ('待支付', '未使用', '已使用')
   ```

---

## 💡 後續相關決議

- **Theater 座位縮減策略**（Phase 1, Item 8）— 減少座位的詳細流程
- **座位配置編輯界面**（Phase 2）— UI 設計細節

---

## ✅ 檢查清單

- [x] 決議文字版本創建
- [ ] erm.dbml Seat 表說明更新（待執行）
- [ ] Feature 檔案驗證（下一步）
- [ ] 減少座位邏輯測試（下一步）

---

**下一個釐清項目**：`Theater_減少座位總數時的座位刪除策略為何.md`
