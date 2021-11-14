using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace GameDevTask2
{
    class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();
            DateTime timestampStart = DateTime.Now;
            string user_id = "";
            for (int i = 0; i < 5; i++)
            {
                user_id += r.Next(100).ToString();
            }

            while (true)
            {
                var jsonRequest = ConstructJsonRequest(user_id, timestampStart);

                bool ok = SendPostHTTP(jsonRequest);
                if (!ok)
                {
                    using (StreamWriter writer = new StreamWriter("../../../backups/jsonBackups.txt", true))
                    {
                        writer.WriteLine(jsonRequest);
                    }
                }

                TryToSendBackups();

                Thread.Sleep(5000);
            }            
        }

        private static string ConstructJsonRequest(string user_id, DateTime timestampStart)
        {
            Dictionary<string, object> event_userid = new Dictionary<string, object>() {
                    {"user_id" , user_id },
                    {"startup_time" , (timestampStart.Ticks/TimeSpan.TicksPerMillisecond).ToString()},
                    {"running_time" , ((DateTime.Now - timestampStart).Ticks/TimeSpan.TicksPerMillisecond).ToString()},
                    {"event_type", "userTime_event"}
                };

            List<Dictionary<string, object>> eventList = new List<Dictionary<string, object>>();
            eventList.Add(event_userid);

            Dictionary<string, object> mainRequest = new Dictionary<string, object>() {
                    {"api_key" , "2250ab21b6ae37df01e30c74c9720288" },
                    {"events" , eventList}
                };
            var mainRequestJson = JsonConvert.SerializeObject(mainRequest);

            return mainRequestJson;
        }

        private static bool SendPostHTTP(string jsonContent)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://api.amplitude.com/2/httpapi");
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            httpRequest.Accept = "*/*";
            try
            {
                using (var requestStream = httpRequest.GetRequestStream())
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(jsonContent);
                }
                using (var httpResponse = httpRequest.GetResponse())
                using (var responseStream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string response = reader.ReadToEnd();
                    Console.WriteLine(response);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void TryToSendBackups()
        {
            bool success = false;
            using (StreamReader sr = new StreamReader("../../../backups/jsonBackups.txt"))
            {
                string jsonLine;
                while ((jsonLine = sr.ReadLine()) != null)
                {
                    success = SendPostHTTP(jsonLine);
                    if (!success) break;
                }
            }

            if (success)
            {
                using (StreamWriter writer = new StreamWriter("../../../backups/jsonBackups.txt", false))
                {
                    writer.Write(string.Empty);
                }
            }
        }
    }
}
