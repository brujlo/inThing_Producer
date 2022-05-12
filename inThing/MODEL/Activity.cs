using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace inThing.MODEL
{
    public class Activity
    {
        public int id { get; set; }
        public string activity { get; set; }
        public string type { get; set; }
        public int participants { get; set; }
        public float price { get; set; }
        public string link { get; set; }
        public string key { get; set; }
        public double accessibility { get; set; }

        public async Task<Activity> GetActivityAsync(string path, HttpClient client)
        {
            Activity act = this;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                act = await response.Content.ReadAsAsync<Activity>();
            }
            return act;
        }

        public void ShowActivity(Activity activity)
        {
            Console.WriteLine(
                "activity: " + activity.activity + Environment.NewLine +
                "type: " + activity.type + Environment.NewLine +
                "participants: " + activity.participants + Environment.NewLine +
                "price: " + activity.price + Environment.NewLine +
                "link: " + activity.link + Environment.NewLine +
                "key: " + activity.key + Environment.NewLine +
                "accessibility: " + activity.accessibility + Environment.NewLine
                );
        }
    }


    //{ "activity":"Create and follow a savings plan","type":"busywork","participants":1,"price":0,"link":"","key":"9366464","accessibility":0.2}
}
