# Night Shift: Asylum Backend

ASP.NET Core 8 + MongoDB cung cấp:

- JWT authentication và role `Player`/`Admin`.
- Player profile, inventory và match history.
- Room discovery cho Unity NGO Host/Client.
- Global lobby chat qua asynchronous TCP socket.

## Chạy local

Yêu cầu:

- .NET 8 runtime/targeting pack.
- MongoDB tại `mongodb://localhost:27017`.

```powershell
cd "D:\Unity\Night-Shift-Asylum-Workspace\Backend-Server"
dotnet restore
dotnet run
```

Server lắng nghe:

- REST API: `http://localhost:5000`
- TCP global chat: `localhost:5001`
- Health check: `GET http://localhost:5000/health`

Chạy test sau khi MongoDB và backend đã hoạt động:

```powershell
.\test_api.ps1
```

## Luồng Unity + NGO

1. Client đăng ký/đăng nhập và nhận JWT.
2. Host gọi `POST /api/rooms/create`, backend lưu room cùng IP/port NGO.
3. Client lấy danh sách qua `GET /api/rooms`.
4. Client gọi `POST /api/rooms/join/{id}`, nhận `hostAddress` và `port`.
5. Unity cấu hình `UnityTransport`, đưa JWT vào NGO connection payload rồi kết nối.
6. Host gọi `/api/auth/session` bằng JWT của client trong Connection Approval.
7. Khi bắt đầu/kết thúc trận, host cập nhật room status và ghi match result.
8. Khi rời/disconnect, room membership được dọn khỏi MongoDB.

> Direct NGO Host/Client cần port host có thể truy cập từ client. Với máy khác trong LAN,
> đặt `APIManager.baseUrl` thành IP máy chạy backend thay vì `localhost`. Internet/NAT thực tế
> cần port forwarding hoặc Unity Relay.

## REST API

Header cho endpoint riêng tư:

```text
Authorization: Bearer <JWT_TOKEN>
```

| Chức năng | Method | Endpoint | Auth |
|---|---:|---|---:|
| Đăng ký | POST | `/api/auth/register` | Không |
| Đăng nhập | POST | `/api/auth/login` | Không |
| Kiểm tra session/JWT | GET | `/api/auth/session` | Player |
| Lấy profile | GET | `/api/playerprofiles/me` | Player |
| Đổi nickname | PUT | `/api/playerprofiles/me` | Player |
| Cộng thắng/thua | POST | `/api/playerprofiles/me/stats` | Player |
| Lấy inventory | GET | `/api/inventories/me` | Player |
| Thay inventory | PUT | `/api/inventories/me` | Player |
| Thêm item | POST | `/api/inventories/me/items` | Player |
| Danh sách room đang chờ | GET | `/api/rooms` | Player |
| Chi tiết room | GET | `/api/rooms/{id}` | Player |
| Tạo room | POST | `/api/rooms/create` | Player |
| Join room | POST | `/api/rooms/join/{id}` | Player |
| Leave room | POST | `/api/rooms/leave/{id}` | Player |
| Đổi room status | PUT | `/api/rooms/{id}/status` | Host |
| Host xóa member disconnect | POST | `/api/rooms/{id}/remove/{memberId}` | Host |
| Lấy lịch sử bản thân | GET | `/api/gamescores/me` | Player |
| Ghi kết quả trận | POST | `/api/gamescores` | Player |

### Body mẫu

```json
{
  "email": "player@example.com",
  "username": "player01",
  "password": "Password123!"
}
```

```json
{
  "roomName": "Ward B",
  "maxPlayers": 4,
  "port": 7777
}
```

```json
{
  "itemId": "battery",
  "name": "Battery",
  "quantity": 1
}
```

## TCP global chat protocol

Kết nối TCP đến port `5001`, mỗi frame text kết thúc bằng newline:

```text
AUTH|<JWT>
hello everyone
```

Server trả:

```text
SYSTEM|CONNECTED|player01
CHAT|player01|hello everyone
```
