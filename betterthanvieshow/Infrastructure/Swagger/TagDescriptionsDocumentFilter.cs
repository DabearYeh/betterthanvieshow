using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace betterthanvieshow.Infrastructure.Swagger;

public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag { Name = "Auth - 會員驗證", Description = "包含使用者註冊、登入以及 JWT Token 認證相關 API。" },
            new OpenApiTag { Name = "Booking - 訂票流程", Description = "訂票核心流程，包含快速查詢可訂票日期、特定日期場次以及影廳座位配置。" },
            new OpenApiTag { Name = "Movies - 電影資訊", Description = "提供前台展示所需的電影列表、搜尋、以及詳細資訊查詢 API。" },
            new OpenApiTag { Name = "Admin/Movies - 電影管理", Description = "管理者專區，用於執行電影資料的建立 (CRUD)、狀態維護與詳細資訊管理。" },
            new OpenApiTag { Name = "Admin/Theaters - 影廳管理", Description = "管理者專區，負責設定影廳規格、樓層資訊以及精細的座位矩陣配置。" },
            new OpenApiTag { Name = "Admin/DailySchedules - 排程管理", Description = "系統核心排票模組。管理者可在草稿狀態編輯場次，確認無誤後發佈 (Publish) 開始販售。" }
        };
    }
}
