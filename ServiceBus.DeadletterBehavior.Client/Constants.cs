
using System;
namespace ServiceBus.DeadletterBehavior.Client
{
    public static class Constants
    {
        public const string EntityName = "ExpiringQueue";
        public const int TimeToLiveInSeconds = 5;

        public static class AppSettings
        {
            public const string AzureCsName = "AzureCS";
            public const string ServerCsName = "ServerCS";
        }
    }
}
