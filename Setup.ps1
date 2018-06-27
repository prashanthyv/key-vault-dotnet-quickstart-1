# **********************************************************************************************
# This sample PowerShell script does the following
# create a self signed certificate
# create an Application within AAD 
# Assign the self signed certificate as a key to the AAD Application
# Create a Resource Group and Key Vault
# Give the AAD Application permissions to read from your Key Vault
# **********************************************************************************************


Write-Host 'Vault name must be between 3-24 alphanumeric characters. The name must begin with a letter, end with a letter or digit, and not contain consecutive hypens' -foregroundcolor Yellow
$vaultName = Read-Host -Prompt 'Please input a Vault Name'
$resourceGroupName = Read-Host -Prompt 'Please input a Azure Resource Group Name'
$applicationName = Read-Host -Prompt 'Please input application name in Azure Active Directory'
$identifierUri = Read-Host -Prompt 'Please input an Identifier Uri (As an example https://microsoft.com'
$CertName = Read-Host -Prompt 'Please input a name for the Self Signed Certificate'
$pwdresponse = Read-host "Please input a password for the Self Signed Certificate" -AsSecureString 
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pwdresponse))

[System.Environment]::SetEnvironmentVariable('VAULT_NAME', $vaultName, [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('RESOURCE_GROUP_NAME', $resourceGroupName, [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('APPLICATION_NAME', $applicationName, [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('CERT_NAME', $CertName, [System.EnvironmentVariableTarget]::User)

[System.Environment]::SetEnvironmentVariable('IDENTIFIER_URI', $identifierUri, [System.EnvironmentVariableTarget]::User)

# **********************************************************************************************
# You MAY set the following values before running this script
# **********************************************************************************************
$location            = 'East US'                          # Get-AzureLocation
$dnsName             = 'mytest.domain.com'
$tempFolder = "C:\temp\"

# **********************************************************************************************
# Create a self signed cert
# **********************************************************************************************
Write-Host 'Creating a Self Signed Certificate named ' $CertName -foregroundcolor Green
$cert = New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dnsname $dnsName
$securepwd = ConvertTo-SecureString -String $password -Force -AsPlainText
$path = "cert:\localMachine\my\" + $cert.thumbprint

If(!(test-path $tempFolder))
{
      New-Item -ItemType Directory -Force -Path $tempFolder
}
$certThumbprint = $cert.thumbprint
$certStore = "Cert:\localMachine\my"

# **********************************************************************************************
# Export the self signed cert to temp folder
# **********************************************************************************************
Write-Host 'Exporting a Self Signed Certificate named ' $CertName  'to C:\temp folder' -foregroundcolor Green
Export-PfxCertificate -cert $path -FilePath C:\temp\$CertName.pfx -Password $securepwd 
Export-Certificate -cert $path -FilePath C:\temp\$CertName.crt

# **********************************************************************************************
# Import certificate into certificate store on Windows
# **********************************************************************************************
# Import-PfxCertificate -FilePath C:\temp\$CertName.pfx -CertStoreLocation Cert:\LocalMachine\My -Password $securepwd

Set-Location -Path C:\Temp
$x509 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
$x509.Import([string]::Concat($tempFolder,$CertName,".crt"))
$credValue = [System.Convert]::ToBase64String($x509.GetRawCertData())
$validFrom = [System.DateTime]::Now
$validTo = [System.DateTime]::Now.AddDays(5)

# **********************************************************************************************
# Login to Azure
# **********************************************************************************************
Write-Host 'Logging into Azure' -foregroundcolor Green
Login-AzureRmAccount 

# **********************************************************************************************
# Create an Application in Azure Active Directory
# **********************************************************************************************
Write-Host 'Creating an Application named'  $applicationName ' in Azure Active Directory ' -foregroundcolor Green
$adapp = New-AzureRmADApplication -DisplayName "$applicationName" -HomePage "https://keyvaultreader.com/" -IdentifierUris $identifierUri -CertValue $credValue ` -StartDate $validFrom -EndDate $validTo


# **********************************************************************************************
# Create a Service Principal associated with the Application in Azure Active Directory
# **********************************************************************************************
Write-Host 'Creating an Service Principal for an Application ' $applicationName ' in Azure Active Directory' -foregroundcolor Green
$ServicePrincipal = New-AzureRmADServicePrincipal -ApplicationId $adapp.ApplicationId

[System.Environment]::SetEnvironmentVariable('SP_OBJECT_ID', $ServicePrincipal.Id, [System.EnvironmentVariableTarget]::User)

# **********************************************************************************************
# Create a Key Vault with a specified Resource Group
# **********************************************************************************************
Write-Host 'Creating a Vault ' $vaultName 'with Specified Resource Group ' $resourceGroupName -foregroundcolor Green
New-AzureRmResourceGroup -Name $resourceGroupName -Location $location
New-AzureRmKeyVault -VaultName $vaultName -ResourceGroupName $resourceGroupName -Location $location


# **********************************************************************************************
# Setting permissions for the Application in AAD to have access to Key Vault Secrets, Keys, Certificates
# **********************************************************************************************
Set-AzureRmKeyVaultAccessPolicy -VaultName $vaultName -ResourceGroupName $resourceGroupName -ObjectId $ServicePrincipal.Id -PermissionsToSecrets get, set, delete 


[System.Environment]::SetEnvironmentVariable('APPLICATION_ID', $adapp.ApplicationId, [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('KEYVAULT_URI', $vaultName, [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('CERT_THUMBPRINT', $certThumbprint, [System.EnvironmentVariableTarget]::User)

Write-Host 'Cert Thumbprint ' $certThumbprint  'Application Name in AAD is ' $applicationName "Vault Name " $vaultName -foregroundcolor Green | Format-Table
