using System;
using System.Collections.Generic;
using Xunit;

namespace Cloud.Core.Messaging.AzureStorageQueue.Tests.Integration
{
    public class SignedAccessUrlTestData : TheoryData<Dictionary<string, string>, ISignedAccessConfig>
    {
        public SignedAccessUrlTestData()
        {
            var oneDayExpiry = DateTime.UtcNow.AddDays(1);
            var threeDaysExpiry = DateTime.UtcNow.AddDays(3);
            var oneWeekExpiry = DateTime.UtcNow.AddDays(7);
            var queueName = "testqueue";

            Add(new Dictionary<string, string>
            {
                {"ExpiryDate", oneDayExpiry.Date.ToString("yyyy-MM-dd")},
                {"Permission", "sp=a" },
                {"QueueName", queueName }
            },
            new SignedAccessConfig(new List<AccessPermission> { AccessPermission.Add }, oneDayExpiry));

            Add(new Dictionary<string, string>
                {
                    {"ExpiryDate", oneWeekExpiry.Date.ToString("yyyy-MM-dd")},
                    {"Permission", "sp=p" },
                    {"QueueName", queueName }
                },
            new SignedAccessConfig(new List<AccessPermission> { AccessPermission.Delete, AccessPermission.List }, oneWeekExpiry));

            Add(new Dictionary<string, string>
                {
                    {"ExpiryDate", oneWeekExpiry.Date.ToString("yyyy-MM-dd")},
                    {"Permission", "sp=a" },
                    {"QueueName", queueName }
                },
            new SignedAccessConfig(new List<AccessPermission> { AccessPermission.Add, AccessPermission.Add }, oneWeekExpiry));

            Add(new Dictionary<string, string>
                {
                    {"ExpiryDate", threeDaysExpiry.Date.ToString("yyyy-MM-dd")},
                    {"Permission", "sp=r" },
                    {"QueueName", queueName }
                },
            new SignedAccessConfig(new List<AccessPermission> { AccessPermission.Read }, threeDaysExpiry));

            Add(new Dictionary<string, string>
                {
                    {"ExpiryDate", oneWeekExpiry.Date.ToString("yyyy-MM-dd")},
                    {"Permission", "sp=rau" },
                    {"QueueName", queueName }
                },
            new SignedAccessConfig(new List<AccessPermission> { AccessPermission.Add, AccessPermission.Read, AccessPermission.Update }, oneWeekExpiry));
        }
    }
}
