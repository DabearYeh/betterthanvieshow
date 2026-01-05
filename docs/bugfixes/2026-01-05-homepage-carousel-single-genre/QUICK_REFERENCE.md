# 快速參考指南 - Homepage Carousel Genre

## 🚀 簡介

**`GET /api/movies/homepage`** API 的 **Carousel（輪播）** 部分的 `genre` 欄位已修改為只返回**第一個類型**。

---

## 📖 變更說明

### Carousel（輪播）

**修改前**:
```json
{
  "carousel": [
    { "genre": "Comedy, Action" }
  ]
}
```

**修改後**:
```json
{
  "carousel": [
    { "genre": "Comedy" }  // 只有第一個類型
  ]
}
```

### 其他列表（不變）

TopWeekly, ComingSoon, Recommended, AllMovies 保持完整 Genre：

```json
{
  "topWeekly": [
    { "genre": "Comedy, Action" }  // 保持完整
  ]
}
```

---

## 💻 使用範例

### JavaScript / Fetch API

```javascript
async function getHomepageData() {
  const response = await fetch('http://localhost:5041/api/movies/homepage');
  const data = await response.json();
  
  // Carousel - Genre 只有一個
  data.data.carousel.forEach(movie => {
    console.log(`${movie.title}: ${movie.genre}`);  // "Comedy" 而非 "Comedy, Action"
  });
  
  // TopWeekly - Genre 可能有多個
  data.data.topWeekly.forEach(movie => {
    const genres = movie.genre.split(',').map(g => g.trim());
    console.log(`${movie.title}: ${genres.join(', ')}`);
  });
}
```

### React Component

```jsx
function CarouselSlide({ movie }) {
  return (
    <div className="carousel-slide">
      <img src={movie.posterUrl} alt={movie.title} />
      <div className="info">
        <h2>{movie.title}</h2>
        {/* Genre 是單一類型，不需要分割 */}
        <span className="genre-badge">{movie.genre}</span>
        <span className="rating">{movie.rating}</span>
      </div>
    </div>
  );
}

function MovieCard({ movie }) {
  // 其他列表可能有多個 Genre
  const genres = movie.genre.split(',').map(g => g.trim());
  
  return (
    <div className="movie-card">
      <h3>{movie.title}</h3>
      <div className="genres">
        {genres.map(genre => (
          <span key={genre} className="genre-tag">{genre}</span>
        ))}
      </div>
    </div>
  );
}
```

### Vue 3

```vue
<template>
  <!-- Carousel -->
  <div class="carousel-item" v-for="movie in carousel" :key="movie.id">
    <h3>{{ movie.title }}</h3>
    <!-- Genre 是單一類型 -->
    <span class="genre">{{ movie.genre }}</span>
  </div>
  
  <!-- TopWeekly -->
  <div class="movie-item" v-for="movie in topWeekly" :key="movie.id">
    <h3>{{ movie.title }}</h3>
    <!-- Genre 可能有多個 -->
    <div class="genres">
      <span v-for="genre in movie.genre.split(',')" :key="genre.trim()">
        {{ genre.trim() }}
      </span>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const carousel = ref([]);
const topWeekly = ref([]);

onMounted(async () => {
  const response = await fetch('/api/movies/homepage');
  const data = await response.json();
  carousel.value = data.data.carousel;
  topWeekly.value = data.data.topWeekly;
});
</script>
```

---

## 🧪 測試

### PowerShell 測試

```powershell
# 執行測試腳本
.\test_carousel_simple.ps1
```

### 預期輸出

```
===== Carousel Movies =====
Movie: 胎尼這次出道保證不下架
Genre: Romance

Movie: 吉伊卡哇
Genre: SciFi

===== Validation =====
PASS - All carousel movies have single genre
```

### cURL 測試

```bash
curl http://localhost:5041/api/movies/homepage | jq '.data.carousel[] | .title + ": " + .genre'
```

---

## 📊 完整 API 回應結構

```json
{
  "success": true,
  "message": "取得首頁電影資料成功",
  "data": {
    "carousel": [
      {
        "id": 1,
        "title": "電影標題",
        "posterUrl": "...",
        "duration": 120,
        "genre": "Comedy",           // ← 單一類型
        "rating": "PG",
        "releaseDate": "2026-01-01",
        "endDate": "2026-02-01",
        "daysUntilRelease": null
      }
    ],
    "topWeekly": [
      {
        "id": 2,
        "title": "另一部電影",
        "genre": "Action, Adventure",  // ← 完整類型
        ...
      }
    ],
    "comingSoon": [ ... ],
    "recommended": [ ... ],
    "allMovies": [ ... ]
  }
}
```

---

## ⚠️ 注意事項

### 只有 Carousel 修改

| 列表 | Genre 格式 |
|------|-----------|
| Carousel | 單一類型 ✅ |
| TopWeekly | 完整類型 |
| ComingSoon | 完整類型 |
| Recommended | 完整類型 |
| AllMovies | 完整類型 |

### 前端處理建議

```javascript
// Carousel - 直接使用
const carouselGenre = movie.genre;  // "Comedy"

// 其他列表 - 可能需要分割
const genres = movie.genre.split(',').map(g => g.trim());  // ["Comedy", "Action"]
```

---

## 🔗 相關文件

- **主文件**: [README.md](./README.md)
- **測試結果**: [test_results.md](./test_results.md)
- **測試腳本**: [test_carousel_simple.ps1](./test_carousel_simple.ps1)

---

## 📞 問題回報

如有任何問題，請聯繫開發團隊或建立 Issue。
