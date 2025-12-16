# ✅ 釐清決議：訂票 - Order 的 total_amount 如何從 Ticket 計算

**決議日期**：2025-12-11  
**決議狀態**：✅ 已完成  
**優先級**：High ⭐⭐

---

## 📌 決議內容

### 選擇方案
**方案 A — 訂單建立時一次性計算並儲存，不動態計算**

### 完整規則

#### 1. 計算時機與邏輯

```
訂單生命週期中 total_price 的處理：

Step 1: 訂單建立時（Order 資料插入前）
  ├─ 計算所有 Ticket 的單價總和
  ├─ total_price = SUM(Ticket.price)
  └─ 將結果儲存到 Order.total_price
  
Step 2: 訂單確認後
  ├─ Order.total_price 成為不可變（只讀）
  └─ 支付系統使用此金額進行收費
  
Step 3: 訂單查詢時
  └─ 直接讀取 Order.total_price（無需計算）
```

#### 2. 計算公式

```
Order.total_price = Σ Ticket.price（i=1 到 n）

其中：
  n = 訂單中的票券數量（1-6）
  Ticket.price = Theater.base_price（依照影廳類型）
  
簡化：
  Order.total_price = ticket_count × Theater.base_price
```

#### 3. 計算範例

**範例 1：一般 2D 廳，訂購 3 張票**

```
影廳類型：一般 2D
Theater.base_price = 300
ticket_count = 3

Ticket[1].price = 300
Ticket[2].price = 300
Ticket[3].price = 300

Order.total_price = 300 + 300 + 300 = 900 元
                 = SUM(Ticket.price)
```

**範例 2：IMAX 廳，訂購 4 張票**

```
影廳類型：IMAX
Theater.base_price = 380
ticket_count = 4

Ticket[1].price = 380
Ticket[2].price = 380
Ticket[3].price = 380
Ticket[4].price = 380

Order.total_price = 380 + 380 + 380 + 380 = 1520 元
                 = SUM(Ticket.price)
```

#### 4. 訂單建立流程（詳細步驟）

```
使用者確認訂單 →

Step 1: 驗證座位選擇
  └─ 確認 1-6 張票，座位可用

Step 2: 計算訂單金額
  ├─ 查詢 MovieShowTime 的 Theater
  ├─ 取得 Theater.base_price
  ├─ total_price = ticket_count × base_price
  └─ 保存到內存

Step 3: 建立 Order 記錄
  ├─ 生成 order_number（格式 #ABC-12345）
  ├─ Order.user_id = 當前用戶
  ├─ Order.show_time_id = 選定場次
  ├─ Order.total_price = 計算出的金額 ← 存儲
  ├─ Order.ticket_count = 票券數量
  ├─ Order.status = 'awaiting_payment'
  └─ Order.created_at = 當前時間

Step 4: 建立 Ticket 記錄（批量）
  ├─ 遍歷每個選定座位
  ├─ 為每個座位建立 Ticket
  │  ├─ Ticket.order_id = Order.id
  │  ├─ Ticket.seat_id = 座位 ID
  │  ├─ Ticket.price = base_price ← 記錄單價
  │  ├─ Ticket.status = 'awaiting_payment'
  │  └─ Ticket.qr_code = 待生成（支付完成後）
  └─ 全部插入數據庫

Step 5: 返回訂單信息給前端
  ├─ order_id
  ├─ order_number
  ├─ total_price ← 使用已儲存的值
  └─ 導向支付頁面

支付完成後：
  ├─ Order.status = 'paid'
  └─ Ticket 生成 QR Code
```

#### 5. 資料庫設計

