using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ServiceBus.DeadletterBehavior.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.DeadletterBehavior.Server
{
    public class ServerQueueAgent : IQueueAgent
    {
        private string _sbCs = string.Empty;
        private NamespaceManager _nsMgr;
        private MessagingFactory _msgFactory;

        private string _entityName = string.Empty;
        public string EntityName
        {
            get { return _entityName; }
            set { _entityName = value; }
        }

        public ServerQueueAgent(string entityName, string sbCs)
        {
            _entityName = entityName;
            _sbCs = sbCs;

            _msgFactory = MessagingFactory.CreateFromConnectionString(sbCs);
            _nsMgr = NamespaceManager.CreateFromConnectionString(sbCs);
        }

        public async Task CreateQueueAsync()
        {
            if (await QueueExists())
                return;

            QueueDescription queueDesc = new QueueDescription(_entityName)
            {
                EnableDeadLetteringOnMessageExpiration = true
            };

            await _nsMgr.CreateQueueAsync(queueDesc);
        }

        public async Task DeleteQueueAsync()
        {
            if (!await QueueExists())
                return;

            await _nsMgr.DeleteQueueAsync(_entityName);
        }

        private async Task<bool> QueueExists()
        {
            return await _nsMgr.QueueExistsAsync(_entityName);
        }

        public async Task SendMessageAsync(TimeSpan timeToLive)
        {
            MessageSender msgSender = await GetSender(_entityName);

            BrokeredMessage message = new BrokeredMessage("Test Message")
            {
                TimeToLive = timeToLive
            };

            await msgSender.SendAsync(message);
        }

        public async Task<string> PeekDeadLetterAsync()
        {
            MessageReceiver msgReceiver = null;

            try
            {
                msgReceiver = await GetReceiver(QueueClient.FormatDeadLetterPath(_entityName));

                BrokeredMessage msg = await msgReceiver.PeekAsync();

                return (msg != null) ? msg.GetBody<string>() : string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (msgReceiver != null)
                {
                    msgReceiver.Close();
                }
            }
        }

        public async Task<string> PeekQueueAsync()
        {
            MessageReceiver msgReceiver = null;

            try
            {
                msgReceiver = await GetReceiver(_entityName);

                BrokeredMessage msg = await msgReceiver.PeekAsync();

                return (msg != null) ? msg.GetBody<string>() : string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (msgReceiver != null)
                {
                    msgReceiver.Close();
                }
            }
        }

        public async Task<string> ReceiveFromQueueAsync()
        {
            MessageReceiver msgReceiver = null;
            string result = string.Empty;

            try
            {
                msgReceiver = await GetReceiver(_entityName);

                BrokeredMessage msg = await msgReceiver.ReceiveAsync(new TimeSpan(0, 0, 15));

                if (msg != null)
                {
                    result = msg.GetBody<string>();
                    await msg.AbandonAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (msgReceiver != null)
                {
                    msgReceiver.CloseAsync();
                }
            }

            return result;
        }

        private async Task<MessageReceiver> GetReceiver(string entity)
        {
            return await _msgFactory.CreateMessageReceiverAsync(entity, ReceiveMode.PeekLock);
        }

        private async Task<MessageSender> GetSender(string entity)
        {
            return await _msgFactory.CreateMessageSenderAsync(entity);
        }
    }
}
