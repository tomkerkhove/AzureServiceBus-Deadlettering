using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ServiceBus.DeadletterBehavior.Core;
using System;

namespace ServiceBus.DeadletterBehavior.Azure
{
    public class Program
    {
        private static string _sbAuth = ConfigurationManager.AppSettings.Get("ServiceBus-Auth");
        static void Main(string[] args)
        {
            CreateQueue(Constants.EntityName);
        }

        public static void CreateQueue(string entityName)
        {
            NamespaceManager nsManager = NamespaceManager.CreateFromConnectionString(_sbAuth);

            QueueDescription queueDesc = new QueueDescription(entityName)
            {
                EnableDeadLetteringOnMessageExpiration = true,
                DefaultMessageTimeToLive = new TimeSpan(0, 0, 0, 15)
            };

            nsManager.CreateQueue(queueDesc);
        }

        public static void SendMessage(string entityName)
        {
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(_sbAuth);
            factory.CreateMessageSender(entityName);
        }

        public static void PeekDeadLetter(string entityName)
        {
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(_sbAuth);
            factory.CreateMessageReceiver(QueueClient.FormatDeadLetterPath(entityName), ReceiveMode.PeekLock);
        }

        public static void PeekQueue(string entityName)
        {
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(_sbAuth);
            factory.CreateMessageReceiver(entityName, ReceiveMode.PeekLock);
        }
    }
}
