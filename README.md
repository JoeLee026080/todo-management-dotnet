# Todo Management .NET API

一個使用 ASP.NET Core、MongoDB 與 JWT 實作的 Todo API 練習專案。

作為個人練習專案，重點實踐以下三項後端工程核心技能：

- MongoDB CRUD API 實作
- JWT 登入與 Bearer Token 驗證
- 以整合測試驗證主要 API 行為

## 技術

| 類別 | 技術 |
| --- | --- |
| 執行環境 | .NET 8 |
| 框架 | ASP.NET Core Web API |
| 資料庫 | MongoDB |
| 驗證 | JWT Bearer Authentication |
| 測試 | xUnit + Microsoft.AspNetCore.Mvc.Testing |
| 測試資料庫 | Mongo2Go |

## 功能列表

- `POST /api/auth/login` 登入並取得 JWT Token
- `GET /api/items` 取得 Todo 清單
- `POST /api/items` 新增 Todo 項目
- `PUT /api/items/{id}` 更新 Todo 名稱
- `DELETE /api/items/{id}` 刪除 Todo 項目
- 受保護路由需攜帶 Bearer Token

## 專案結構

```text
TodoManagement.Api/
  Controllers/
    AuthController.cs
    HealthController.cs
    ItemsController.cs
  Models/
    LoginRequest.cs
    TodoItem.cs
  Options/
    JwtOptions.cs
    MongoDbOptions.cs
  Services/
    ItemRepository.cs
    JwtTokenService.cs
  Program.cs

TodoManagement.Tests/
  AuthApiTests.cs
  CustomWebApplicationFactory.cs
  HealthCheckTests.cs
  ItemsApiTests.cs
```

## API 概覽

### 健康檢查

| 方法 | 路徑 | 說明 |
| --- | --- | --- |
| GET | `/health` | 確認 API 可正常啟動 |

### 登入

| 方法 | 路徑 | 說明 |
| --- | --- | --- |
| POST | `/api/auth/login` | 使用帳號密碼取得 JWT Token |

Request body:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

成功時回傳：

```json
{
  "token": "<jwt-token>"
}
```

### Todo 項目

| 方法 | 路徑 | 說明 |
| --- | --- | --- |
| GET | `/api/items` | 取得所有項目 |
| POST | `/api/items` | 新增項目 |
| PUT | `/api/items/{id}` | 更新指定項目名稱 |
| DELETE | `/api/items/{id}` | 刪除指定項目 |

所有 `/api/items` 路由都需要在 Header 帶入：

```text
Authorization: Bearer <token>
```

## 本機啟動方式

### 1. 準備 MongoDB

請先確認本機已有可用的 MongoDB，預設設定如下：

```json
"MongoDb": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "crudDemo",
  "ItemsCollectionName": "items"
}
```

如需調整，可修改 `TodoManagement.Api/appsettings.json`。

如果你習慣使用 `.env`，也可以在 repo 根目錄建立 `.env`，目前專案會自動讀取以下兩個名稱：

```dotenv
MONGODB_URI=mongodb+srv://<your-connection-string>
JWT_SECRET=dev_secret_change_in_prod
```

程式會自動把它們對應到：

- `MongoDb:ConnectionString`
- `Jwt:SecretKey`

`.env` 已加入 `.gitignore`，不要把真實帳密提交到 repo。

### 2. 啟動 API

```bash
dotnet restore
dotnet run --project TodoManagement.Api
```

啟動後可先測試：

```bash
curl http://localhost:5200/health
```

## 執行測試

```bash
dotnet test TodoManagement.sln
```

測試會使用 Mongo2Go 啟動暫時性的 MongoDB 測試環境，不會連到你本機的正式資料庫。

目前測試覆蓋重點包含：

- 健康檢查端點可正常回應
- Todo CRUD 主流程
- 登入成功與失敗情境
- 未授權、無效 Token、有效 Token 的授權情境