# Azure Key Vault - Getting Started in .NET
Azure Key Vault helps safeguard cryptographic keys and secrets used by cloud applications and services. By using Key Vault, you can encrypt keys and secrets (such as authentication keys, storage account keys, data encryption keys, .PFX files, and passwords) using keys protected by hardware security modules (HSMs).

In this example you learn how to:
- **How to authenticate to Key Vault?**
- **How to create a secret in Key Vault?**
- **How to read a secret from Key Vault?**

These are the steps in doing so
- Create an application in Azure Active Directory
- Create a Service Principal associated with the application 
- Create a Key Vault
- Give permissions to the Service Principal created above to access Key Vault 
- Create a secret in Key Vault
- Read the secret from Key Vault

### Prerequisites
If you don't have an Azure subscription, please create a **[free account](https://azure.microsoft.com/free/?ref=microsoft.com&amp;utm_source=microsoft.com&amp;utm_medium=docs)** before you begin.
In addition you would need

* [.NET Core](https://www.microsoft.com/net/learn/get-started/windows)
    * Please install .NET Core. This can be run on Windows, Mac and Linux.
* [Git](https://www.git-scm.com/)
    * Please download git from [here](https://git-scm.com/downloads).
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
    * For the purpose of this tutorial we would work with Azure CLI which is available on Windows, Mac and Linux
* [Powershell](https://docs.microsoft.com/en-us/powershell/azure/install-azurerm-ps?view=azurermps-6.1.0&viewFallbackFrom=azurermps-5.5.0)
    * Please run the following commands in powershell (Admin mode)
    ```
    Get-Module -Name PowerShellGet -ListAvailable | Select-Object -Property Name,Version,Path

    Install-Module PowerShellGet -Force
    
    # Install the Azure Resource Manager modules from the PowerShell Gallery
    Install-Module -Name AzureRM -AllowClobber
    ```
    By default, the PowerShell gallery is not configured as a Trusted repository for PowerShellGet. The first time you use the PSGallery you see the following prompt

    ```
    Untrusted repository

    You are installing the modules from an untrusted repository. If you trust this repository, change
    its InstallationPolicy value by running the Set-PSRepository cmdlet.
    
    Are you sure you want to install the modules from 'PSGallery'?
    [Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "N"): Y
    ```

    Answer 'Yes' or 'Yes to All' to continue with the installation.


## Quickstart
### On Windows
- Clone this [repo](https://github.com/prashanthyv/key-vault-dotnet-quickstart) by running 
    ```
    git clone https://github.com/prashanthyv/key-vault-dotnet-quickstart.git
    ```
    
- Download the powershell file locally from this [repo](https://github.com/prashanthyv/key-vault-dotnet-quickstart) (Named Setup.ps1) and run it in administrator mode
- Go to C:\Temp folder by using cd C:\temp command. 
- Find the cert named .pfx in that folder and install it on your machine as "Current User" (by right clicking on the .pfx file and selecting Install)
- Once cloned open the repo in any text editor and run the following command w.r.t that folder
    ```
    dotnet run
    ```
    

### On Mac/Linux
- This quickstart requires that you are running the Azure CLI version 2.0.4 or later. To find the version, run `az --version`. If you need to install or upgrade, see [Install Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).
- First we want to download / clone this [repo](https://github.com/prashanthyv/key-vault-dotnet-quickstart)
- Then cd into dotnetconsole folder
- We then want to set these variables by running the following commands
    - On Windows (Use set)
    - On Mac/Linux (Use export instead of set)
    <br />
    ```
        export SERVICE_PRINCIPAL_NAME="service_principal_name" 
    ```
    <br />
    ```
        export RESOURCE_GROUP_NAME="resource_group_name"
    ```
    <br />
    ```
        export VAULT_NAME="vault_name"
    ```
- This command creates a self signed certificate. It also creates an Application (service principal) in AAD and assigns this self signed certificate as it's key

    ```
    az ad sp create-for-rbac -n $SERVICE_PRINCIPAL_NAME --create-cert > ServicePrincipal.json
    ```
    
    output of the `create-for-rbac` command is in the following format:
    
    ```json
    {
      "appId": "APP_ID",
      "displayName": "ServicePrincipalName",
      "fileWithCertAndPrivateKey" : "PathToYourPrivateKey",
      "name": "http://ServicePrincipalName",
      "password": ...,
      "tenant": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
    }
    ```

    > [!NOTE]
    > Please make a copy of the APP_ID from the ServicePrincipal.json file output

    We run the following command to find the thumbprint of the cert that's just created (you can find this from PathToYourPrivateKey shown in the previous result)

    ```
    openssl x509 -in <CERTNAME>.pem -noout -sha1 -fingerprint > CertThumbprint.txt
    ```
    
    The `appId`, `tenant` values are used for authentication. The `displayName` is used when searching for an existing service principal. Please make a copy of the `appId` as you will need it later.
    
    > [!NOTE]
    > If your account does not have sufficient permissions to create a service principal, you see an error message containing "Insufficient privileges to complete the operation." Contact your Azure Active Directory admin to create a service principal.

- Before deploying any resources to your subscription, you must create a resource group that will contain the resources. 

    ```
    az group create --name $RESOURCE_GROUP_NAME --location "East US"
    ```

- [This command creates a Key Vault in the specified Resource Group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/xplat-cli-azure-resource-manager#create-a-resource-group)
(Please replace the VaultName and ResourceGroupName with values you choose).
    ```
    az keyvault create --name $VAULT_NAME --resource-group $RESOURCE_GROUP_NAME --location eastus > KeyVault.json
    ```
    
    To authorize the above created application to read secrets in your vault, run the following:
    
    ```
    az keyvault set-policy --name $VAULT_NAME --spn APP_ID --secret-permissions get
    ```

- Once done, with above commands clone this [repo](https://github.com/prashanthyv/key-vault-dotnet-quickstart) by running the following command
    ```
    git clone https://github.com/prashanthyv/key-vault-dotnet-quickstart.git
    ```

    Then cd into that folder and run dotnet run

    ```
    dotnet run
    ```
You should see the secret Key Value pair set and retrieved


### What does this code do?
- This sample will show you how to create a test key and secret in Key Vault
- It will also show you how to retrieve the secret from Key Vault

## Resources
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
- [Developer Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
