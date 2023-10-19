// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Diagnostics;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Storage;

namespace Azure.ResourceManager.Samples.Common
{
    public static class Utilities
    {
        public static bool IsRunningMocked { get; set; }
        public static Action<string> LoggerMethod { get; set; }
        public static Func<string> PauseMethod { get; set; }

        public static string ProjectPath { get; set; }
        private static Random _random => new Random();

        public static string ReadLine() => PauseMethod.Invoke();
        public static string CreateRandomName(string namePrefix) => $"{namePrefix}{_random.Next(9999)}";
        public static string CreatePassword() => "azure12345QWE!";
        static Utilities()
        {
            LoggerMethod = Console.WriteLine;
            PauseMethod = Console.ReadLine;
            ProjectPath = ".";
        }

        public static void Log(string message)
        {
            LoggerMethod.Invoke(message);
        }

        public static void Log(object obj)
        {
            if (obj != null)
            {
                LoggerMethod.Invoke(obj.ToString());
            }
            else
            {
                LoggerMethod.Invoke("(null)");
            }
        }

        public static void Log()
        {
            Utilities.Log("");
        }

        public static string CreateUsername()
        {
            return "tirekicker";
        }

        public static void PrintVirtualNetwork(VirtualNetworkResource network)
        {
            var info = new StringBuilder();
            info.Append("Network: ").Append(network.Id)
                    .Append("Name: ").Append(network.Data.Name)
                    .Append("\n\tResource group: ").Append(network.Id.ResourceGroupName)
                    .Append("\n\tRegion: ").Append(network.Data.Location)
                    .Append("\n\tTags: ").Append(FormatDictionary(network.Data.Tags))
                    .Append("\n\tDNS server IPs: ").Append(FormatCollection(network.Data.DhcpOptionsDnsServers));

            // Output subnets
            foreach (var subnet in network.Data.Subnets)
            {
                info.Append("\n\tSubnet: ").Append(subnet.Name)
                        .Append("\n\t\tAddress prefix: ").Append(subnet.AddressPrefix);

                // Output associated route table
                var routeTable = subnet.RouteTable;
                if (routeTable != null)
                {
                    info.Append("\n\tRoute table ID: ").Append(routeTable.Id);
                }
            }

            // Output peerings
            foreach (var peering in network.Data.VirtualNetworkPeerings)
            {
                info.Append("\n\tPeering: ").Append(peering.Name)
                    .Append("\n\t\tRemote network ID: ").Append(peering.RemoteVirtualNetworkId)
                    .Append("\n\t\tPeering state: ").Append(peering.ProvisioningState)
                    .Append("\n\t\tIs traffic forwarded from remote network allowed? ").Append(peering.AllowForwardedTraffic)
                    .Append("\n\t\tGateway use: ").Append(peering.UseRemoteGateways);
            }

            Utilities.Log(info.ToString());
        }

        public static void PrintStorageAccount(StorageAccountResource storageAccount)
        {
            Utilities.Log($"{storageAccount.Data.Name} created @ {storageAccount.Data.CreatedOn}");
        }

