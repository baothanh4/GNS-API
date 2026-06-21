# Night Shift: Asylum Backend

Backend cho game **Night Shift: Asylum**, sử dụng ASP.NET Core 8, MongoDB,
JWT authentication và TCP socket cho global chat.

## Yêu cầu

- .NET 8 SDK
- MongoDB Community Server
- PowerShell

## 1. Chạy MongoDB

Mở PowerShell bằng quyền Administrator:

```powershell
Start-Service MongoDB
Get-Service MongoDB
```

MongoDB đã sẵn sàng khi trạng thái service là `Running`. Kết nối mặc định:

```text
mongodb://localhost:27017
```

Nếu MongoDB chưa được cài dưới dạng Windows Service, chạy trực tiếp:

```powershell
mongod --dbpath C:\data\db
```

Tạo thư mục `C:\data\db` trước nếu thư mục chưa tồn tại.

## 2. Cấu hình JWT

Không lưu JWT secret thật trong Git. Thiết lập biến môi trường cho cửa sổ
PowerShell hiện tại:

```powershell
$env:JwtSettings__Secret="your-local-secret-key-at-least-32-characters"
```

MongoDB có thể được thay đổi bằng biến môi trường:

```powershell
$env:MongoDbSettings__ConnectionString="mongodb://localhost:27017"
$env:MongoDbSettings__DatabaseName="NightShiftDb"
```

## 3. Chạy Backend

```powershell
cd D:\Unity\Night-Shift-Asylum-Workspace\Backend-Server
dotnet restore
dotnet run
```

Các địa chỉ mặc định:

- REST API: `http://localhost:5000`
- Health check: `http://localhost:5000/health`
- TCP global chat: `localhost:5001`

Kiểm tra API:

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:5000/health
```

Kết quả hợp lệ có `StatusCode` là `200` và nội dung:

```json
{"status":"ok"}
```

## 4. Kiểm tra luồng dữ liệu

Sau khi MongoDB và backend đang chạy:

```powershell
.\test_api.ps1
```

Script kiểm tra đăng ký, đăng nhập, profile, inventory, room và game score.
Kết quả thành công:

```text
PASS: auth/profile/inventory/room/score flow completed.
```

## Cấu hình Unity

Unity client sử dụng:

```text
http://localhost:5000/api
```

Khi chạy client trên máy khác, thay `localhost` bằng IP của máy chạy backend.
