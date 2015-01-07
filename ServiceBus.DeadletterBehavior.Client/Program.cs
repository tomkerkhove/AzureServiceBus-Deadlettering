using ServiceBus.DeadletterBehavior.Azure;
using ServiceBus.DeadletterBehavior.Core;
using ServiceBus.DeadletterBehavior.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBus.DeadletterBehavior.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Service Bus - Expiring simulation";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("What platform do you want to simulate the behavior?");
            Console.WriteLine("- 'Azure' for Microsoft Azure Service Bus");
            Console.WriteLine("- 'Server' for Service bus for Windows Server");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            string platform = Console.ReadLine();

            IQueueAgent queueAgent = GetAgent(platform);

            SimulateBehavior(queueAgent).Wait();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Enter a key to exit the application...");
            Console.ReadLine();
        }

        private static IQueueAgent GetAgent(string platform)
        {
            if (platform == "azure")
            {
                return new AzureQueueAgent(string.Format("{0}-{1}", Constants.EntityName, DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")), ConfigurationManager.AppSettings.Get(Constants.AppSettings.AzureCsName));
            }
            else
            {
                return new ServerQueueAgent(string.Format("{0}-{1}", Constants.EntityName, DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")), ConfigurationManager.AppSettings.Get(Constants.AppSettings.ServerCsName));
            }
        }

        public static async Task SimulateBehavior(IQueueAgent queueAgent)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Starting simulation on queue '{0}'...", queueAgent.EntityName);

            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                // Create queue
                await queueAgent.CreateQueueAsync();
                Console.WriteLine("Queue created.");

                // Send message
                await queueAgent.SendMessageAsync(new TimeSpan(0, 0, Constants.TimeToLiveInSeconds));
                Console.WriteLine("Message sent.");

                // Wait
                Thread.Sleep(new TimeSpan(0, 0, Constants.TimeToLiveInSeconds * 2));

                // Check DLQ
                Console.WriteLine("Peek message from DLQ with content - '{0}'", await queueAgent.PeekDeadLetterAsync());

                // Peek & check
                Console.WriteLine("Peek message with content - '{0}'", await queueAgent.PeekQueueAsync());

                // Check DLQ
                Console.WriteLine("Peek message from DLQ with content - '{0}'", await queueAgent.PeekDeadLetterAsync());

                // Receive & check
                Console.WriteLine("Received message with content - '{0}'", await queueAgent.ReceiveFromQueueAsync());

                // Check DLQ
                Console.WriteLine("Peek message from DLQ with content - '{0}'", await queueAgent.PeekDeadLetterAsync());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something went wrong - {0}", ex.Message);
            }
            finally
            {
                // Delete queue
                queueAgent.DeleteQueueAsync();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Queue deleted.");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Simulation done...");
            Console.WriteLine();
        }
    }
}
