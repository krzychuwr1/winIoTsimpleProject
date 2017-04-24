using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        public ActionResult Chart(SingleLedModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            string connectionString = ""; //insert connection string from azure iot hub here
            string iotHubD2cEndpoint = "messages/events";

            var eventHubClient = EventHubClient.
            CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            var receiver = eventHubClient.GetDefaultConsumerGroup().
            CreateReceiver(d2cPartitions[0], DateTime.Now.AddHours(-5));
            var data =  ReceiveMessagesFromDeviceAsync(receiver);;
            receiver.Close();
            return View(data);
        }

        LedsModel ReceiveMessagesFromDeviceAsync(EventHubReceiver receiver)
        {
            var newData = new LedsModel();
            while (true)
            {
                try
                {
                    EventData eventData = receiver.Receive(TimeSpan.FromMilliseconds(1000));

                    if (eventData == null) break;

                    var messageString = Encoding.UTF8.GetString(eventData.GetBytes());

                    if (messageString.EndsWith("red led"))
                    {
                        newData.Red.Times.Add(eventData.EnqueuedTimeUtc);
                        newData.Red.States.Add(messageString.StartsWith("turn on") ? 1 : 0);
                    }
                    else
                    {
                        newData.Green.Times.Add(eventData.EnqueuedTimeUtc);
                        newData.Green.States.Add(messageString.StartsWith("turn on") ? 1 : 0);
                    }
                }
                catch(Exception)
                {
                }

            }
            return newData;
        }
    }
}