# Todo Management .NET API

一個使用 ASP.NET Core、MongoDB 與 JWT 實作的 Todo API 練習專案。

這個 repo 的目標是用 .NET 重新完成一份與既有 Node.js 版本功能對齊的後端作品，重點放在三件事：

- MongoDB CRUD API 實作
- JWT 登入與 Bearer Token 驗證
- 以整合測試驗證主要 API 行為

目前這個版本不額外擴充 Swagger、Docker、角色權限或前端畫面，重點是把核心後端能力做完整。

## 技術棧

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

## 學習重點

- 用 ASP.NET Core 重做一份既有 Node.js API 的功能規格
- 練習 MongoDB 在 .NET 中的設定綁定與 CRUD 實作
- 練習 JWT 簽發與 Bearer 驗證管線設定
- 練習用整合測試驗證 API 行為，而不是只測單一方法

## 對照版本

這個題目另外有一份 Node.js 版本，這份 .NET 專案的定位是功能對齊版，用來展示相同題目在不同後端技術棧下的實作方式。