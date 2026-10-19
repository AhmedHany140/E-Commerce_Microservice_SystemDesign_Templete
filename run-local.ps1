param(
    [switch]$WithRabbitMq,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$repo = $PSScriptRoot
Set-Location $repo

# Load one repository-level .env so secrets/configuration are entered once.
$envFile = Join-Path $repo '.env'
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line -and -not $line.StartsWith('#') -and $line.Contains('=')) {
            $parts = $line.Split('=', 2)
            [Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim(), 'Process')
        }
    }
}

$jwtKey = $env:JWT_KEY
if ($jwtKey) { $env:Jwt__Key = $jwtKey }
if ($env:JWT_ISSUER) { $env:Jwt__Issuer = $env:JWT_ISSUER }
if ($env:JWT_AUDIENCE) { $env:Jwt__Audience = $env:JWT_AUDIENCE }
if ($env:SMTP_USERNAME) { $env:SmtpSettings__Username = $env:SMTP_USERNAME }
if ($env:SMTP_PASSWORD) { $env:SmtpSettings__Password = $env:SMTP_PASSWORD }
if ($env:PAYMOB_API_KEY) { $env:Paymob__ApiKey = $env:PAYMOB_API_KEY }
if ($env:PAYMOB_HMAC_SECRET) { $env:Paymob__HmacSecret = $env:PAYMOB_HMAC_SECRET }
if ($env:PAYMOB_CARD_INTEGRATION_ID -as [int]) { $env:Paymob__CardIntegrationId = $env:PAYMOB_CARD_INTEGRATION_ID }
if ($env:PAYMOB_WALLET_INTEGRATION_ID -as [int]) { $env:Paymob__WalletIntegrationId = $env:PAYMOB_WALLET_INTEGRATION_ID }
if ($env:PAYMOB_KIOSK_INTEGRATION_ID -as [int]) { $env:Paymob__KioskIntegrationId = $env:PAYMOB_KIOSK_INTEGRATION_ID }
if ($env:PAYMOB_IFRAME_ID -as [int]) { $env:Paymob__IframeId = $env:PAYMOB_IFRAME_ID }

if ($WithRabbitMq) {
    if ($env:RABBITMQ_URL) {
        $rabbitUri = [Uri]$env:RABBITMQ_URL
        $rabbitUser = [Uri]::UnescapeDataString($rabbitUri.UserInfo.Split(':', 2)[0])
        $rabbitPassword = [Uri]::UnescapeDataString($rabbitUri.UserInfo.Split(':', 2)[1])
        $rabbitHost = $rabbitUri.Host
        $rabbitVhost = $rabbitUri.AbsolutePath.TrimStart('/')
    } elseif ($env:RABBITMQ_USERNAME -and $env:RABBITMQ_PASSWORD) {
        $rabbitUser = $env:RABBITMQ_USERNAME
        $rabbitPassword = $env:RABBITMQ_PASSWORD
        $rabbitHost = 'localhost'
        $rabbitVhost = ''
        $rabbitUri = [Uri]("amqp://$rabbitUser`:$rabbitPassword@localhost:5672/")
    } else {
        throw 'Set RABBITMQ_URL or RABBITMQ_USERNAME/RABBITMQ_PASSWORD in the repository .env first.'
    }
    $env:RabbitMq__Enabled = 'true'
    $env:RabbitMq__Url = $rabbitUri.AbsoluteUri
    $env:RabbitMQ__Enabled = 'true'
    $env:RabbitMQ__Host = $rabbitHost
    $env:RabbitMQ__Username = $rabbitUser
    $env:RabbitMQ__Password = $rabbitPassword
    $env:RabbitMQ__VirtualHost = $rabbitVhost
}

$services = @(
    @{ Name='auth'; Project='E-Commerce_Auth_Service/ECommerce.AuthService.Api/ECommerce.AuthService.Api.csproj'; Port=5051 },
    @{ Name='cart'; Project='E-Commerce_CartService/ECommerce.CartService.Api/ECommerce.CartService.Api.csproj'; Port=5057 },
    @{ Name='product'; Project='E-Commerce_Product_Service/ECommerce.ProductService.Api/ECommerce.ProductService.Api.csproj'; Port=5090 },
    @{ Name='order'; Project='E-Commerce_OrderService/ECommerce.OrderService.Api/ECommerce.OrderService.Api.csproj'; Port=5072 },
    @{ Name='payment'; Project='E-Commerce-PaymentService/PaymentService.Api/PaymentService.Api.csproj'; Port=5113 },
    @{ Name='email'; Project='E-Commerce_Email_Service/E-Commerce_Email_Service.csproj'; Port=5103 },
    @{ Name='query-gateway'; Project='E-Commerce-QueryGatewayDemo/QueryGateqay/QueryGateqay.csproj'; Port=5008 },
    @{ Name='gateway'; Project='E-Commerce_GatewayDemo/E-Commerce-Gateway/E-Commerce-Gateway.csproj'; Port=5046 }
)

foreach ($service in $services) {
    $arguments = @('run', '--project', $service.Project)
    if ($service.Name -eq 'payment') { $arguments += @('--launch-profile', 'https') }
    if ($NoBuild) { $arguments += '--no-build' }
    Start-Process -FilePath 'dotnet' -ArgumentList $arguments -WorkingDirectory $repo -WindowStyle Normal
    Write-Host "Started $($service.Name) on http://localhost:$($service.Port)"
}

Write-Host 'Gateway: http://localhost:5046'
Write-Host 'GraphQL through Gateway: http://localhost:5046/graphql'