```sql
-- Order 表必須包含 total_price 欄位
CREATE TABLE Order (
  id INT PRIMARY KEY AUTO_INCREMENT,
  order_number VARCHAR(20) UNIQUE NOT NULL,
  user_id INT NOT NULL,
  show_time_id INT NOT NULL,
  total_price DECIMAL(10, 2) NOT NULL,  -- ← 關鍵欄位
  ticket_count INT NOT NULL,
  status ENUM('awaiting_payment', 'paid', 'cancelled') DEFAULT 'awaiting_payment',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  expires_at TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES User(id),
  FOREIGN KEY (show_time_id) REFERENCES MovieShowTime(id)
);

-- Ticket 表記錄每張票的單價
CREATE TABLE Ticket (
  id INT PRIMARY KEY AUTO_INCREMENT,
  ticket_number VARCHAR(50) UNIQUE,
  order_id INT NOT NULL,
  seat_id INT NOT NULL,
  price INT NOT NULL,  -- ← 單價記錄（冗餘但必要）
  status ENUM('awaiting_payment', 'unused', 'used', 'expired') DEFAULT 'awaiting_payment',
  qr_code TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (order_id) REFERENCES Order(id),
  FOREIGN KEY (seat_id) REFERENCES Seat(id),
  UNIQUE KEY (order_id, seat_id)
);
```

---

## 🔧 實施影響

### 受影響的檔案

#### 1. **spec/erm.dbml** ✅ 待更新

```sql
Table Order {
  ...
  total_price Decimal [not null, note: 'SUM(Ticket.price) 計算結果，訂單建立時存儲']
  ...
}

Table Ticket {
  ...
  price Integer [not null, note: '該票券的單價（記錄歷史，不應改變）']
  ...
}
```

#### 2. **API 實施** ✅ 待實施

```javascript
// 建立訂單的核心邏輯
async function createOrder(req, res) {
  const { user_id, show_time_id, seat_ids } = req.body;
  
  // 1. 驗證座位數量
  if (!seat_ids || seat_ids.length === 0 || seat_ids.length > 6) {
    return res.status(400).json({ error: '座位數量必須 1-6 張' });
  }
  
  try {
    // 2. 查詢影廳和票價
    const showTime = await MovieShowTime.findById(show_time_id);
    const theater = await Theater.findById(showTime.theater_id);
    const base_price = theater.base_price;
    
    // 3. 計算訂單金額（關鍵步驟）
    const ticket_count = seat_ids.length;
    const total_price = ticket_count * base_price;
    
    // 4. 生成訂單編號
    const order_number = generateOrderNumber(); // #ABC-12345 格式
    
    // 5. 在事務中建立 Order 和 Tickets
    const order = await sequelize.transaction(async (t) => {
      // 建立 Order（儲存總價）
      const newOrder = await Order.create({
        order_number: order_number,
        user_id: user_id,
        show_time_id: show_time_id,
        total_price: total_price,  // ← 儲存計算結果
        ticket_count: ticket_count,
        status: 'awaiting_payment',
        expires_at: new Date(Date.now() + 3 * 60 * 1000) // 3 分鐘後過期
      }, { transaction: t });
      
      // 建立 Tickets（記錄單價）
      const tickets = seat_ids.map(seat_id => ({
        order_id: newOrder.id,
        seat_id: seat_id,
        price: base_price,  // ← 記錄當時的單價
        status: 'awaiting_payment'
      }));
      
      await Ticket.bulkCreate(tickets, { transaction: t });
      
      return newOrder;
    });
    
    // 6. 返回訂單信息
    return res.status(201).json({
      success: true,
      order_id: order.id,
      order_number: order.order_number,
      total_price: order.total_price,  // ← 使用已儲存的值
      ticket_count: order.ticket_count,
      expires_at: order.expires_at,
      message: '訂單建立成功，請於 3 分鐘內完成付款'
    });
    
  } catch (error) {
    return res.status(500).json({ error: error.message });
  }
}

// 查詢訂單時（不需要動態計算）
async function getOrderDetails(req, res) {
  const order = await Order.findById(req.params.order_id)
    .include([
      { association: 'user' },
      { association: 'showTime' },
      { association: 'tickets', include: ['seat'] }
    ]);
  
  if (!order) {
    return res.status(404).json({ error: '訂單不存在' });
  }
  
  // 驗證用戶權限（顧客只能查詢自己的訂單）
  if (req.user.role === 'customer' && order.user_id !== req.user.id) {
    return res.status(403).json({ error: '無權查詢該訂單' });
  }
  
  return res.json({
    order_id: order.id,
    order_number: order.order_number,
    status: order.status,
    created_at: order.created_at,
    total_price: order.total_price,  // ← 直接讀取，無需計算
    tickets: order.tickets.map(t => ({
      ticket_id: t.id,
      seat: `${t.seat.row_name}${t.seat.column_number}`,
      price: t.price,  // 當時購票的單價
      status: t.status
    }))
  });
}

// 驗證訂單金額（支付時）
async function verifyOrderAmount(order_id, payment_amount) {
  const order = await Order.findById(order_id);
  
  if (order.total_price !== payment_amount) {
    throw new Error(`金額不符：訂單 ${order.total_price} 元，支付 ${payment_amount} 元`);
  }
  
  return true;
}
```

