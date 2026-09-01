# Genera el certificado HTTPS de desarrollo para la API y la PWA.
#
# El certificado debe incluir la IP de la LAN en el SAN: sin ella el teléfono
# rechaza https://<IP>:5114 y https://<IP>:5131 por nombre no coincidente.
#
# Uso:
#   .\create-local-dev-cert.ps1                 # detecta la IP de la LAN
#   .\create-local-dev-cert.ps1 -HostName 192.168.0.100
#   .\create-local-dev-cert.ps1 -HostName 192.168.0.100,inventario.local

param(
    [string[]]$HostName = @(),
    [string]$Password = "Inventory2026!",
    # No intenta añadir la CA al almacén de confianza de este PC (evita el diálogo
    # de confirmación de Windows). El .cer se exporta igual para el teléfono.
    [switch]$SkipTrust
)

$ErrorActionPreference = "Stop"
$certDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$leafPfx = Join-Path $certDir "inventory-dev.pfx"
$rootCer = Join-Path $certDir "inventory-dev-root.cer"

New-Item -ItemType Directory -Path $certDir -Force | Out-Null

# Si no se indicó ningún host, agrega la IP local para que el teléfono confíe en la URL.
if (-not $HostName -or $HostName.Count -eq 0) {
    $detected = (Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.IPAddress -notlike '169.*' -and $_.IPAddress -ne '127.0.0.1' -and $_.PrefixOrigin -eq 'Dhcp' } |
        Select-Object -First 1).IPAddress
    if ($detected) {
        $HostName = @($detected)
        Write-Host "IP local detectada: $detected" -ForegroundColor DarkGray
    }
    else {
        $HostName = @()
        Write-Warning "No se detectó una IP de LAN. El certificado sólo servirá para localhost."
    }
}

$rootCertName = "Inventory Local Dev Root CA"
$leafCertName = "Inventory Local Dev Cert"

# La CA se crea siempre en CurrentUser\My: New-SelfSignedCertificate no admite
# crear directamente en otro almacén ("A new certificate can only be installed
# into MY store"). Después se copia al almacén de raíces de confianza.
$existingRoot = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq "CN=$rootCertName" -and $_.NotAfter -gt (Get-Date) } |
    Select-Object -First 1

if (-not $existingRoot) {
    $existingRoot = New-SelfSignedCertificate `
        -CertStoreLocation Cert:\CurrentUser\My `
        -Type Custom `
        -KeySpec Signature `
        -Subject "CN=$rootCertName" `
        -FriendlyName $rootCertName `
        -TextExtension @("2.5.29.19={text}ca=1&pathlength=1") `
        -KeyUsage CertSign,CRLSign `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -NotAfter (Get-Date).AddYears(5)
}

$alreadyTrusted = Get-ChildItem Cert:\CurrentUser\Root |
    Where-Object { $_.Thumbprint -eq $existingRoot.Thumbprint } |
    Select-Object -First 1

if (-not $alreadyTrusted -and -not $SkipTrust) {
    try {
        # Windows pide confirmación al añadir una raíz de confianza: hay que aceptar una vez.
        $rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "CurrentUser")
        $rootStore.Open("ReadWrite")
        $rootStore.Add($existingRoot)
        $rootStore.Close()
        Write-Host "CA añadida a las raíces de confianza de este usuario." -ForegroundColor DarkGray
    }
    catch {
        Write-Warning "No se pudo añadir la CA al almacén de confianza: $($_.Exception.Message). El navegador de escritorio mostrará un aviso de seguridad."
    }
}

# Todos los nombres van en la extensión SAN. No se usa -DnsName porque
# New-SelfSignedCertificate no admite ambas formas de declarar el SAN a la vez.
$sanEntries = @("DNS=localhost", "IPAddress=127.0.0.1")
foreach ($name in $HostName) {
    if ([string]::IsNullOrWhiteSpace($name)) { continue }
    $trimmed = $name.Trim()
    if ($trimmed -match '^\d{1,3}(\.\d{1,3}){3}$') {
        $entry = "IPAddress=$trimmed"
    }
    else {
        $entry = "DNS=$trimmed"
    }
    if ($sanEntries -notcontains $entry) { $sanEntries += $entry }
}

$sanExtension = "2.5.29.17={text}" + ($sanEntries -join "&")

$leafCert = New-SelfSignedCertificate `
    -CertStoreLocation Cert:\CurrentUser\My `
    -Type SSLServerAuthentication `
    -Subject "CN=localhost" `
    -FriendlyName $leafCertName `
    -TextExtension @($sanExtension) `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -Signer $existingRoot `
    -NotAfter (Get-Date).AddYears(1)

Export-PfxCertificate -Cert $leafCert -FilePath $leafPfx -Password (ConvertTo-SecureString $Password -AsPlainText -Force) | Out-Null
Export-Certificate -Cert $existingRoot -FilePath $rootCer | Out-Null

Write-Host "Certificado generado:" -ForegroundColor Green
Write-Host "  PFX: $leafPfx" -ForegroundColor Green
Write-Host "  CA:  $rootCer" -ForegroundColor Green
Write-Host "  SAN: $($sanEntries -join ', ')" -ForegroundColor Green
Write-Host ""
Write-Host "En el teléfono, instala inventory-dev-root.cer como certificado de CA para que el navegador no muestre advertencias." -ForegroundColor Yellow
