# Azure Dynamic DNS

## About The Project

In the connected world of today you sometimes need a simple way to connect to your home IP.

There are lots of free and great Dynamic DNS services available, however, it's lots more fun to run your own with your own custom domain name. With a few simple components you can manage your own DNS zone in Azure.

Included components:

* ARM template to deploy basic infrastructure:
  * [Azure DNS zone](https://azure.microsoft.com/en-us/services/dns/) for managing your domain
  * [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/) to hold function code
* [Azure Function](https://azure.microsoft.com/en-us/services/functions/) on a consumption plan to update your DNS

Current functionality is very basic, I'll be adding more in the near future. Feel free to suggest changes by forking this repo and creating a pull request or opening an issue.

### Built With

* [.NET Core 3.1](https://dotnet.microsoft.com/)
* [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)
* :coffee:

## Getting Started

To get a your own copy up and running follow these simple steps.

### Prerequisites

* [Azure Powershell](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-5.1.0)
* [Azure Subscription](https://azure.microsoft.com/en-us/free/)
* A domain name with the ability to update the NS servers

### Installation

1. Clone the repo

   ```powershell
   git clone https://github.com/Pauwelz/azure-dynamic-dns.git
   ```

2. Create Resource Group and deploy base infrastructure

   ```powershell
   cd .\azure-dynamic-dns\
   $rgName = "rg-dyndns-prod"
   $domainName = "domain.tld"
   New-AzResourceGroup -Name $rgName -Location 'France Central'
   New-AzResourceGroupDeployment -ResourceGroupName $rgName `
      -TemplateFile .\src\infra\infra.json -zoneName $domainName
   ```

3. Build and publish the function app

   ```powershell
   dotnet publish .\src\dnsmanagement\dnsmanagement.csproj -c Release
   Compress-Archive -Path .\src\dnsmanagement\bin\Release\netcoreapp3.1\publish\* `
      -DestinationPath dnsmanagement.zip -Force
   Publish-AzWebApp -ResourceGroupName $rgName -Name (Get-AzResourceGroupDeployment `
      -ResourceGroupName $rgName -Name infra).Parameters.siteName.Value `
      -ArchivePath $PWD\dnsmanagement.zip -Force
   ```

4. Get the NameServers to input in your domain management panel

   ```powershell
   (Get-AzDnsZone -Name $domainName -ResourceGroupName $rgName).NameServers | fl
   ```

   This will provide you with something as follows:

   ```powershell
   ns1-05.azure-dns.com.
   ns2-05.azure-dns.net.
   ns3-05.azure-dns.org.
   ns4-05.azure-dns.info.
   ```

   You can now input these as the authorative nameservers in your registrars panel

## Usage

1. Open up the Azure Portal and go to the newly created Function App

2. Get the function URL of the UpdateDnsFunction

3. Browsing to the URL will give you the following output

   > Successfully updated @.domainname.tld to 127.0.0.1

4. Congratulations your domainname will now point to your home IP

## License

Distributed under the MIT License. See `LICENSE` for more information.