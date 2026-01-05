# 快速參考指南 - Orders API 只返回已付款訂單

## 🚀 簡介

`GET /api/orders` API 已修改為**只返回已付款的訂單**。

---

## 📖 API 說明

### 端點
```
GET /api/orders
```

### 認證
需要 JWT Token（Bearer Authentication）

### 過濾邏輯
- ✅ 返回：Status = `"Paid"` 的訂單
- ❌ 過濾：Status = `"Pending"` 的訂單
- ❌ 過濾：Status = `"Cancelled"` 的訂單

### 排序
按場次時間倒序排列（最新的場次在最前面）

---

## 📊 回應格式

```json
{
  "success": true,
  "message": "成功取得訂單列表",
  "data": [
    {
      "orderId": 89,
      "movieTitle": "電影標題",
      "posterUrl": "海報URL",
      "showTime": "2026-01-10T10:15:00",
      "ticketCount": 6,
      "durationMinutes": 60,
      "status": "Paid",  // 所有訂單都是 Paid
      "isUsed": false
    }
  ]
}
```

---

## 💻 使用範例

### JavaScript / Fetch API

```javascript
async function getMyOrders() {
  const response = await fetch('http://localhost:5041/api/orders', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const data = await response.json();
  
  // 所有訂單都是已付款狀態
  data.data.forEach(order => {
    console.log(`Order ${order.orderId}: ${order.status}`); // "Paid"
  });
  
  return data.data;
}
```

### React Hook

```jsx
function useOrders() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchOrders() {
      try {
        const response = await fetch('/api/orders', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        setOrders(data.data); // 只有已付款訂單
      } catch (error) {
        console.error('Failed to fetch orders:', error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchOrders();
  }, [token]);

  return { orders, loading };
}
```

### Vue Composition API

```javascript
import { ref, onMounted } from 'vue';

export function useOrders() {
  const orders = ref([]);
  const loading = ref(true);

  async function fetchOrders() {
    try {
      const response = await fetch('/api/orders', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      const data = await response.json();
      orders.value = data.data; // 只有已付款訂單
    } catch (error) {
      console.error('Failed to fetch orders:', error);
    } finally {
      loading.value = false;
    }
  }

  onMounted(() => {
    fetchOrders();
  });

  return { orders, loading, fetchOrders };
}
```

### cURL

```bash
curl -X GET "http://localhost:5041/api/orders" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### PowerShell

```powershell
$token = "YOUR_JWT_TOKEN"
$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod -Uri "http://localhost:5041/api/orders" `
    -Method Get `
    -Headers $headers

# 顯示所有訂單
$response.data | ForEach-Object {
    Write-Host "Order $($_.orderId): $($_.status)"
}
```

---

## 🎯 前端顯示建議

### 訂單狀態說明

由於 API 只返回已付款訂單，前端可以簡化邏輯：

```javascript
function renderOrder(order) {
  // 不需要檢查 status，因為所有訂單都是 Paid
  
  if (order.isUsed) {
    return (
      <div className="order-card used">
        <Badge>已使用</Badge>
        <MovieInfo movie={order} />
      </div>
    );
  } else {
    return (
      <div className="order-card active">
        <Badge>可使用</Badge>
        <MovieInfo movie={order} />
        <QRCodeButton orderId={order.orderId} />
      </div>
    );
  }
}
```

### 空狀態處理

```javascript
function OrderList({ orders }) {
  if (orders.length === 0) {
    return (
      <EmptyState
        icon={<TicketIcon />}
        title="還沒有訂單"
        description="立即選購電影票，開始您的觀影之旅！"
        action={<Button href="/movies">瀏覽電影</Button>}
      />
    );
  }

  return (
    <div className="orders-grid">
      {orders.map(order => (
        <OrderCard key={order.orderId} order={order} />
      ))}
    </div>
  );
}
```

---

## 🧪 測試

### 測試腳本

```powershell
# 執行測試
.\test_orders_simple.ps1
```

### 預期結果

```
Testing GET /api/orders...
Total Orders: 2
Order Status Summary:
  Order 89: Status = Paid
  Order 96: Status = Paid
Validation Result:
  PASS - All 2 orders have status 'Paid'
```

---

## 📌 重要提醒

### ⚠️ 向後相容性

此修改**不完全向後相容**：
- 舊版前端如果期望收到未付款訂單，需要調整
- 未付款訂單不會出現在列表中
- 建議前端移除處理未付款訂單的邏輯

### ✅ 最佳實踐

1. **不要假設狀態**：雖然所有訂單都是 Paid，但仍建議在前端驗證
2. **使用 isUsed 欄位**：根據 `isUsed` 區分可用和已使用的訂單
3. **錯誤處理**：妥善處理 API 錯誤和空訂單情況
4. **快取策略**：考慮快取訂單列表以提升效能

---

## 🔗 相關文件

- **主文件**: [README.md](./README.md)
- **測試結果**: [test_results.md](./test_results.md)
- **測試腳本**: [test_orders_simple.ps1](./test_orders_simple.ps1)

---

## 📞 問題回報

如有任何問題，請聯繫開發團隊或建立 Issue。
