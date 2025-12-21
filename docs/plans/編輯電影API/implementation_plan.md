# 編輯電影 API 實作計畫

實作 `PUT /api/admin/movies/{id}` 端點，讓管理員可以編輯電影資訊。

## API 規格

- **端點**: `PUT /api/admin/movies/{id}`
- **權限**: 僅限 Admin
- **功能**: 更新指定 ID 的電影記錄

### 可修改欄位
所有欄位皆可修改（與新增電影相同），時長必須 > 0，下映日 >= 上映日。

---

## Proposed Changes

### DTOs

#### [NEW] [UpdateMovieRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/UpdateMovieRequestDto.cs)

與 `CreateMovieRequestDto` 結構相同，所有欄位皆為必填。

---

### Repository 層

#### [MODIFY] [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs)

新增方法：
```csharp
Task<Movie?> GetByIdAsync(int id);
Task<Movie> UpdateAsync(Movie movie);
```

---

#### [MODIFY] [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs)

實作 `GetByIdAsync` 和 `UpdateAsync` 方法。

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：
```csharp
Task<ApiResponse<MovieResponseDto>> UpdateMovieAsync(int id, UpdateMovieRequestDto request);
```

---

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作 `UpdateMovieAsync`：
- 驗證電影是否存在
- 驗證 `endDate >= releaseDate`
- 更新所有欄位
- 回傳更新結果

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增端點：
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateMovie(int id, [FromBody] UpdateMovieRequestDto request)
```

---

## Verification Plan

| 測試案例 | 預期結果 |
|---------|---------|
| 成功編輯電影 | 200 OK + 更新後資料 |
| 電影 ID 不存在 | 404 Not Found |
| 時長為 0 | 400 Bad Request |
| endDate < releaseDate | 400 Bad Request |
