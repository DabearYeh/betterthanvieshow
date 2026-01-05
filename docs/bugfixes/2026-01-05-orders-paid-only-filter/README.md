# Orders API 修改為只返回已付款訂單

**日期**: 2026-01-05  
**類型**: 功能修改 / 過濾優化  
**影響範圍**: Orders API  
**狀態**: ✅ 已完成並測試

---

## 📋 問題描述

原本的 `GET /api/orders` API 會返回當前使用者的**所有訂單**，包含：
- 未付款（Pending）的訂單
- 已付款（Paid）的訂單
- 已取消（Cancelled）的訂單

### 需求變更

前端只需要顯示**已付款的訂單**，未付款和已取消的訂單不應該出現在訂單列表中。

### 原始行為
```
GET /api/orders
→ 返回所有狀態的訂單（Pending, Paid, Cancelled）
```

### 期望行為
```
GET /api/orders
→ 只返回已付款的訂單（Paid）
```

---

## 🎯 解決方案

在 Repository 層的查詢中添加狀態過濾條件，只返回 `Status == "Paid"` 的訂單。

---

## 🔧 技術實作

### 1. 修改 Repository (`OrderRepository.cs`)

**檔案位置**: `betterthanvieshow/Repositories/Implementations/OrderRepository.cs`

**修改前**:
```csharp
public async Task<List<Order>> GetByUserIdAsync(int userId)
{
    return await _context.Orders
        .Include(o => o.ShowTime)
            .ThenInclude(s => s.Movie)
        .Where(o => o.UserId == userId)  // 只過濾 userId
        .OrderByDescending(o => o.ShowTime.ShowDate)
        .ThenByDescending(o => o.ShowTime.StartTime)
        .ToListAsync();
}
```

**修改後**:
```csharp
public async Task<List<Order>> GetByUserIdAsync(int userId)
{
    return await _context.Orders
        .Include(o => o.ShowTime)
            .ThenInclude(s => s.Movie)
        .Where(o => o.UserId == userId && o.Status == "Paid") // 增加狀態過濾
        .OrderByDescending(o => o.ShowTime.ShowDate)
        .ThenByDescending(o => o.ShowTime.StartTime)
        .ToListAsync();
}
```

**改動說明**:
- 在 `Where` 條件中增加 `&& o.Status == "Paid"`
- 只返回已付款的訂單
- 過濾掉 `Pending` 和 `Cancelled` 狀態的訂單

---

### 2. 更新 Controller 註解 (`OrdersController.cs`)

**檔案位置**: `betterthanvieshow/Controllers/OrdersController.cs`

更新 `GetMyOrders` 方法的 XML 文件註解：

**修改前**:
```csharp
/// <summary>
/// GET /api/orders 取得所有訂單
/// </summary>
/// <remarks>
/// 取得當前使用者的所有訂單，包含未付款、已付款、已取消的訂單。
/// </remarks>
```

**修改後**:
```csharp
/// <summary>
/// GET /api/orders 取得所有訂單
/// </summary>
/// <remarks>
/// 取得當前使用者的所有「已付款」訂單。
/// 
/// **過濾條件**：只返回 Status 為 "Paid" 的訂單（已移除未付款和已取消的訂單）。
/// </remarks>
```

---

## 🧪 測試結果

### API 回應範例

**請求**:
```http
GET /api/orders HTTP/1.1
Authorization: Bearer {token}
```

**回應**:
```json
{
  "success": true,
  "message": "成功取得訂單列表",
  "data": [
    {
      "orderId": 89,
      "movieTitle": "胎尼這次出道保證不下架",
      "posterUrl": "https://...",
      "showTime": "2026-01-10T10:15:00",
      "ticketCount": 6,
      "durationMinutes": 60,
      "status": "Paid",
      "isUsed": false
    },
    {
      "orderId": 96,
      "movieTitle": "某部電影",
      "posterUrl": "https://...",
      "showTime": "2026-01-08T14:30:00",
      "ticketCount": 3,
      "durationMinutes": 120,
      "status": "Paid",
      "isUsed": false
    }
  ],
  "errors": null
}
```

### 測試驗證

