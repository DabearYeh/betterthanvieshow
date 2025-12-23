電影院訂票系統 — 需求總整理

## 一、系統角色（Actors）

系統共包含三種角色：

### 前台使用者（Customer）
- 可瀏覽電影、選擇時段與影廳、訂票、購票
- 可查詢個人訂票記錄
- 必須註冊帳號（名稱、帳號、密碼）才能訂票

### 影城管理者（Admin）
- 使用後台來管理影廳、座位、電影場次
- 可查詢所有訂票記錄

### 驗票人員（Ticket Checker）
- 用於入場驗證票券的真偽與有效性
- 掃描 QR Code 驗票並更新票券狀態

## 二、資料模型（Data Model）

詳見 `spec/erm.dbml`

### 核心實體

#### User（使用者）
- **email** (string, unique): 登入帳號是用信箱
- **password** (string): 密碼
- **name** (string): 使用者名稱
- **role** (string): 角色（Customer、Admin、TicketChecker）
- **created_at** (string): 建立時間

#### Theater（影廳）
- **name** (string): 影廳名稱
- **type** (string): 影廳類型（一般數位、4DX、IMAX）
- **row_count** (int): 排數
- **column_count** (int): 列數
- **total_seats** (int): 座位總數
- 規則：影廳不能刪除
- 這裡還要一個樓層數

#### Seat（座位）
- **theater_id** (int, FK): 所屬影廳
- **row_name** (string): 排的名字（A、B、C…）
- **column_number** (int): 欄的號碼 (1、2、3…) 
- **seat_type** (string): 座位類型（一般座位、殘障座位、走道）
- **is_valid** (bool): 座位是否有效（預設為真）
- 座位不是每一格都會用到，關於座位的格子可以這樣理解，排數*列數 = 總座位數，但是真正可用的座位數是要把empty和走道去掉
- Empty = 不屬於座位區，也不是走道，不會售票，也不在計數裡。
- 唯一條件：(theater_id, row_name, seat_number)

#### Movie（電影）
- **title** (string): 片名
- 影片類型(動作、愛情、冒險、懸疑、恐怖、科幻、日本動漫、喜劇) 一個電影可以有多個類型
- **duration** (int): 片長（分鐘）
- 電影分級:普遍級、輔導級、限制級，每個電影只會有一個電影分級

- 導演
- 演員
- **description** (string): 描述
- 預告片聯結
- 電影封面，可以上傳圖片
- 有上映日和下映日，只七在這個播放區間的電影都可以被排進時刻中
- **is_published** (bool): 是否上架（true上架、false下架）
- **can_carousel** (bool): 是否加入輪播
- **release_date** (string): 電影放映日期（YYYY-MM-DD）
- 兩個標籤，(1)本週前十：該週銷售最多的前 10 部電影和(2)即將上映：電影放映日 > 今天
- 規則：電影不能刪除，只能上架/下架

#### MovieShowTime（場次）
- **movie_id** (int, FK): 所屬電影
- **theater_id** (int, FK): 所在影廳
- **show_date** (string): 放映日期（YYYY-MM-DD）
- **start_time** (string): 開始時間（HH:MM）
- 唯一條件：(theater_id, show_date, start_time)
- 規則：同一影廳同一日期同一時間只能有一個場次

#### Order（訂單）
- **order_number** (string, unique): 訂單編號，格式 ^#[A-Z]{3}-\d{5}$（例 #ABC-12345）
- **user_id** (int, FK): 訂票使用者
- **created_at** (string): 訂單建立時間
- **expires_at** (string): 付款期限（建立後 5 分鐘自動過期）
- **status** (string): 訂單狀態（未付款、已付款、已取消）
- **show_time_id** (int, FK): 場次 ID
- **total_price** (float): 訂單總金額
- **ticket_count** (int): 票券數量
- 規則：每張 Order 最多訂 6 張票；5 分鐘內未付款自動取消並釋放座位

#### Ticket（票券）
- **ticket_number** (string, unique): 票券編號
- **order_id** (int, FK): 所屬訂單
- **seat_id** (int, FK): 座位 ID
- **qr_code** (string): QR Code（包含日期、影廳、座位、時間、影廳類型）
- **status** (string): 票券狀態（未使用、已使用、過期）
- **price** (float): 票價（固定值 300 元）
- 唯一條件：(show_time_id, seat_id)

