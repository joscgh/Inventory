# Levanta Inventario para usarlo como app en el teléfono (red local).
# Uso: clic derecho > "Ejecutar con PowerShell", o desde una terminal:  .\start-app.ps1
#
# Requisitos: PostgreSQL corriendo y el teléfono en el mismo WiFi que esta PC.
#
# La API sirve también la PWA, así que app y API comparten un único origen: el
# teléfono sólo tiene que confiar en un certificado, y no hay CORS ni contenido
# mixto. Puertos:
#   5114 (https) -> app + API. Esta es la URL que se abre en el teléfono. Es el
#                   mismo puerto de siempre a propósito: la PWA ya instalada lo
#                   tiene grabado en su start_url y así el icono sigue sirviendo.
#   5130 (http)  -> lo mismo por HTTP, para la app nativa (APK) y pruebas locales.
#
# -WithDevServer arranca además el servidor de desarrollo de Blazor en el 5115,
# útil sólo para iterar el front en el escritorio con recarga en caliente.

param(
    [switch]$WithDevServer
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Detecta la IP local (WiFi/Ethernet) para mostrar la URL que se abre en el teléfono.
$ip = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -notlike '169.*' -and $_.IPAddress -ne '127.0.0.1' -and $_.PrefixOrigin -eq 'Dhcp' } |
    Select-Object -First 1).IPAddress
if (-not $ip) { $ip = "TU-IP-LOCAL" }

$certPath = Join-Path $root 'certs\inventory-dev.pfx'
$certPassword = 'Inventory2026!'

# El certificado tiene que incluir la IP de la LAN en el SAN; si no, el teléfono
# rechaza la conexión https y la PWA no puede llamar a la API.
function Test-CertCoversHost {
    param([string]$Path, [string]$Password, [string]$IpAddress)

    try {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $Path, $Password
    }
    catch {
        return $false
    }

    if ($cert.NotAfter -lt (Get-Date)) { return $false }

    $san = $cert.Extensions | Where-Object { $_.Oid.Value -eq '2.5.29.17' } | Select-Object -First 1
    if (-not $san) { return $false }

    return ($san.Format($false) -like "*$IpAddress*")
}

$needsCert = $true
if ((Test-Path $certPath) -and $ip -ne "TU-IP-LOCAL") {
    $needsCert = -not (Test-CertCoversHost -Path $certPath -Password $certPassword -IpAddress $ip)
    if ($needsCert) {
        Write-Host " El certificado actual no cubre la IP $ip. Se regenerará." -ForegroundColor Yellow
    }
}
elseif (Test-Path $certPath) {
    $needsCert = $false
}

if ($needsCert) {
    $createScript = Join-Path $root 'certs\create-local-dev-cert.ps1'
    if (Test-Path $createScript) {
        if ($ip -eq "TU-IP-LOCAL") {
            & $createScript -Password $certPassword
        }
        else {
            & $createScript -HostName $ip -Password $certPassword
        }
    }
    else {
        Write-Warning "No se encontró certs\create-local-dev-cert.ps1; se intentará con el certificado de dotnet dev-certs."
        dotnet dev-certs https --export-path $certPath --password $certPassword --format pfx | Out-Null
    }
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Iniciando Inventario..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host " En el TELEFONO (mismo WiFi), abre esta unica URL:" -ForegroundColor Yellow
Write-Host "      https://$($ip):5114" -ForegroundColor White
Write-Host ""
Write-Host " Acepta el aviso de seguridad una vez y luego instalala en la" -ForegroundColor Yellow
Write-Host " pantalla de inicio. La app y la API van en el mismo origen, asi" -ForegroundColor Yellow
Write-Host " que con aceptar ese aviso queda todo funcionando." -ForegroundColor Yellow
Write-Host ""
Write-Host " Para no ver ningun aviso, instala certs\inventory-dev-root.cer en" -ForegroundColor DarkGray
Write-Host " el telefono como certificado de CA (Ajustes > Seguridad > Cifrado)." -ForegroundColor DarkGray
Write-Host ""
Write-Host " Swagger:    https://$($ip):5114/swagger" -ForegroundColor DarkGray
Write-Host " App nativa: http://$($ip):5130" -ForegroundColor DarkGray
Write-Host " Certificado HTTPS local: $certPath" -ForegroundColor DarkGray
Write-Host ""

if ($WithDevServer) {
    # Certificado para el servidor de desarrollo de Blazor.
    $env:ASPNETCORE_Kestrel__Certificates__Default__Path = $certPath
    $env:ASPNETCORE_Kestrel__Certificates__Default__Password = $certPassword

    Write-Host " Servidor de desarrollo del front: https://$($ip):5115" -ForegroundColor DarkGray
    Write-Host ""

    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\Inventory.APP'; dotnet run --launch-profile https --urls 'https://0.0.0.0:5115'"
}

# La API sirve la app y la API: http://0.0.0.0:5130 y https://0.0.0.0:5131.
Set-Location "$root\Inventory.API"
dotnet run --launch-profile https