**測試日期**: 2026-01-05  
**測試環境**: Development (http://localhost:5041)  
**測試用戶**: test (userId: 35, role: Customer)

**測試結果**:
- ✅ 總訂單數: 2
- ✅ Order 89: Status = `Paid`
- ✅ Order 96: Status = `Paid`
- ✅ **驗證通過**: 所有訂單狀態都是 `Paid`

詳細測試結果請參考: [test_results.md](./test_results.md)

---

## 📊 影響分析

### 資料庫查詢

**修改前**:
```sql
SELECT * FROM Order 
WHERE UserId = @userId
ORDER BY ShowDate DESC, StartTime DESC
```

**修改後**:
```sql
SELECT * FROM Order 
WHERE UserId = @userId AND Status = 'Paid'
ORDER BY ShowDate DESC, StartTime DESC
```

### 效能影響

- ✅ **正面影響**: 減少返回的資料量
- ✅ **查詢效能**: 添加索引建議 `CREATE INDEX IX_Order_UserId_Status ON Order(UserId, Status)`
- ✅ **前端渲染**: 減少需要渲染的訂單數量

---

## 🔄 向後相容性

⚠️ **不完全向後相容**

此修改會影響前端行為：
- 前端將不再收到未付款和已取消的訂單
- 如果前端有顯示未付款訂單的功能，需要調整
- 建議前端使用新的 API 端點來處理未付款訂單（若需要）

### 遷移建議

如果前端需要顯示未付款訂單：
1. **選項 A**: 建立新的 API 端點 `GET /api/orders/pending` 專門返回未付款訂單
2. **選項 B**: 添加查詢參數 `GET /api/orders?status=all` 來控制過濾行為
3. **選項 C**: 維持現狀，只顯示已付款訂單（推薦）

---

## 📱 前端整合建議

### JavaScript 範例

```javascript
// 取得已付款訂單列表
const response = await fetch('/api/orders', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const data = await response.json();

// 所有訂單都是已付款狀態
data.data.forEach(order => {
  console.log(`Order ${order.orderId}: ${order.status}`); // 都是 "Paid"
  
  // 根據 isUsed 顯示不同樣式
  if (order.isUsed) {
    // 顯示為已使用/已過期
    renderUsedOrder(order);
  } else {
    // 顯示為可用訂單
    renderActiveOrder(order);
  }
});
```

### React 範例

```jsx
function OrderList() {
  const [orders, setOrders] = useState([]);

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    const response = await fetch('/api/orders', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    setOrders(data.data); // 所有訂單都是已付款
  };

  return (
    <div>
      <h2>我的訂單（已付款）</h2>
      {orders.map(order => (
        <OrderCard 
          key={order.orderId} 
          order={order}
          isUsed={order.isUsed}
        />
      ))}
    </div>
  );
}
```

---

## 📝 相關檔案

### 修改的檔案
- `betterthanvieshow/Repositories/Implementations/OrderRepository.cs` (第 77 行)
- `betterthanvieshow/Controllers/OrdersController.cs` (第 313-318 行)

### 測試檔案
- `test_orders_simple.ps1` - PowerShell 測試腳本

### 文件
- `README.md` - 此文件
- `test_results.md` - 詳細測試結果
- `QUICK_REFERENCE.md` - 快速參考指南

---

## ✅ 檢查清單

- [x] Repository 添加狀態過濾
- [x] Controller 更新 API 文件註解
- [x] 編譯成功
- [x] 功能測試通過
- [x] 驗證所有返回訂單狀態為 Paid
- [x] API 文件自動更新（Swagger/Scalar）
- [x] 建立測試腳本
- [x] 撰寫技術文件
- [x] 提供前端整合範例

---

## 👥 負責人

**開發者**: Gemini (AI Assistant)  
**審核者**: 待指定  
**測試者**: 待指定

---

## 📌 備註

- 此修改簡化了訂單列表的邏輯
- 未付款訂單會在 5 分鐘後自動取消，因此不需要在列表中顯示
- 如果未來需要顯示所有狀態的訂單，建議添加查詢參數而非修改預設行為
- 建議在資料庫中為 `(UserId, Status)` 建立複合索引以優化查詢效能
