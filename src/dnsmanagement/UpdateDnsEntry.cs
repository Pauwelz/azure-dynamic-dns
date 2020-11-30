using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;

namespace AzureDynamicDns.DnsManagement
{
    public static class UpdateDnsEntry
    {
        [FunctionName("UpdateDnsEntry")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string zone = req.Query["zone"];
            string name = req.Query["name"];
            string ip = req.Query["ip"];

            // If we don't get the variables from the querystring, use defaults
            name = name ?? Environment.GetEnvironmentVariable("defaultSubdomain");
            zone = zone ?? Environment.GetEnvironmentVariable("defaultZone");
            ip = ip ?? req.HttpContext.Connection.RemoteIpAddress.ToString();

            // Remove the DNS Zone from the subdomain to avoid getting subdomain.domain.tld.domain.tld records.
            name.Replace(zone, "");

            var dnsManagmentClient = new DnsManagementClient(Environment.GetEnvironmentVariable("subscriptionId"), new DefaultAzureCredential());

            // Create a new RecordSet with a default TTL of 600
            var recordSet = new RecordSet()
            {
                TTL = 600
            };
            // Azure App Services are IPv4 only, unless going through Azure Front Door
            recordSet.ARecords.Add(new ARecord() { Ipv4Address = ip });
            await dnsManagmentClient.RecordSets.CreateOrUpdateAsync(Environment.GetEnvironmentVariable("resourceGroup"), zone, name, RecordType.A, recordSet);

            return new OkObjectResult($"Successfully updated {name}.{zone} to {ip}");
        }
    }
}
