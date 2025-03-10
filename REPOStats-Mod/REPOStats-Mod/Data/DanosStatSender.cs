using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace REPOStats_Mod.Data
{
    public class DanosStatSender : MonoBehaviour
    {
        private static DanosStatSender _instance;

        public static DanosStatSender Instance
        {
            get
            {
                if (_instance == null)
                {
                    //Check if a gameobject with the same name exists
                    _instance = FindObjectOfType<DanosStatSender>();
                    if (_instance != null)
                    {
                        return _instance;
                    }


                    GameObject obj = new GameObject("DanosStatSender");
                    _instance = obj.AddComponent<DanosStatSender>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public void SendStats(DanosStatsStore stats)
        {
            StartCoroutine(PostStatsCoroutine(stats));
        }

        private IEnumerator PostStatsCoroutine(DanosStatsStore stats)
        {
            Debug.Log("Posting stats to API");

            string encodedUrl = "aHR0cHM6Ly9yZXBvLWFwaS5zcGxpdHN0YXRzLmlvL2FwaS9wb3N0Z2FtZS9zZW5kc3RhdHM=";
            string apiUrl = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUrl));

//#if DEBUG
//            apiUrl = "https://localhost:7018/api/postgame/sendstats";
//#endif

            string json = SerializeToJson(stats);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("XAuthCode", "cmVwb3N0YXRzbGt0b3A0MzI=");

                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Start the async request and store the task
                Task<HttpResponseMessage> requestTask = client.PostAsync(apiUrl, content);

                // Yield until the request is done (does not block main thread)
                yield return new WaitUntil(() => requestTask.IsCompleted);

            }

            yield break; 
        }


        private string SerializeToJson(DanosStatsStore stats)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DanosStatsStore));
                serializer.WriteObject(stream, stats);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
