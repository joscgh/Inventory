# Levanta la API y la APP en HTTPS para usar Inventario como app en el teléfono (red local).
# Uso: clic derecho > "Ejecutar con PowerShell", o desde una terminal:  .\start-app.ps1
#
# Requisitos: PostgreSQL corriendo y el teléfono en el mismo WiFi que esta PC.

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Detecta la IP local (WiFi/Ethernet) para mostrar la URL que se abre en el teléfono.
$ip = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -notlike '169.*' -and $_.IPAddress -ne '127.0.0.1' -and $_.PrefixOrigin -eq 'Dhcp' } |
    Select-Object -First 1).IPAddress
if (-not $ip) { $ip = "TU-IP-LOCAL" }

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Iniciando Inventario (API + APP) en HTTPS..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host " En el TELEFONO (mismo WiFi), abre primero la API y acepta" -ForegroundColor Yellow
Write-Host " el aviso de seguridad una vez:" -ForegroundColor Yellow
Write-Host "   https://$($ip):5130/swagger" -ForegroundColor White
Write-Host ""
Write-Host " Luego abre la app e instálala en la pantalla de inicio:" -ForegroundColor Yellow
Write-Host "   https://$($ip):5113" -ForegroundColor White
Write-Host ""

# Arranca la API en una ventana aparte.
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\Inventory.API'; dotnet run --launch-profile https"

# Arranca la APP en esta ventana.
Set-Location "$root\Inventory.APP"
dotnet run --launch-profile https
