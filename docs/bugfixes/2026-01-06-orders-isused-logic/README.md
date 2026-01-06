# 訂單 isUsed 欄位邏輯修改

**修改日期：** 2026-01-06  
**相關 API：** `GET /api/orders`  
**Branch：** `feature/orders-isused-ticket-validation-logic`

---

## 📝 問題描述

原本的 `GET /api/orders` API 中的 `isUsed` 欄位是根據**場次時間是否已過**來判斷，但這個邏輯不符合實際需求。

**實際需求：**  
`isUsed` 應該要根據**該訂單下所有票券是否都已驗票**來判斷。

---

## 🎯 修改目標

### 修改前的邏輯
```csharp
// 根據場次結束時間判斷
bool isUsed = endTime < now;
```

### 修改後的邏輯
```csharp
// 根據所有票券的驗票狀態判斷
bool isUsed = o.Tickets.Any() && o.Tickets.All(t => t.Status == "Used");
```

### 判定規則
- ✅ **Order A 有 3 張 ticket，全部都是 "Used"** → `isUsed = true`
- ❌ **Order B 有 3 張 ticket，只要有一張不是 "Used"** → `isUsed = false`

---

## 🔧 修改內容

### 1. 修改 `OrderRepository.GetByUserIdAsync`

**檔案：** `betterthanvieshow/Repositories/Implementations/OrderRepository.cs`

**修改說明：** 加入 `Include(o => o.Tickets)` 以便在 Service 層可以檢查票券狀態

```csharp
public async Task<List<Order>> GetByUserIdAsync(int userId)
{
    return await _context.Orders
        .Include(o => o.ShowTime)
            .ThenInclude(s => s.Movie)
        .Include(o => o.Tickets) // 新增：加入 Tickets 以便判斷 isUsed
        .Where(o => o.UserId == userId && o.Status == "Paid")
        .OrderByDescending(o => o.ShowTime.ShowDate)
        .ThenByDescending(o => o.ShowTime.StartTime)
        .ToListAsync();
}
```

---

### 2. 修改 `OrderService.GetMyOrdersAsync`

**檔案：** `betterthanvieshow/Services/Implementations/OrderService.cs`

**修改說明：** 將 `isUsed` 的判定邏輯從「時間判斷」改為「票券驗票狀態判斷」

```csharp
public async Task<List<OrderHistoryResponseDto>> GetMyOrdersAsync(int userId)
{
    var orders = await _orderRepository.GetByUserIdAsync(userId);
    
    var now = DateTime.Now;

    return orders.Select(o =>
    {
        var showTime = o.ShowTime.ShowDate.Date.Add(o.ShowTime.StartTime);
        var endTime = showTime.AddMinutes(o.ShowTime.Movie.Duration);
        
        // 修改：根據所有票券的驗票狀態判斷
        bool isUsed = o.Tickets.Any() && o.Tickets.All(t => t.Status == "Used");

        return new OrderHistoryResponseDto
        {
            OrderId = o.Id,
            MovieTitle = o.ShowTime.Movie.Title,
            PosterUrl = o.ShowTime.Movie.PosterUrl ?? "",
            ShowTime = showTime,
            TicketCount = o.TicketCount,
            DurationMinutes = o.ShowTime.Movie.Duration,
            Status = o.Status,
            IsUsed = isUsed
        };
    }).ToList();
}
```

---

### 3. 更新 API 文件註解

**檔案：** `betterthanvieshow/Controllers/OrdersController.cs`

**修改說明：** 更新 `GetMyOrders` 方法的 XML 文件註解

```csharp
/// <summary>
/// GET /api/orders 取得所有訂單
/// </summary>
/// <remarks>
/// 取得當前使用者的所有「已付款」訂單。
/// 
/// **過濾條件**：只返回 Status 為 "Paid" 的訂單（已移除未付款和已取消的訂單）。
/// 
/// **排序**：按場次時間倒序排列（最新的場次在最前面）。
/// 
/// **IsUsed 判定**：
/// - 檢查訂單下的所有票券是否都已驗票（Status = "Used"）
/// - 若所有票券都已驗票，`isUsed` 為 true
/// - 只要有任何一張票券尚未驗票，`isUsed` 為 false
/// </remarks>
```

---

## ✅ 測試驗證

### 測試資料

**票券資料：**
| Ticket ID | Order ID | Status |
|-----------|----------|--------|
| 174 | 115 | Unused |
| 175 | 116 | Used |
| 176 | 116 | Unused |

**訂單資料：**
| Order ID | Order Number | 預期 isUsed |
|----------|--------------|------------|
| 115 | #SFA-34707 | `false`（有 1 張 Unused） |
| 116 | #QQI-57357 | `false`（有 1 張 Unused，1 張 Used） |

### 測試結果

執行 `test-orders-isused-simple.ps1` 測試腳本：

```
[Step 1] Login...
✅ Login successful!

[Step 2] Get orders...
✅ Success! Total orders: 4

Order Details:
----------------------------------------
Order 115: isUsed = false ✅
Order 116: isUsed = false ✅
```

**結論：** ✅ 所有測試通過！`isUsed` 欄位正確反映票券驗票狀態。

---

## 📦 影響範圍

### 修改的檔案
1. `betterthanvieshow/Repositories/Implementations/OrderRepository.cs`
2. `betterthanvieshow/Services/Implementations/OrderService.cs`
3. `betterthanvieshow/Controllers/OrdersController.cs`

### API 行為變化
- **API 端點：** `GET /api/orders`
- **回應欄位：** `isUsed` 欄位的判定邏輯改變
- **向後相容性：** 欄位名稱和資料型別不變，但語意改變（從「時間判斷」改為「驗票狀態判斷」）

---

## 📚 相關資源

- 測試腳本：`test-orders-isused-simple.ps1`
- 快速測試：`quick-test.ps1`
- Branch：`feature/orders-isused-ticket-validation-logic`

---

## 🔄 後續步驟

1. ✅ 本地測試通過
2. ⏳ 提交變更並推送到遠端
3. ⏳ 建立 Pull Request
4. ⏳ Code Review
5. ⏳ 合併到 main branch
