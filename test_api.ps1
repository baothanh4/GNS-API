$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5000/api"
$suffix = Get-Random -Minimum 1000 -Maximum 9999
$username = "testplayer_$suffix"
$password = "Password123!"

Write-Host "Testing backend as $username" -ForegroundColor Cyan

$register = @{
    email = "$username@example.com"
    username = $username
    password = $password
} | ConvertTo-Json
Invoke-RestMethod "$baseUrl/auth/register" -Method Post `
    -Body $register -ContentType "application/json" | Out-Null

$login = @{ username = $username; password = $password } | ConvertTo-Json
$auth = Invoke-RestMethod "$baseUrl/auth/login" -Method Post `
    -Body $login -ContentType "application/json"
$headers = @{ Authorization = "Bearer $($auth.token)" }

$session = Invoke-RestMethod "$baseUrl/auth/session" -Headers $headers
$profile = Invoke-RestMethod "$baseUrl/playerprofiles/me" -Headers $headers
$inventory = Invoke-RestMethod "$baseUrl/inventories/me" -Headers $headers

$item = @{ itemId = "battery"; name = "Battery"; quantity = 1 } | ConvertTo-Json
Invoke-RestMethod "$baseUrl/inventories/me/items" -Method Post `
    -Headers $headers -Body $item -ContentType "application/json" | Out-Null

$roomBody = @{
    roomName = "Automation Room"
    maxPlayers = 4
    port = 7777
} | ConvertTo-Json
$room = Invoke-RestMethod "$baseUrl/rooms/create" -Method Post `
    -Headers $headers -Body $roomBody -ContentType "application/json"

$rooms = Invoke-RestMethod "$baseUrl/rooms" -Headers $headers
$status = @{ status = "Playing" } | ConvertTo-Json
Invoke-RestMethod "$baseUrl/rooms/$($room.id)/status" -Method Put `
    -Headers $headers -Body $status -ContentType "application/json" | Out-Null

$score = @{
    matchId = $room.id
    players = @($session.userId)
    escapeTimeSeconds = 123
    result = "Victory"
} | ConvertTo-Json
Invoke-RestMethod "$baseUrl/gamescores" -Method Post `
    -Headers $headers -Body $score -ContentType "application/json" | Out-Null

Invoke-RestMethod "$baseUrl/rooms/leave/$($room.id)" -Method Post `
    -Headers $headers -Body "{}" -ContentType "application/json" | Out-Null

Write-Host "PASS: auth/profile/inventory/room/score flow completed." -ForegroundColor Green
Write-Host "Rooms returned: $($rooms.rooms.Count), Profile: $($profile.nickname), Items before: $($inventory.items.Count)"