        public static void PrintVirtualMachine(VirtualMachineResource virtualMachine)
        {
            var storageProfile = new StringBuilder().Append("\n\tStorageProfile: ");
            if (virtualMachine.Data.StorageProfile.ImageReference != null)
            {
                storageProfile.Append("\n\t\tImageReference:");
                storageProfile.Append("\n\t\t\tPublisher: ").Append(virtualMachine.Data.StorageProfile.ImageReference.Publisher);
                storageProfile.Append("\n\t\t\tOffer: ").Append(virtualMachine.Data.StorageProfile.ImageReference.Offer);
                storageProfile.Append("\n\t\t\tSKU: ").Append(virtualMachine.Data.StorageProfile.ImageReference.Sku);
                storageProfile.Append("\n\t\t\tVersion: ").Append(virtualMachine.Data.StorageProfile.ImageReference.Version);
            }

            if (virtualMachine.Data.StorageProfile.OSDisk != null)
            {
                storageProfile.Append("\n\t\tOSDisk:");
                storageProfile.Append("\n\t\t\tOSType: ").Append(virtualMachine.Data.StorageProfile.OSDisk.OSType);
                storageProfile.Append("\n\t\t\tName: ").Append(virtualMachine.Data.StorageProfile.OSDisk.Name);
                storageProfile.Append("\n\t\t\tCaching: ").Append(virtualMachine.Data.StorageProfile.OSDisk.Caching);
                storageProfile.Append("\n\t\t\tCreateOption: ").Append(virtualMachine.Data.StorageProfile.OSDisk.CreateOption);
                storageProfile.Append("\n\t\t\tDiskSizeGB: ").Append(virtualMachine.Data.StorageProfile.OSDisk.DiskSizeGB);
                if (virtualMachine.Data.StorageProfile.OSDisk.ImageUri != null)
                {
                    storageProfile.Append("\n\t\t\tImage Uri: ").Append(virtualMachine.Data.StorageProfile.OSDisk.ImageUri);
                }
                if (virtualMachine.Data.StorageProfile.OSDisk.VhdUri != null)
                {
                    storageProfile.Append("\n\t\t\tVhd Uri: ").Append(virtualMachine.Data.StorageProfile.OSDisk.VhdUri);
                }
                if (virtualMachine.Data.StorageProfile.OSDisk.EncryptionSettings != null)
                {
                    storageProfile.Append("\n\t\t\tEncryptionSettings: ");
                    storageProfile.Append("\n\t\t\t\tEnabled: ").Append(virtualMachine.Data.StorageProfile.OSDisk.EncryptionSettings.Enabled);
                    storageProfile.Append("\n\t\t\t\tDiskEncryptionKey Uri: ").Append(virtualMachine
                            .Data
                            .StorageProfile
                            .OSDisk
                            .EncryptionSettings
                            .DiskEncryptionKey.SecretUri);
                    storageProfile.Append("\n\t\t\t\tKeyEncryptionKey Uri: ").Append(virtualMachine
                            .Data
                            .StorageProfile
                            .OSDisk
                            .EncryptionSettings
                            .KeyEncryptionKey.KeyUri);
                }
            }
        }
            public static void PrintIPAddress(PublicIPAddressResource publicIPAddress)
        {
            Utilities.Log(new StringBuilder().Append("Public IP Address: ").Append(publicIPAddress.Id)
                .Append("Name: ").Append(publicIPAddress.Data.Name)
                .Append("\n\tResource group: ").Append(publicIPAddress.Id.ResourceGroupName)
                .Append("\n\tRegion: ").Append(publicIPAddress.Data.Location)
                .Append("\n\tTags: ").Append(FormatDictionary(publicIPAddress.Data.Tags))
                .Append("\n\tIP Address: ").Append(publicIPAddress.Data.IPAddress)
                .Append("\n\tFQDN: ").Append(publicIPAddress.Data.DnsSettings.Fqdn)
                .Append("\n\tReverse FQDN: ").Append(publicIPAddress.Data.DnsSettings.ReverseFqdn)
                .Append("\n\tIdle timeout (minutes): ").Append(publicIPAddress.Data.IdleTimeoutInMinutes)
                .Append("\n\tIP allocation method: ").Append(publicIPAddress.Data.PublicIPAllocationMethod)
                .ToString());
        }

