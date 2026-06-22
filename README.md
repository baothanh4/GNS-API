# Night Shift: Asylum Backend

Backend cho game **Night Shift: Asylum**, sử dụng ASP.NET Core 8, MongoDB,
JWT authentication và TCP socket cho global chat.

## Yêu cầu

- .NET 8 SDK (Dự án đã được cập nhật tương thích tốt với cả .NET 10 SDK)
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

*Lưu ý: Để thuận tiện cho việc test cục bộ, một khóa bí mật mặc định đã được cấu hình sẵn trong `appsettings.json`, bạn có thể bỏ qua bước đặt biến môi trường này khi chạy thử nghiệm.*

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

---

## 🎮 Hướng dẫn Tích hợp & Kiểm thử nâng cao trên Unity (Mới)

### 1. Cơ chế Tự động Đăng nhập trong Unity Editor (Auto-Login)
Khi bạn chạy thử nghiệm (Play) trực tiếp các Scene chơi game (như `MainScence`) trong Unity Editor mà chưa có UI Đăng nhập/Đăng ký:
*   `APIManager` sẽ **tự động khởi tạo** và chạy ngầm.
*   Hệ thống tự động đăng ký/đăng nhập tài khoản kiểm thử mặc định: **`test_player_new`** (mật khẩu: `Password123!`).
*   Giúp bạn nhặt đồ và lưu dữ liệu trực tiếp vào MongoDB mà không bị lỗi xác thực JWT.

### 2. Thiết lập UI Inventory (Hệ thống 2 Ô chứa)
*   **Giới hạn**: Inventory được giới hạn tối đa **2 ô**. Mỗi ô chỉ chứa tối đa **1 vật phẩm** (không cộng dồn số lượng).
*   **Vật lý nhặt đồ**: Để nhặt vật phẩm, bạn phải đảm bảo thành phần **Box Collider** trên prefab vật phẩm (như chiếc hộp `Key` hay `Battery`) đã được **tích chọn `Is Trigger`**. Nhân vật đi xuyên qua vật phẩm sẽ kích hoạt sự kiện nhặt đồ, cập nhật UI và tự động đồng bộ lên Database.
*   **Đoạn code hỗ trợ va chạm vật lý thường (OnCollisionEnter)**: Nếu bạn quên tích chọn `Is Trigger`, hệ thống va chạm vẫn hỗ trợ tự nhận diện và nhặt đồ khi bạn đi tông trực tiếp vào vật phẩm.

### 3. Cấu trúc CSDL MongoDB (Collection: Inventories)
Mỗi tài khoản người chơi sẽ sở hữu một bản ghi lưu trữ hòm đồ dạng:

```json
{
  "_id": ObjectId("6a39210f7924ab828370fb50"),
  "PlayerId": ObjectId("6a39210f7924ab828370fb4e"),
  "Items": [
    {
      "ItemId": "key",
      "Name": "key",
      "Quantity": 1
    }
  ]
}
```
