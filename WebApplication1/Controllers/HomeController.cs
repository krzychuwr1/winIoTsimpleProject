using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string connectionString = ""; //insert connection string from azure iot hub here
            string iotHubD2cEndpoint = "messages/events";

            var eventHubClient = EventHubClient.
            CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            IEnumerable<string> data = new List<string>();

            
                var receiver = eventHubClient.GetDefaultConsumerGroup().
                CreateReceiver(d2cPartitions[0], DateTime.Now.AddDays(-1));
                var dataTask = ReceiveMessagesFromDeviceAsync(receiver);
                data = dataTask;
            
            return View(data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        IEnumerable<string> ReceiveMessagesFromDeviceAsync(EventHubReceiver receiver)
        {
            var data = new List<string>();
            while (true)
            {
                try
                {
                    EventData eventData = receiver.Receive(TimeSpan.FromMilliseconds(1000));

                    if (eventData == null) break;

                    string singleEventData = Encoding.UTF8.GetString(eventData.GetBytes());
                    data.Add(singleEventData);
                }
                catch(Exception)
                {
                }

            }
            return data;
        }
    }
}