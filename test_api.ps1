# PowerShell Script to test the C# MongoDB Web API Backend
# Ensure the backend is running before executing this script (dotnet run)

$baseUrl = "http://localhost:5000/api"
$testUsername = "testplayer_" + (Get-Random -Minimum 1000 -Maximum 9999)
$testPassword = "Password123!"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "STARTING BACKEND API INTEGRATION TEST" -ForegroundColor Cyan
Write-Host "Testing against: $baseUrl" -ForegroundColor Yellow
Write-Host "Testing credentials: $testUsername / $testPassword" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan

# Helper to format json output
function Show-Response($response) {
    return ($response | ConvertFrom-Json | ConvertTo-Json -Depth 5)
}

# 1. TEST USER REGISTRATION
Write-Host "`n1. Testing Register API (/auth/register)..." -ForegroundColor Cyan
$regBody = @{
    email = "$testUsername@example.com"
    username = $testUsername
    password = $testPassword
} | ConvertTo-Json

try {
    $regResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $regBody -ContentType "application/json"
    Write-Host "Success!" -ForegroundColor Green
    $regResponse | ConvertTo-Json | Write-Host -ForegroundColor Gray
} catch {
    Write-Host "Failed to register:" -ForegroundColor Red
    $_ | Write-Host -ForegroundColor Red
    exit
}

# 2. TEST USER LOGIN & JWT GENERATION
Write-Host "`n2. Testing Login API (/auth/login)..." -ForegroundColor Cyan
$loginBody = @{
    username = $testUsername
    password = $testPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "Success! JWT Token received." -ForegroundColor Green
    Write-Host "Token snippet: $($token.Substring(0, 30))..." -ForegroundColor Gray
} catch {
    Write-Host "Failed to login:" -ForegroundColor Red
    $_ | Write-Host -ForegroundColor Red
    exit
}

# 3. TEST RETRIEVING PROFILE (Requires JWT Token)
Write-Host "`n3. Testing Get Profile API (/playerprofiles/me)..." -ForegroundColor Cyan
$headers = @{
    Authorization = "Bearer $token"
}

try {
    $profileResponse = Invoke-RestMethod -Uri "$baseUrl/playerprofiles/me" -Method Get -Headers $headers
    Write-Host "Success! PlayerProfile is auto-created in MongoDB." -ForegroundColor Green
    $profileResponse | ConvertTo-Json | Write-Host -ForegroundColor Gray
} catch {
    Write-Host "Failed to fetch profile:" -ForegroundColor Red
    $_ | Write-Host -ForegroundColor Red
}

# 4. TEST RETRIEVING INVENTORY (Requires JWT Token)
Write-Host "`n4. Testing Get Inventory API (/inventories/me)..." -ForegroundColor Cyan
try {
    $inventoryResponse = Invoke-RestMethod -Uri "$baseUrl/inventories/me" -Method Get -Headers $headers
    Write-Host "Success! Inventory is auto-created in MongoDB." -ForegroundColor Green
    $inventoryResponse | ConvertTo-Json | Write-Host -ForegroundColor Gray
} catch {
    Write-Host "Failed to fetch inventory:" -ForegroundColor Red
    $_ | Write-Host -ForegroundColor Red
}

# 5. TEST ROOM CREATION
Write-Host "`n5. Testing Create Room API (/rooms/create)..." -ForegroundColor Cyan
$roomBody = @{
    roomName = "Test Game Room 1"
    maxPlayers = 4
} | ConvertTo-Json

try {
    $roomResponse = Invoke-RestMethod -Uri "$baseUrl/rooms/create" -Method Post -Body $roomBody -ContentType "application/json"
    Write-Host "Success! Room created in MongoDB." -ForegroundColor Green
    $roomResponse | ConvertTo-Json | Write-Host -ForegroundColor Gray
} catch {
    Write-Host "Failed to create room:" -ForegroundColor Red
    $_ | Write-Host -ForegroundColor Red
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "TESTS COMPLETED!" -ForegroundColor Green
Write-Host "Check your MongoDB Compass under 'NightShiftDb' to see the newly populated data." -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