        public static void PrintLoadBalancer(LoadBalancerResource loadBalancer)
        {
            var info = new StringBuilder();
            info.Append("Load balancer: ").Append(loadBalancer.Id)
                    .Append("Name: ").Append(loadBalancer.Data.Name)
                    .Append("\n\tResource group: ").Append(loadBalancer.Id.ResourceGroupName)
                    .Append("\n\tRegion: ").Append(loadBalancer.Data.Location)
                    .Append("\n\tTags: ").Append(FormatDictionary(loadBalancer.Data.Tags));

            // Show TCP probes
            info.Append("\n\tTCP probes: ")
                    .Append(loadBalancer.Data.Probes.Count);
            foreach (var probe in loadBalancer.Data.Probes)
            {
                info.Append("\n\t\tProbe name: ").Append(probe.Name)
                        .Append("\n\t\t\tPort: ").Append(probe.Port)
                        .Append("\n\t\t\tInterval in seconds: ").Append(probe.IntervalInSeconds)
                        .Append("\n\t\t\tRetries before unhealthy: ").Append(probe.NumberOfProbes);

                // Show associated load balancing rules
                info.Append("\n\t\t\tReferenced from load balancing rules: ")
                        .Append(probe.LoadBalancingRules.Count);
                foreach (var rule in probe.LoadBalancingRules)
                {
                    info.Append("\n\t\t\t\tID: ").Append(rule.Id);
                }
            }

            // Show load balancing rules
            info.Append("\n\tLoad balancing rules: ")
                    .Append(loadBalancer.Data.LoadBalancingRules.Count);
            foreach (var rule in loadBalancer.Data.LoadBalancingRules)
            {
                info.Append("\n\t\tLB rule name: ").Append(rule.Name)
                        .Append("\n\t\t\tProtocol: ").Append(rule.Protocol)
                        .Append("\n\t\t\tFloating IP enabled? ").Append(rule.EnableFloatingIP)
                        .Append("\n\t\t\tIdle timeout in minutes: ").Append(rule.IdleTimeoutInMinutes)
                        .Append("\n\t\t\tLoad distribution method: ").Append(rule.LoadDistribution);

                var frontend = rule.FrontendPort;
                info.Append("\n\t\t\tFrontend: ");
                if (frontend != null)
                {
                    info.Append(frontend.Value);
                }
                else
                {
                    info.Append("(None)");
                }

                info.Append("\n\t\t\tFrontend port: ").Append(rule.FrontendPort);

                var backend = rule.BackendPort;
                info.Append("\n\t\t\tBackend: ");
                if (backend != null)
                {
                    info.Append(backend.Value);
                }
                else
                {
                    info.Append("(None)");
                }

                info.Append("\n\t\t\tBackend port: ").Append(rule.BackendPort);

                var probe = rule.ProbeId;
                info.Append("\n\t\t\tProbe: ");
                if (probe == null)
                {
                    info.Append("(None)");
                }
                else
                {
                    info.Append(probe.Name).Append(" [").Append(probe.ToString()).Append("]");
                }
            }

            // Show inbound NAT rules
            info.Append("\n\tInbound NAT rules: ")
                    .Append(loadBalancer.Data.InboundNatRules.Count);
            foreach (var natRule in loadBalancer.Data.InboundNatRules)
            {
                info.Append("\n\t\tInbound NAT rule name: ").Append(natRule.Name)
                        .Append("\n\t\t\tProtocol: ").Append(natRule.Protocol.ToString())
                        .Append("\n\t\t\tFrontend port: ").Append(natRule.FrontendPort)
                        .Append("\n\t\t\tBackend port: ").Append(natRule.BackendPort)
                        .Append("\n\t\t\tFloating IP? ").Append(natRule.EnableFloatingIP)
                        .Append("\n\t\t\tIdle timeout in minutes: ").Append(natRule.IdleTimeoutInMinutes);
            }

            // Show inbound NAT pools
            info.Append("\n\tInbound NAT pools: ")
                    .Append(loadBalancer.Data.InboundNatPools.Count);
            foreach (var natPool in loadBalancer.Data.InboundNatPools)
            {
                info.Append("\n\t\tInbound NAT pool name: ").Append(natPool.Name)
                        .Append("\n\t\t\tProtocol: ").Append(natPool.Protocol.ToString())
                        .Append("\n\t\t\tFrontend port range: ")
                        .Append(natPool.FrontendPortRangeStart)
                        .Append("-")
                        .Append(natPool.FrontendPortRangeEnd)
                        .Append("\n\t\t\tBackend port: ").Append(natPool.BackendPort);
            }

            Utilities.Log(info.ToString());
        }

        private static string FormatDictionary(IDictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return string.Empty;
            }

            var outputString = new StringBuilder();

            foreach (var entity in dictionary)
            {
                outputString.AppendLine($"{entity.Key}: {entity.Value}");
            }

            return outputString.ToString();
        }

        private static string FormatCollection(IEnumerable<string> collection)
        {
            return string.Join(", ", collection);
        }
    }
}