- 規則：電影未結束都可掃 QR code 入場

#### TicketValidateLog（驗票日誌）
- **ticket_id** (int, FK): 票券 ID
- **validated_at** (string): 驗票時間
- **validated_by** (int, FK): 驗票人員 ID
- **validation_result** (bool): 驗票結果（真=有效、假=無效）



## 三、影城管理者需求（後台）

只有一個影城

### 1. 使用者帳號管理

#### 建立帳號（使用者註冊）
- 必填欄位：名稱、帳號、密碼
- 帳號必須唯一
- 功能規格：見 `spec/features/使用者註冊.feature`

#### 登入登出
- 登入：帳號 + 密碼
- 登出：清除登入狀態
- 功能規格：見 `spec/features/使用者登入.feature`、`spec/features/使用者登出.feature`

### 2. 影廳管理（Theaters）

管理者可進行以下操作：

#### 建立影廳
- 必填：影廳名稱、影廳類型（一般數位、4DX、IMAX）
- 然後選擇排數和列數生成座位網格
- 功能規格：見 `spec/features/建立影廳.feature`

#### 編輯影廳
- 可修改：影廳名稱、影廳類型
- 座位配置後不可改動排列
- 功能規格：見 `spec/features/編輯影廳.feature`

#### 影廳限制
- **影廳不能刪除**（系統不提供刪除功能）
- 功能規格：見 `spec/features/刪除影廳.feature`

### 3. 座位管理

#### 設定座位配置
- 建立影廳後，需設定座位配置
- 流程：
  1. 已有排數和列數生成的網格
  2. 對每個格子標記座位類型（一般座位、殘障座位、走道）
  3. 走道不計入座位總數
  4. 系統自動計算總座位數 = 一般座 + 殘障座
- 座位預設為有效（is_valid = true）
- 功能規格：見 `spec/features/設定座位配置.feature`

### 4. 電影管理

#### 建立電影
- 必填：片名、簡介、時長（> 0）
- 可選：海報、加入輪播、放映日期
- 功能規格：見 `spec/features/建立電影.feature`

#### 編輯電影
- 可修改：片名、簡介、時長、輪播設定、放映日期、海報
- 功能規格：見 `spec/features/編輯電影.feature`

#### 電影限制
- **電影不能刪除，只能上架/下架**
- is_published = true（上架）或 false（下架）
- 下架後不再前台顯示
- 功能規格：見 `spec/features/刪除電影.feature`

### 5. 電影排片（Scheduling）

#### 設定場次
- 必選：電影、影廳、放映日期、開始時間
- 規則：同一影廳同一日期同一時間只能有一個場次
- 功能規格：見 `spec/features/設定場次.feature`

### 6. 訂票記錄查詢

#### 查詢訂票記錄
- 管理者可查詢所有使用者的訂單
- 功能規格：見 `spec/features/查詢訂票記錄.feature`


## 四、前台使用者需求（User）

### 1. 帳號管理

#### 使用者註冊
- 必填：名稱、帳號、密碼
- 帳號必須唯一
- 註冊成功後自動登入
- 功能規格：見 `spec/features/使用者註冊.feature`

#### 使用者登入
- 必填：帳號、密碼
- 登入成功後可進行後續操作
- 功能規格：見 `spec/features/使用者登入.feature`

#### 使用者登出
- 清除登入狀態
- 登出後無法進行需登入的操作
- 功能規格：見 `spec/features/使用者登出.feature`

### 2. 瀏覽電影

使用者可瀏覽電影資訊：

#### 本週前十
- 顯示該週銷售最多的前 10 部已上架電影

#### 即將上映
- 顯示所有 release_date > 今天的已上架電影

#### 輪播電影
- can_carousel = true 的電影在首頁輪播

- 功能規格：見 `spec/features/瀏覽電影.feature`

### 3. 瀏覽場次

使用者可查看電影的所有場次，包含：
- 影廳名稱和影廳類型（一般數位、4DX、IMAX）
- 放映日期和開始時間
- 可用座位數
- 功能規格：見 `spec/features/瀏覽場次.feature`

### 4. 訂票流程

