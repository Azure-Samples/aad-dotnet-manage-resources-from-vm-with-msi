---
services: Compute
platforms: dotnet
author: anuchandy
---

# Getting started on managing Azure resources from a virtual machine with managed service identity (MSI) using C# #

          Azure Compute sample for managing virtual machines -
            - Create a virtual machine with Managed Service Identity enabled with access to resource group
            - Set custom script in the virtual machine that
                 - install az cli in the virtual machine
                 - uses az cli MSI credentials to create a storage account
            - Get storage account created through Managed Service Identity (MSI) credentials.


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-java/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/aad-dotnet-manage-resources-from-vm-with-msi.git

    cd aad-dotnet-manage-resources-from-vm-with-msi

    dotnet restore

    dotnet run

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.