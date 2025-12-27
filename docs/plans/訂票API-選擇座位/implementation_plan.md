# 取得場次座位配置 API + WebSocket 即時同步

## 目標

實作 `GET /api/showtimes/{showTimeId}/seats` API 並整合 SignalR WebSocket，實現即時座位狀態同步功能。

## 背景

根據 `訂票.feature` 的規則：
- **Rule**: 必須選擇座位
- **Rule**: 只能選擇未被訂走的座位
- **Rule**: 同一座位在同一場次只能被一人購買
- **Rule**: 一次訂單最多可訂 6 張票

根據 UI 設計（上傳圖片），座位選擇畫面需要顯示：
- 座位二維陣列（排列）
- 每個座位的狀態（可用、已售、走道等）
- 影廳資訊、電影資訊、時間、票價

**WebSocket 的必要性**：
- 多人同時選座時，需要即時同步座位狀態
- 避免競爭條件（兩人同時選同一座位）
- 提升用戶體驗（看到其他人訂票的即時狀態）

---

## 使用者審查項目

> [!IMPORTANT]
> **API 端點路徑確認**
> 
> 建議的 API 路徑為：`GET /api/showtimes/{showTimeId}/seats`
> 
> - `{showTimeId}`: 場次 ID
> 
> 這個路徑符合 RESTful 設計，將座位視為場次的子資源。是否同意此設計？

> [!IMPORTANT]
> **座位狀態定義**
> 
> 建議的座位狀態分類：
> - **可用** (`available`): 該座位尚未被訂購
> - **已售** (`sold`): 該座位已有有效票券（待支付、未使用、已使用）
> - **走道** (`aisle`): 座位類型為走道，不可選擇
> - **空位** (`empty`): 座位類型為 Empty，不可選擇
> - **無效** (`invalid`): `is_valid = false`，不可選擇
> 
> 狀態判斷邏輯：
> 1. 如果 `seat_type = "走道"` → 走道
> 2. 如果 `seat_type = "Empty"` → 空位
> 3. 如果 `is_valid = false` → 無效
> 4. 如果有有效票券 → 已售
> 5. 否則 → 可用

> [!IMPORTANT]
> **WebSocket 事件設計**
> 
> 建議的 SignalR Hub 事件：
> 
> **Client → Server**:
> - `JoinShowtime(showtimeId)`: 加入場次房間
> - `LeaveShowtime(showtimeId)`: 離開場次房間
> 
> **Server → Clients**:
> - `SeatStatusChanged(seatId, status)`: 座位狀態變更
> - `SeatLocked(seatId)`: 座位被鎖定（有人選中）
> - `SeatUnlocked(seatId)`: 座位解鎖（取消選擇）
> 
> 是否同意此設計？

---

## 建議變更

### NuGet 套件

#### 安裝 SignalR

```bash
# SignalR 已包含在 ASP.NET Core 中，無需額外安裝
```

---

### SignalR Hub

#### [NEW] [ShowtimeHub.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Hubs/ShowtimeHub.cs)

```csharp
using Microsoft.AspNetCore.SignalR;

namespace betterthanvieshow.Hubs;

/// <summary>
/// 場次座位即時同步 Hub
/// </summary>
public class ShowtimeHub : Hub
{
    private readonly ILogger<ShowtimeHub> _logger;

    public ShowtimeHub(ILogger<ShowtimeHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 加入場次房間
    /// </summary>
    public async Task JoinShowtime(int showtimeId)
    {
        var roomName = $"showtime_{showtimeId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation("Client {ConnectionId} joined showtime {ShowtimeId}", 
            Context.ConnectionId, showtimeId);
    }

    /// <summary>
    /// 離開場次房間
    /// </summary>
    public async Task LeaveShowtime(int showtimeId)
    {
        var roomName = $"showtime_{showtimeId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation("Client {ConnectionId} left showtime {ShowtimeId}", 
            Context.ConnectionId, showtimeId);
    }

    /// <summary>
    /// 廣播座位狀態變更（由訂單建立時呼叫）
    /// </summary>
    public async Task BroadcastSeatStatusChanged(int showtimeId, int seatId, string status)
    {
        var roomName = $"showtime_{showtimeId}";
        await Clients.Group(roomName).SendAsync("SeatStatusChanged", seatId, status);
        _logger.LogInformation("Broadcast seat {SeatId} status changed to {Status} in showtime {ShowtimeId}", 
            seatId, status, showtimeId);
    }
}
```