#### 訂票步驟
1. 選擇電影和場次
2. 選擇座位（只能選未被訂走的座位）
3. 確認訂單
4. 進行付款

#### 訂票限制
- **每張 Order 最多訂 6 張票**
- 同一座位在同一場次只能被一人購買（DB Unique Constraint）
- 只能選擇尚未被訂走的座位

#### 訂單編號
- 格式：^#[A-Z]{3}-\d{5}$（例 #ABC-12345）
- 自動生成

#### 付款時限
- **5 分鐘內未付款，系統自動取消訂單並釋放預留座位**
- Order.expires_at = created_at + 5 分鐘

#### 票券資訊
訂票成功後產生 Ticket，包含：
- **電影日期**
- **影廳名稱和影廳類型**
- **座位號碼**
- **開始時間**
- **QR Code**（掃碼用於驗票）

#### 票券狀態
- 新票券狀態：未使用
- 驗票後更新為：已使用
- 自動過期：過期（電影放映結束後）

- 功能規格：見 `spec/features/訂票.feature`

### 5. 查詢訂票記錄

使用者可查詢個人訂票記錄，包含：
- 訂單編號、訂單狀態、購票日期
- 電影名、影廳、座位、時間、票券狀態

- 功能規格：見 `spec/features/查詢訂票記錄.feature`



## 五、驗票人員需求（Ticket Checker）

驗票人員使用掃碼槍或手機掃描票券 QR Code，系統需進行以下驗證：

### 驗證步驟

1. **檢查票券是否存在**
   - 掃描 QR Code → 系統查詢票券記錄

2. **檢查票券狀態**
   - 票券必須為「未使用」狀態
   - 已使用或過期的票券無法驗票

3. **檢查場次是否結束**
   - 規則：**場次沒有結束，就都可以掃 QR code 入場**

4. **驗票成功**
   - 更新 Ticket.status 為「已使用」
   - 建立 TicketValidateLog 記錄（驗票時間、驗票人員）

### 驗票結果
- 票券存在且有效：允許入場，登記為「已入場」
- 票券不存在、已使用、過期或場次已結束：拒絕入場

- 功能規格：見 `spec/features/驗票.feature`





## 六、更新歷史

### 2025/12/10 更新
1. 票價調整為固定值 300 元
   - 移除 Movie 表的票價欄位
   - Ticket 表的票價固定為 300 元
   - 所有場次、所有電影的票券統一價格

### 2025/12/5 更新
1. 新增使用者登入登出系統
   - 使用者可以註冊帳號（名稱、帳號、密碼）
   - 登入時輸入帳號密碼
2. 影城管理者建立影廳流程優化
   - 先選排數和列數生成網格
   - 逐格標記座位類型（走道、一般座位、殘障座位）
   - 自動計算總座位數

### 2025/12/9 更新
1. 電影新增兩個標籤
   - 本週前十：該週銷售最多的前 10 部電影
   - 即將上映：電影放映日 > 今天
2. 訂票限制：每張 Order 最多 6 張票
3. 付款時限：訂票完成後 5 分鐘內未付款，自動取消訂單並釋放座位
4. 電影輪播：電影可設定加入輪播（can_carousel 欄位）
5. QR Code 內容：包含電影日期、影廳名稱、座位號、時間、影廳類型
6. 影廳類型：一般數位、4DX、IMAX
7. 票券狀態：未使用、已使用、過期
8. 影廳限制：影廳不能刪除
9. 電影限制：電影不能刪除，只能上架/下架
10. 訂單編號格式：^#[A-Z]{3}-\d{5}$（例 #ABC-12345）
11. 驗票規則：場次沒有結束，就都可以掃 QR code 入場

## 七、規格文件結構

- `spec/erm.dbml` - 資料模型（DBML 格式）
- `spec/features/*.feature` - 功能規格（Gherkin 格式）
  - 使用者登入.feature
  - 使用者登出.feature
  - 使用者註冊.feature
  - 建立影廳.feature
  - 編輯影廳.feature
  - 刪除影廳.feature
  - 設定座位配置.feature
  - 建立電影.feature
  - 編輯電影.feature
  - 刪除電影.feature
  - 設定場次.feature
  - 瀏覽電影.feature
  - 瀏覽場次.feature
  - 訂票.feature
  - 查詢訂票記錄.feature
  - 驗票.feature