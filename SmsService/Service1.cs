using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.IO;


namespace SmsService
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _timer;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string Server = ConfigurationManager.AppSettings["Server"];
        private static readonly string UserId = ConfigurationManager.AppSettings["Username"];
        private static readonly string Password = ConfigurationManager.AppSettings["Password"];
        private static readonly string DBName = ConfigurationManager.AppSettings["DBName"];
        private static readonly string Table = ConfigurationManager.AppSettings["Table"];
        private static readonly string result = ConfigurationManager.AppSettings["Result"];
        private readonly string smsGatewayUrl;


        private string connectionString = $"Server={Server};Database={DBName};User Id={UserId};Password={Password};";

        public Service1()
        {
            InitializeComponent();
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Url.txt");
            smsGatewayUrl = File.ReadAllText(filepath);

        }

        protected override void OnStart(string[] args)
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["Interval"]);
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string query = $"select top 1 idx, PhoneNo,MessageText From {Table} where sendstatus=0";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        
                        int id = Convert.ToInt32(reader.GetInt64(0));
                        string phoneNumber = reader.GetString(1);
                        string message = reader.GetString(2);

                        string url = smsGatewayUrl
                            .Replace("MyPhoneNo", phoneNumber)
                            .Replace("MyMessage", Uri.EscapeDataString(message));


                        string response = await SendSms(url);

                        if (response.Contains(result))
                        {
                            UpdateSendStatus(id, 1);
                            LoggingUtility.LogMessageSent(id, phoneNumber, message);
                        }
                        else
                        {
                            UpdateSendStatus(id, 5);
                            LoggingUtility.LogError(new Exception($"Failed to send message to {phoneNumber}. Response: {response}"));
                        }
                    }
                }

                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }

            }

        }

        private async Task<string> SendSms(string url)
        {
            
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                return $"Error: {e.Message}";
            }
            
        }

        private void UpdateSendStatus(int id , int type)
        {
            if (type == 1)
            {
                string query = $"Update {Table} set sendstatus=1 where Idx = @Id";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            else if (type == 5)
            {
                string query = $"Update {Table} set sendstatus=5 where Idx = @Id";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            

        }

    }
}
