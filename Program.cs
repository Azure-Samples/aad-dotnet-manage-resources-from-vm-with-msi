// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.ManagedServiceIdentities;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Compute.Models;
using Microsoft.Azure.Management.Samples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Network;
using Azure;

namespace ManageStorageFromMSIEnabledVirtualMachine
{
    public class Program
    {
        /**
         * Azure Compute sample for managing virtual machines -
         *   - Create a virtual machine with Managed Service Identity enabled with access to resource group
         *   - Set custom script in the virtual machine that
         *        - install az cli in the virtual machine
         *        - uses az cli MSI credentials to create a storage account
         *   - Get storage account created through Managed Service Identity (MSI) credentials.
         */
        public static async Task RunSample(ArmClient client)
        {
            var identityName = Utilities.CreateRandomName("idt");
            var linuxVMName = Utilities.CreateRandomName("VM1");
            var linuxComputerName = Utilities.CreateRandomName("linuxComputer");
            var networkConfigurationName = Utilities.CreateRandomName("networkconfiguration");
            var rgName = Utilities.CreateRandomName("rgCOMV");
            var pipName = Utilities.CreateRandomName("pip1");
            var extensionName = Utilities.CreateRandomName("extension");
            var pipDnsLabelLinuxVM = Utilities.CreateRandomName("rgpip1");
            var userName = Utilities.CreateUsername();
            var password = Utilities.CreatePassword();
            var lro = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup = lro.Value;
            var region = AzureLocation.EastUS;

            var installScript = "https://raw.githubusercontent.com/Azure/azure-libraries-for-net/master/Samples/Asset/create_resources_with_msi.sh";
            var installCommand = "bash create_resources_with_msi.sh {identityId} {stgName} {rgName} {location}";
            List<String> fileUris = new List<String>();
            fileUris.Add(installScript);
            try
            {
                //=============================================================
                // Create an identity with access to current resource group
                var identityCollection = resourceGroup.GetUserAssignedIdentities();
                var identityData = new UserAssignedIdentityData(region)
                {
                };
                var identity = (await identityCollection.CreateOrUpdateAsync(WaitUntil.Completed, identityName, identityData)).Value;

                //=============================================================
                // Create a Linux VM with MSI enabled for contributor access to the current resource group

                Utilities.Log("Creating a Linux VM with MSI enabled");
                // Create a linux virtual network
                Utilities.Log("Creating a Linux virtual network...");
                var virtualNetworkName = Utilities.CreateRandomName("VirtualNetwork_");
                var virtualNetworkCollection = resourceGroup.GetVirtualNetworks();
                var data = new VirtualNetworkData()
                {
                    Location = AzureLocation.EastUS,
                    AddressPrefixes =
                    {
                        new string("10.0.0.0/28"),
                    },
                };
                var virtualNetworkLro = await virtualNetworkCollection.CreateOrUpdateAsync(WaitUntil.Completed, virtualNetworkName, data);
                var virtualNetwork = virtualNetworkLro.Value;
                Utilities.Log("Created a Linux virtual network with name : " + virtualNetwork.Data.Name);

                // Create a public IP address
                Utilities.Log("Creating a Linux Public IP address...");
                var publicAddressIPCollection = resourceGroup.GetPublicIPAddresses();
                var publicIPAddressdata = new PublicIPAddressData()
                {
                    Location = AzureLocation.EastUS,
                    Sku = new PublicIPAddressSku()
                    {
                        Name = PublicIPAddressSkuName.Standard,
                    },
                    PublicIPAddressVersion = NetworkIPVersion.IPv4,
                    PublicIPAllocationMethod = NetworkIPAllocationMethod.Static,
                    DnsSettings = new PublicIPAddressDnsSettings()
                    {
                        DomainNameLabel = pipDnsLabelLinuxVM
                    },
                };
                var publicIPAddressLro = await publicAddressIPCollection.CreateOrUpdateAsync(WaitUntil.Completed, pipName, publicIPAddressdata);
                var publicIPAddress = publicIPAddressLro.Value;
                Utilities.Log("Created a Linux Public IP address with name : " + publicIPAddress.Data.Name);

                //Create a subnet
                Utilities.Log("Creating a Linux subnet...");
                var subnetName = Utilities.CreateRandomName("subnet_");
                var subnetData = new SubnetData()
                {
                    ServiceEndpoints =
                    {
                        new ServiceEndpointProperties()
                        {
                            Service = "Microsoft.Storage"
                        }
                    },
                    Name = subnetName,
                    AddressPrefix = "10.0.0.0/28",
                };
                var subnetLRro = await virtualNetwork.GetSubnets().CreateOrUpdateAsync(WaitUntil.Completed, subnetName, subnetData);
                var subnet = subnetLRro.Value;
                Utilities.Log("Created a Linux subnet2 with name : " + subnet.Data.Name);

                //Create a networkInterface
                Utilities.Log("Created a linux networkInterface");
                var networkInterfaceData = new NetworkInterfaceData()
                {
                    Location = AzureLocation.EastUS,
                    IPConfigurations =
                    {
                        new NetworkInterfaceIPConfigurationData()
                        {
                            Name = "internal",
                            Primary = true,
                            Subnet = new SubnetData
                            {
                                Name = subnetName,
                                Id = new ResourceIdentifier($"{virtualNetwork.Data.Id}/subnets/{subnetName}")
                            },
                            PrivateIPAllocationMethod = NetworkIPAllocationMethod.Dynamic,
                            PublicIPAddress = publicIPAddress.Data,
                        }
                    }
                };
                var networkInterfaceName = Utilities.CreateRandomName("networkInterface");
                var nic = (await resourceGroup.GetNetworkInterfaces().CreateOrUpdateAsync(WaitUntil.Completed, networkInterfaceName, networkInterfaceData)).Value;
                Utilities.Log("Created a Linux network interface with name : " + nic.Data.Name);

                //Create a VM with the Public IP address
                Utilities.Log("Creating a LinuxVM with the Public IP address...");
                var virtualMachineCollection = resourceGroup.GetVirtualMachines();
                var linuxVmdata = new VirtualMachineData(AzureLocation.EastUS)
                {
                    HardwareProfile = new VirtualMachineHardwareProfile()
                    {
                        VmSize = "Standard_D2a_v4"
                    },
                    OSProfile = new VirtualMachineOSProfile()
                    {
                        AdminUsername = userName,
                        AdminPassword = password,
                        ComputerName = linuxComputerName,
                    },
                    NetworkProfile = new VirtualMachineNetworkProfile()
                    {
                        NetworkInterfaces =
                        {
                            new VirtualMachineNetworkInterfaceReference()
                            {
                                Id = nic.Id,
                                Primary = true,
                            }
                        }
                    },
                    StorageProfile = new VirtualMachineStorageProfile()
                    {
                        OSDisk = new VirtualMachineOSDisk(DiskCreateOptionType.FromImage)
                        {
                            OSType = SupportedOperatingSystemType.Linux,
                            Caching = CachingType.ReadWrite,
                            ManagedDisk = new VirtualMachineManagedDisk()
                            {
                                StorageAccountType = StorageAccountType.StandardLrs
                            }
                        },
                        ImageReference = new ImageReference()
                        {
                            Publisher = "Canonical",
                            Offer = "UbuntuServer",
                            Sku = "16.04-LTS",
                            Version = "latest",
                        }
                    },
                    Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
                    {
                        UserAssignedIdentities =
                        {
                        }
                    }
                };
                var virtualMachine_lro = await virtualMachineCollection.CreateOrUpdateAsync(WaitUntil.Completed, linuxVMName, linuxVmdata);
                var virtualMachine = virtualMachine_lro.Value;

                Utilities.Log("Created virtual machine with MSI enabled");
                Utilities.PrintVirtualMachine(virtualMachine);

                // Prepare custom script to install az cli that uses MSI to create a storage account
                //
                var stgName = Utilities.CreateRandomName("st44");
                installCommand = installCommand
                                .Replace("{identityId}", identity.Id)
                                .Replace("{stgName}", stgName)
                                .Replace("{rgName}", rgName)
                                .Replace("{location}", region.Name);

                // Update the VM by installing custom script extension.
                //
                Utilities.Log("Installing custom script extension to configure az cli in the virtual machine");
                Utilities.Log("az cli will use MSI credentials to create storage account");

                var linuxSettings2 = new
                {
                    fileUris = fileUris,
                    commandToExecute = installCommand
                };
                var linuxBinaryData = BinaryData.FromObjectAsJson(linuxSettings2);
                var linuxVmExtensionData = new VirtualMachineExtensionData(AzureLocation.EastUS)
                {
                    Publisher = "Microsoft.OSTCExtensions",
                    ExtensionType = "CustomScriptForLinux",
                    TypeHandlerVersion = "1.4",
                    AutoUpgradeMinorVersion = true,
                    Settings = linuxBinaryData
                };
                var extensionCollection = virtualMachine.GetVirtualMachineExtensions();
                var extension =(await extensionCollection.CreateOrUpdateAsync(WaitUntil.Completed, extensionName, linuxVmExtensionData)).Value;

                // Retrieve the storage account created by az cli using MSI credentials
                //
                var storageAccount = azure.StorageAccounts
                        .GetByResourceGroup(rgName, stgName);

                Utilities.Log("Storage account created by az cli using MSI credential");
                Utilities.PrintStorageAccount(storageAccount);
            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + rgName);
                    azure.ResourceGroups.BeginDeleteByName(rgName);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}
