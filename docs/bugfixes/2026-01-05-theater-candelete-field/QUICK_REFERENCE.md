# 快速參考指南 - canDelete 欄位

## 🚀 快速開始

### API 端點
```
GET /api/admin/theaters
```

### 回應格式
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "IMAX廳",
      "canDelete": false  // ← 新增欄位
    }
  ]
}
```

---

## 📖 欄位說明

| 欄位名稱 | 類型 | 說明 |
|---------|------|------|
| `canDelete` | boolean | 影廳是否可以刪除 |

### 值的意義

- **`true`**: 影廳沒有關聯的場次，可以安全刪除
- **`false`**: 影廳有關聯的場次，無法刪除

---

## 💻 前端使用範例

### JavaScript

```javascript
// 取得影廳列表
const theaters = await fetchTheaters();

// 根據 canDelete 顯示或隱藏刪除按鈕
theaters.forEach(theater => {
  const deleteBtn = document.querySelector(`#delete-${theater.id}`);
  
  if (theater.canDelete) {
    deleteBtn.classList.remove('hidden');
  } else {
    deleteBtn.classList.add('hidden');
  }
});
```

### React

```jsx
function TheaterItem({ theater }) {
  return (
    <div className="theater-card">
      <h3>{theater.name}</h3>
      
      {theater.canDelete ? (
        <button onClick={() => handleDelete(theater.id)}>
          刪除
        </button>
      ) : (
        <span className="text-muted">有場次，無法刪除</span>
      )}
    </div>
  );
}
```

### Vue

```vue
<template>
  <div class="theater-card">
    <h3>{{ theater.name }}</h3>
    
    <button 
      v-if="theater.canDelete"
      @click="handleDelete(theater.id)"
    >
      刪除
    </button>
    <span v-else class="text-muted">
      有場次，無法刪除
    </span>
  </div>
</template>
```

---

## 🧪 測試

### 使用 PowerShell 測試

```powershell
# 1. 先登入取得 token
$token = "your_jwt_token"

# 2. 執行測試腳本
.\test_candelete.ps1 -Token $token
```

### 使用 cURL 測試

```bash
# 1. 登入
curl -X POST http://localhost:5041/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"account":"admin","password":"admin123"}'

# 2. 取得影廳列表
curl -X GET http://localhost:5041/api/admin/theaters \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 📋 相關檔案

- **主文件**: [README.md](./README.md)
- **測試結果**: [test_results.md](./test_results.md)
- **測試腳本**: [test_candelete.ps1](./test_candelete.ps1)

---

## ⚠️ 注意事項

1. **向後相容**: 此欄位為新增，舊版前端可以忽略
2. **權限要求**: 需要 Admin 角色才能呼叫此 API
3. **即時性**: `canDelete` 值即時計算，反映當前資料庫狀態

---

## 🔗 相關 API

- `DELETE /api/admin/theaters/{id}` - 刪除影廳（需要 `canDelete` 為 true）
- `GET /api/admin/theaters/{id}` - 取得單一影廳詳細資訊

---

## 📞 問題回報

如有任何問題，請聯繫開發團隊或建立 Issue。
