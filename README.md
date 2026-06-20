# Hướng dẫn Khởi chạy và Kiểm thử MongoDB API Backend (Night Shift Asylum)

Dự án này là Backend Web API viết bằng **C# ASP.NET Core** kết nối tới **MongoDB** phục vụ cho Game Client Unity (Đăng ký, Đăng nhập, Quản lý phòng, Túi đồ, và Điểm số trận đấu).

---

## 1. Yêu cầu hệ thống
1. **.NET 10.0 SDK & Runtime** (Đã cài đặt sẵn trên máy).
2. **MongoDB Community Server** chạy tại localhost:
   - Địa chỉ mặc định: `mongodb://localhost:27017`
3. **MongoDB Compass** (Giao diện trực quan để xem dữ liệu).

---

## 2. Cách khởi chạy Server API Backend

> [!WARNING]
> **QUAN TRỌNG**: Không chạy lệnh này trực tiếp trong các cửa sổ terminal giả lập của AI/IDE vì tiến trình sẽ tự động tắt ngay lập tức do thiếu luồng nhập liệu (`standard input`). Bạn **bắt buộc** phải chạy trên PowerShell hoặc Cmd chính thức của Windows.

### Các bước thực hiện:
1. Bấm phím `Windows + R`, gõ `powershell` (hoặc `cmd`) và nhấn **Enter** để mở terminal của Windows.
2. Di chuyển đến thư mục chứa mã nguồn backend:
   ```powershell
   cd "d:\unity\Night-Shift-Asylum-2026-06-17-17-36-44\Assets\Script\API"
   ```
3. Chạy lệnh để khởi chạy Server:
   ```powershell
   dotnet run
   ```
   *Hoặc chạy file DLL trực tiếp:*
   ```powershell
   dotnet bin/Debug/net10.0/API.dll
   ```
4. Khi màn hình hiện thông báo sau tức là server đã hoạt động thành công và đang lắng nghe kết nối:
   ```text
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5000
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   ```
5. **Giữ nguyên cửa sổ terminal này** (không tắt) trong suốt quá trình chơi game hoặc kiểm thử.

---

## 3. Cách chạy Kiểm thử tự động (Automation Test Script)

Chúng tôi đã viết sẵn một script kiểm thử tự động bằng PowerShell để test nhanh tất cả các tính năng của database MongoDB mà không cần cài đặt Postman.

### Các bước chạy test:
1. Mở một cửa sổ **PowerShell mới** (giữ nguyên cửa sổ chạy server ở Bước 2).
2. Di chuyển đến thư mục chứa API:
   ```powershell
   cd "d:\unity\Night-Shift-Asylum-2026-06-17-17-36-44\Assets\Script\API"
   ```
3. Chạy file script kiểm thử:
   ```powershell
   .\test_api.ps1
   ```

### Các kịch bản kiểm thử mà script tự động thực hiện:
1. **Register**: Đăng ký một tài khoản ngẫu nhiên mới (`testplayer_xxxx`).
2. **Login**: Đăng nhập tài khoản đó để lấy chuỗi **JWT Token** xác thực.
3. **Get Profile**: Dùng Token truy vấn Profile người chơi để xác nhận bảng `PlayerProfiles` đã tự động khởi tạo dữ liệu mặc định (Level 1, Escapes 0, Fails 0).
4. **Get Inventory**: Dùng Token truy vấn Inventory để xác nhận bảng `Inventories` đã tự động khởi tạo túi đồ trống.
5. **Create Room**: Gửi yêu cầu tạo phòng chơi game mới lên MongoDB.

---

## 4. Kiểm tra dữ liệu trên MongoDB Compass
1. Mở phần mềm **MongoDB Compass** trên máy tính.
2. Nhập URI kết nối: `mongodb://localhost:27017` và nhấn **Connect**.
3. Tại danh sách Database ở cột trái, chọn **`NightShiftDb`**.
4. Bạn sẽ thấy 5 bảng dữ liệu (Collections) đã được tạo và chứa đầy đủ dữ liệu thử nghiệm:
   - **`Users`**: Chứa thông tin tài khoản và mật khẩu đã được mã hóa dạng **SHA256**.
   - **`PlayerProfiles`**: Chứa thông tin nickname và level.
   - **`Inventories`**: Chứa túi đồ của người chơi.
   - **`Rooms`**: Chứa danh sách phòng chơi game.
   - **`GameScores`**: Nhật ký lịch sử các trận đấu thắng/thua.

---

## 5. Danh sách các API Endpoint để tích hợp vào Unity

*Tất cả các API yêu cầu Authorization cần truyền Header: `Authorization: Bearer <JWT_TOKEN>`*

| Chức năng | Method | Endpoint | Body (JSON) / Ghi chú |
| :--- | :--- | :--- | :--- |
| **Đăng ký** | `POST` | `/api/auth/register` | `{"username": "...", "password": "..."}` |
| **Đăng nhập** | `POST` | `/api/auth/login` | Trả về JSON chứa `token` xác thực |
| **Lấy Profile của mình** | `GET` | `/api/playerprofiles/me` | *Cần Token* |
| **Cập nhật Profile** | `PUT` | `/api/playerprofiles/me` | *Cần Token*. Cập nhật Nickname, Level... |
| **Cập nhật túi đồ** | `PUT` | `/api/inventories/me` | *Cần Token*. Truyền mảng danh sách Item |
| **Thêm 1 vật phẩm** | `POST` | `/api/inventories/me/items` | *Cần Token*. `{"itemId": "...", "name": "...", "quantity": 1}` |
| **Tạo phòng** | `POST` | `/api/rooms/create` | `{"roomName": "...", "maxPlayers": 4}` |
| **Vào phòng** | `POST` | `/api/rooms/join/{id}` | *Cần Token*. Tự động thêm người chơi vào phòng |
| **Rời phòng** | `POST` | `/api/rooms/leave/{id}` | *Cần Token*. Tự động xóa người chơi khỏi phòng |
| **Lưu điểm số trận đấu** | `POST` | `/api/gamescores` | Ghi nhận lịch sử trận đấu thắng/thua |
# GNS-API