---

### Repository 層

#### [MODIFY] [ISeatRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ISeatRepository.cs)

如果尚未有 `ISeatRepository`，則建立：

```csharp
using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 座位 Repository 介面
/// </summary>
public interface ISeatRepository
{
    /// <summary>
    /// 根據影廳 ID 取得所有座位
    /// </summary>
    Task<List<Seat>> GetSeatsByTheaterIdAsync(int theaterId);
}
```

---

#### [NEW/MODIFY] [SeatRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/SeatRepository.cs)

```csharp
using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 座位 Repository 實作
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly ApplicationDbContext _context;

    public SeatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<Seat>> GetSeatsByTheaterIdAsync(int theaterId)
    {
        return await _context.Seats
            .Where(s => s.TheaterId == theaterId)
            .OrderBy(s => s.RowName)
            .ThenBy(s => s.ColumnNumber)
            .ToListAsync();
    }
}
```

---

### Service 層

#### [NEW] [IShowtimeService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IShowtimeService.cs)

```csharp
using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 場次 Service 介面
/// </summary>
public interface IShowtimeService
{
    /// <summary>
    /// 取得場次的座位配置
    /// </summary>
    Task<ShowtimeSeatsResponseDto?> GetShowtimeSeatsAsync(int showtimeId);
}
```

---

#### [NEW] [ShowtimeService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/ShowtimeService.cs)