#### 3. **前端實施** ✅ 待實施

```javascript
// 訂票頁面 - 建立訂單
async function submitOrder(showTimeId, selectedSeats) {
  try {
    const response = await fetch('/api/orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        show_time_id: showTimeId,
        seat_ids: selectedSeats
      })
    });
    
    const result = await response.json();
    
    // 顯示訂單摘要
    console.log(`訂單號：${result.order_number}`);
    console.log(`總金額：${result.total_price} 元`);  // 使用返回的值
    console.log(`有效期：${result.expires_at}`);
    
    // 導向支付頁面
    window.location.href = `/payment/${result.order_id}`;
    
  } catch (error) {
    alert(`訂單建立失敗：${error.message}`);
  }
}

// 支付頁面 - 確認金額
async function processPayment(orderId) {
  // 1. 查詢訂單（取得 total_price）
  const order = await fetch(`/api/orders/${orderId}`).then(r => r.json());
  
  // 2. 顯示金額供用戶確認
  document.getElementById('total-amount').textContent = `${order.total_price} 元`;
  
  // 3. 發送付款請求（後端會驗證 total_price）
  const paymentResult = await fetch(`/api/payments`, {
    method: 'POST',
    body: JSON.stringify({
      order_id: orderId,
      amount: order.total_price,  // 使用 API 返回的值
      payment_method: 'credit_card'
    })
  }).then(r => r.json());
  
  if (paymentResult.success) {
    alert('支付成功！');
    window.location.href = `/ticket/${orderId}`;
  }
}
```

---

## 🎯 設計理由

### 1. 數據完整性
- 訂單金額一旦確定不應改變
- 符合會計「訂單不可變」原則

### 2. 性能優化
- 查詢訂單時無需 JOIN Ticket 表
- 避免每次都計算 SUM

### 3. 支付安全
- 支付系統直接對比 Order.total_price
- 防止票價改變後金額計算誤差

### 4. 審計追蹤
- 記錄每張 Ticket 的單價
- 便於檢查歷史金額是否異常

### 5. 易於除錯
- 若發現金額異常，可直接比對 Order.total_price 與 SUM(Ticket.price)
- 發現不一致立即報警

---

## 📊 決議對照表

| 項目 | 決議 |
|------|------|
| 計算時機 | 訂單建立時一次性計算 |
| 儲存位置 | Order.total_price |
| 計算方式 | SUM(Ticket.price) = ticket_count × Theater.base_price |
| 動態計算 | ❌ 不使用（性能與安全原因） |
| Ticket 單價 | ✅ 記錄（用於驗證與追溯） |

---

## ✅ 檢查清單

- [x] 決議文字版本創建
- [ ] spec/erm.dbml 確認 total_price 欄位（下一步）
- [ ] API 建立訂單邏輯實施（下一步）
- [ ] 支付驗證邏輯實施（下一步）
- [ ] 前端訂單流程實施（下一步）

---

**Phase 2 完成狀態**：✅ Item 1-3/8 完成

**下一個釐清項目**：`訂票_取消訂單的條件與流程是什麼.md`

---

**文檔版本**：v1.0  
**最後更新**：2025-12-11  
**維護人員**：GitHub Copilot
