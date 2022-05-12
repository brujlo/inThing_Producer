using inThing.DAL;
using inThing.MODEL;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace inThing
{
    public class Program
    {
        static HttpClient client = new HttpClient();
        public const string URL = "https://www.boredapi.com/api/activity";
        private static Timer timer1;

        public static string ConStr;

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
               .AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            ConStr = config.GetConnectionString("DBpostgre");


            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            //stvaramo loop 10sec
            timer1 = new Timer();
            timer1.Elapsed += (sender, e) =>
            {
                DoTask();
            };

            timer1.Interval = 10000;
            timer1.Start();

            Console.WriteLine(Environment.NewLine + "STARTING SERVICE -> press any key to stop" + Environment.NewLine + "----------------------------------" + Environment.NewLine + Environment.NewLine);
            Console.ReadKey();

        }


        public async static void DoTask()
        {
            Console.WriteLine("... getting activity from API!" + Environment.NewLine);

            Activity act = new Activity();
            act = await act.GetActivityAsync(URL, client);
            
            if(act.activity != "")
            {
                act.ShowActivity(act);

                Console.WriteLine("... trying to save activity to DB!");

                NpgSql db = new NpgSql(ConStr);
                Console.WriteLine("Saved: " + db.AddActivityToDB(act).ToString() + Environment.NewLine) ;

                Console.WriteLine("... trying to add activity to message ques!");

                RabbitSendMessage(act);
            }
            else
            {
                Console.WriteLine("Did not get activity! Nothing done!" + Environment.NewLine);
            }
        }

        public static void RabbitSendMessage(Activity act)
        {
            try
            {
                string rabbitmqconnection = $"amqp://{HttpUtility.UrlEncode("guest")}:{ HttpUtility.UrlEncode("guest")}@{ "localhost:5672"}";
                var factory = new ConnectionFactory();
                factory.Uri = new Uri(rabbitmqconnection);
                factory.AutomaticRecoveryEnabled = true;
                factory.DispatchConsumersAsync = true;

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "inThing1",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var message = new 
                    {
                        activity = act.activity,
                        type = act.type,
                        participants = act.participants,
                        price = act.price,
                        link = act.link,
                        key = act.key,
                        accessibility = act.accessibility 
                    };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    channel.BasicPublish(exchange: "",
                                         routingKey: "inThing1",
                                         basicProperties: null,
                                         body: body);

                    //Console.WriteLine(" [x] Sent {0}", message);
                    Console.WriteLine("Aadded: true");

                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Aadded: false");
                //Console.WriteLine("Message not sent");
            }
        }

        public static void RabbitSendMessageTest(Activity act)
        {
            IConnection _rabbit;
            try
            {
                /*Uri uri = new Uri("ampq://guest:guest@localhost:5672");
                var factory = new ConnectionFactory();
                factory.Uri = uri;*/
                //string rabbitmqconnection = $"amqp://{HttpUtility.UrlEncode("guest")}:{ HttpUtility.UrlEncode("guest")}@{ "localhost:5672"}/{ HttpUtility.UrlEncode("/act")}";
                string rabbitmqconnection = $"amqp://{HttpUtility.UrlEncode("guest")}:{ HttpUtility.UrlEncode("guest")}@{ "localhost:5672"}";

                var factory = new ConnectionFactory();
                //factory.Uri = new Uri("ampq://guest:guest@localhost:5672/act");
                factory.Uri = new Uri(rabbitmqconnection);
                factory.AutomaticRecoveryEnabled = true;
                factory.DispatchConsumersAsync = true;

                _rabbit = factory.CreateConnection("ProviderMessage");

                //var factory = new ConnectionFactory()
                //{
                //    Uri = new Uri("http://guest:guest@localhost:15672")
                //};

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "inThing1",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var message = new
                    {
                        activity = act.activity,
                        type = act.type,
                        participants = act.participants,
                        price = act.price,
                        link = act.link,
                        key = act.key,
                        accessibility = act.accessibility
                    };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    //var message = "test";
                    //var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "inThing1",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);

                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                Console.WriteLine("Message not sent");
            }
        }
    }
}
