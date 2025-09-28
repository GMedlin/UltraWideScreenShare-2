# Create a self-signed certificate for MSIX package signing
# This script will be used in GitHub Actions to create a test certificate

param(
    [string]$CertName = "CN=UltraWideScreenShare2-Test",
    [string]$OutputPath = "UltraWideScreenShare2-Test.pfx",
    [string]$Password = "test123"
)

Write-Host "Creating test certificate for MSIX signing..."

try {
    # Import the PKI module
    Import-Module PKI -ErrorAction SilentlyContinue

    # Create the certificate in LocalMachine store for GitHub Actions
    $cert = New-SelfSignedCertificate -Subject $CertName -Type CodeSigningCert -CertStoreLocation "Cert:\LocalMachine\My" -HashAlgorithm SHA256

    # Convert password to secure string
    $securePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText

    # Export the certificate
    Export-PfxCertificate -Cert $cert -FilePath $OutputPath -Password $securePassword

    Write-Host "Certificate created successfully: $OutputPath"
    Write-Host "Certificate thumbprint: $($cert.Thumbprint)"
    Write-Host "Certificate password: $Password"

    # Remove from store
    Remove-Item -Path "Cert:\LocalMachine\My\$($cert.Thumbprint)" -Force

} catch {
    Write-Error "Failed to create certificate: $_"
    Write-Host "Falling back to creating placeholder certificate for workflow testing..."

    # Create a base64 encoded placeholder certificate for workflow testing
    $placeholderCert = @"
PLACEHOLDER_FOR_ACTUAL_CERTIFICATE
This file will be replaced with a real certificate in GitHub Actions
Certificate Name: $CertName
Password: $Password
"@

    Set-Content -Path $OutputPath -Value $placeholderCert
    Write-Host "Placeholder certificate created: $OutputPath"
}