```csharp
using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 場次 Service 實作
/// </summary>
public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<ShowtimeService> _logger;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        ISeatRepository seatRepository,
        ITicketRepository ticketRepository,
        ILogger<ShowtimeService> logger)
    {
        _showtimeRepository = showtimeRepository;
        _seatRepository = seatRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ShowtimeSeatsResponseDto?> GetShowtimeSeatsAsync(int showtimeId)
    {
        try
        {
            _logger.LogInformation("開始取得場次 {ShowtimeId} 的座位配置", showtimeId);

            // 1. 取得場次資訊（包含電影和影廳）
            var showtime = await _showtimeRepository.GetByIdWithDetailsAsync(showtimeId);
            if (showtime == null)
            {
                _logger.LogWarning("找不到場次: ID={ShowtimeId}", showtimeId);
                return null;
            }

            // 2. 取得影廳的所有座位
            var seats = await _seatRepository.GetSeatsByTheaterIdAsync(showtime.TheaterId);

            // 3. 查詢該場次的所有有效票券（用於判斷座位狀態）
            // 假設 TicketRepository 有此方法
            var soldSeatIds = await _ticketRepository.GetSoldSeatIdsByShowTimeAsync(showtimeId);

            // 4. 建立座位二維陣列
            var seatGrid = BuildSeatGrid(seats, soldSeatIds, showtime.Theater.RowCount, showtime.Theater.ColumnCount);

            _logger.LogInformation("成功取得場次 {ShowtimeId} 的座位配置", showtimeId);

            // 5. 計算結束時間和票價
            var endTime = showtime.StartTime.Add(TimeSpan.FromMinutes(showtime.Movie.Duration));
            var price = GetPriceByTheaterType(showtime.Theater.Type);

            return new ShowtimeSeatsResponseDto
            {
                ShowTimeId = showtime.Id,
                MovieTitle = showtime.Movie.Title,
                ShowDate = showtime.ShowDate.ToString("yyyy-MM-dd"),
                StartTime = showtime.StartTime.ToString(@"hh\:mm"),
                EndTime = endTime.ToString(@"hh\:mm"),
                TheaterName = showtime.Theater.Name,
                TheaterType = showtime.Theater.Type,
                Price = price,
                RowCount = showtime.Theater.RowCount,
                ColumnCount = showtime.Theater.ColumnCount,
                Seats = seatGrid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得場次 {ShowtimeId} 的座位配置時發生錯誤", showtimeId);
            throw;
        }
    }

    /// <summary>
    /// 建立座位二維陣列
    /// </summary>
    private static List<List<SeatDto>> BuildSeatGrid(
        List<Seat> seats, 
        HashSet<int> soldSeatIds, 
        int rowCount, 
        int columnCount)
    {
        var grid = new List<List<SeatDto>>();

        // 建立索引以快速查找座位
        var seatMap = seats.ToDictionary(s => (s.RowName, s.ColumnNumber), s => s);

        // 生成排名列表 (A, B, C, ...)
        var rowNames = seats.Select(s => s.RowName).Distinct().OrderBy(r => r).ToList();

        foreach (var rowName in rowNames)
        {
            var row = new List<SeatDto>();

            for (int col = 1; col <= columnCount; col++)
            {
                if (seatMap.TryGetValue((rowName, col), out var seat))
                {
                    // 判斷座位狀態
                    string status;
                    if (seat.SeatType == "走道")
                        status = "aisle";
                    else if (seat.SeatType == "Empty")
                        status = "empty";
                    else if (!seat.IsValid)
                        status = "invalid";
                    else if (soldSeatIds.Contains(seat.Id))
                        status = "sold";
                    else
                        status = "available";

                    row.Add(new SeatDto
                    {
                        SeatId = seat.Id,
                        RowName = seat.RowName,
                        ColumnNumber = seat.ColumnNumber,
                        SeatType = seat.SeatType,
                        Status = status,
                        IsValid = seat.IsValid
                    });
                }
                else
                {
                    // 不存在的位置填入 null 或 empty
                    row.Add(new SeatDto
                    {
                        SeatId = 0,
                        RowName = rowName,
                        ColumnNumber = col,
                        SeatType = "Empty",
                        Status = "empty",
                        IsValid = false
                    });
                }
            }

            grid.Add(row);
        }

        return grid;
    }

    /// <summary>
    /// 根據影廳類型取得票價
    /// </summary>
    private static int GetPriceByTheaterType(string theaterType)
    {
        return theaterType switch
        {
            "一般數位" => 300,
            "4DX" => 380,
            "IMAX" => 380,
            _ => 300
        };
    }
}
```

---

### DTO 層

#### [NEW] [ShowtimeSeatsResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/ShowtimeSeatsResponseDto.cs)

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 場次座位配置回應 DTO
/// </summary>
public class ShowtimeSeatsResponseDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 放映日期（格式：YYYY-MM-DD）
    /// </summary>
    public string ShowDate { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 票價
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 排數
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// 列數
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位二維陣列
    /// </summary>
    public List<List<SeatDto>> Seats { get; set; } = new();
}

/// <summary>
/// 座位 DTO
/// </summary>
public class SeatDto
{
    /// <summary>
    /// 座位 ID（0 表示不存在的位置）
    /// </summary>
    public int SeatId { get; set; }

    /// <summary>
    /// 排名（如：A、B、C）
    /// </summary>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型（一般座位、殘障座位、走道、Empty）
    /// </summary>
    public string SeatType { get; set; } = string.Empty;

