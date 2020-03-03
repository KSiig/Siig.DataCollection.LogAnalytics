using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Siig.DataCollection.LogAnalytics
{
    public class LogAnalytics : ILogAnalytics
    {
        public string WorkspaceId { get; set; }
        public string SharedKey { get; set; }
        public string LogType { get; set; }
        public string TimeStampField { get; set; }
        public string ApiVersion { get; set; }
        public ILogger AILog { get; set; }

        public LogAnalytics()
        {
            TimeStampField = "";
            ApiVersion = "2016-04-01";
        }

        public void SendLogEntry<T>(T entity)
        {
            SendLogEntries(new List<T>() { entity });
        }

        public void SendLogEntries<T>(List<T> entities)
        {
            try
            {
                var entityAsJson = JsonConvert.SerializeObject(entities);
                var datestring = DateTime.UtcNow.ToString("r");
                var signature = CreateSignature(entityAsJson, SharedKey, WorkspaceId, datestring);

                string url = $"https://{WorkspaceId}.ods.opsinsights.azure.com/api/logs?api-version={ApiVersion}";

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Log-Type", LogType);
                client.DefaultRequestHeaders.Add("Authorization", signature);
                client.DefaultRequestHeaders.Add("x-ms-date", datestring);
                client.DefaultRequestHeaders.Add("time-generated-field", TimeStampField);

                HttpContent httpContent = new StringContent(entityAsJson, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpContent response = client.PostAsync(new Uri(url), httpContent).Result.Content;
                string result = response.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(result)) throw new Exception(result);
            }
            catch(Exception ex)
            {
                if (AILog != null) AILog.LogError(ex.Message);
                throw ex;
            }
        }
        public string CreateSignature(string entitiyAsJson, string key, string workspaceId, string datestring)
        {
            var jsonBytes = Encoding.UTF8.GetBytes(entitiyAsJson);
            string stringToHash = $"POST\n{jsonBytes.Length}\napplication/json\nx-ms-date:{datestring}\n/api/logs";
            string hashedString = BuildSignature(stringToHash, key);
            return $"SharedKey {workspaceId}:{hashedString}";
        }

        /// <summary>
        /// Builds signature to use when sending data
        /// </summary>
        /// <param name="message">Message to build signature from</param>
        /// <param name="secret">Secret to build signature from</param>
        /// <returns>Hashed signature</returns>
        private string BuildSignature(string message, string secret)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
}
