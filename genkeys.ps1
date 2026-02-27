$rng = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
$encBytes = New-Object byte[] 32
$jwtBytes = New-Object byte[] 48
$rng.GetBytes($encBytes)
$rng.GetBytes($jwtBytes)
Write-Host "ENC_KEY=$([Convert]::ToBase64String($encBytes))"
Write-Host "JWT_KEY=$([Convert]::ToBase64String($jwtBytes))"