    /// <summary>
    /// 座位狀態（available、sold、aisle、empty、invalid）
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
}
```

---

### Controller 層

#### [NEW] [ShowtimesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/ShowtimesController.cs)

```csharp
using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 場次控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(
        IShowtimeService showtimeService,
        ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService;
        _logger = logger;
    }

    /// <summary>
    /// 取得場次的座位配置
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第三步：選擇座位。
    /// 
    /// 返回該場次的完整座位資訊，包含座位二維陣列和每個座位的狀態。
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// 
    /// **業務規則**：
    /// - 座位狀態分為：可用、已售、走道、空位、無效
    /// - 已售：該座位已有有效票券（待支付、未使用、已使用）
    /// - 走道/空位/無效：不可選擇
    /// 
    /// **WebSocket 整合**：
    /// - 前端在進入此頁面後應連接到 SignalR Hub
    /// - 加入場次房間以接收即時座位狀態更新
    /// - 當其他用戶訂票時會收到 `SeatStatusChanged` 事件
    /// 
    /// **回應資料包含**：
    /// - 場次和電影資訊
    /// - 影廳資訊和票價
    /// - 座位二維陣列（完整配置）
    /// </remarks>
    /// <param name="id">場次 ID</param>
    /// <response code="200">成功取得座位配置</response>
    /// <response code="404">找不到指定的場次</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>座位配置</returns>
    [HttpGet("{id}/seats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ShowtimeSeatsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetShowtimeSeats(int id)
    {
        try
        {
            var result = await _showtimeService.GetShowtimeSeatsAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"找不到 ID 為 {id} 的場次",
                    Data = null
                });
            }

            return Ok(new ApiResponse<ShowtimeSeatsResponseDto>
            {
                Success = true,
                Message = "成功取得座位配置",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得場次 {ShowtimeId} 的座位配置時發生錯誤", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "取得座位配置時發生錯誤",
                Data = null
            });
        }
    }
}
```

---

### Repository 層擴展

#### [MODIFY] [ITicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定場次已售出的座位 ID 集合（狀態為 待支付、未使用、已使用）
/// </summary>
Task<HashSet<int>> GetSoldSeatIdsByShowTimeAsync(int showTimeId);
```

---

#### [MODIFY] [TicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs)

實作方法：

```csharp
/// <inheritdoc />
public async Task<HashSet<int>> GetSoldSeatIdsByShowTimeAsync(int showTimeId)
{
    var seatIds = await _context.Tickets
        .Where(t => t.ShowTimeId == showTimeId && 
                   (t.Status == "待支付" || t.Status == "未使用" || t.Status == "已使用"))
        .Select(t => t.SeatId)
        .ToListAsync();

    return new HashSet<int>(seatIds);
}
```

---

## 依賴注入

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

```csharp
// 註冊 Repository
builder.Services.AddScoped<ISeatRepository, SeatRepository>();

// 註冊 Service
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

// 註冊 SignalR
builder.Services.AddSignalR();

// 在 app 建立後，映射 Hub
app.MapHub<ShowtimeHub>("/hub/showtime");
```

---

## 驗證計畫

### HTTP 測試

#### [NEW] [get-showtime-seats.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/訂票API-選擇座位/tests/get-showtime-seats.http)

```http
@baseUrl = http://localhost:5041

### 測試 1: 成功取得座位配置
# @name getShowtimeSeats
# 前置條件：場次 ID 7 存在
GET {{baseUrl}}/api/showtimes/7/seats
Content-Type: application/json

###

### 測試 2: 場次不存在（應該返回 404）
# @name getShowtimeSeatsNotFound
GET {{baseUrl}}/api/showtimes/999999/seats
Content-Type: application/json

###
```

### SignalR 測試（前端）

```javascript
// 連接到 SignalR Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5041/hub/showtime")
    .build();

// 監聽座位狀態變更事件
connection.on("SeatStatusChanged", (seatId, status) => {
    console.log(`Seat ${seatId} status changed to ${status}`);
    updateSeatUI(seatId, status);
});

// 啟動連接
await connection.start();

// 加入場次房間
await connection.invoke("JoinShowtime", 7);

// 離開時
await connection.invoke("LeaveShowtime", 7);
await connection.stop();
```

---

## WebSocket 整合到訂單建立

當用戶確認訂單時（API 4: `POST /api/orders`），需要廣播座位狀態變更：

```csharp
// 在 OrderService 中
public class OrderService : IOrderService
{
    private readonly IHubContext<ShowtimeHub> _hubContext;

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        // ... 建立訂單邏輯 ...

        // 廣播座位狀態變更
        foreach (var seatId in dto.SeatIds)
        {
            await _hubContext.Clients
                .Group($"showtime_{order.ShowTimeId}")
                .SendAsync("SeatStatusChanged", seatId, "sold");
        }

        return order;
    }
}
```
