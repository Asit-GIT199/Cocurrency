using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form2 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        public Form2()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            LoadingGif.Visible = true;
            
            try
            {
                //await Retry(ProcessGreeting);
                var content = await Retry(ProcessGreetingReturns);
                Console.WriteLine($"From ProcessGreetingReturns : {content}");
                
            }
            catch (Exception ex)
            {

                Console.WriteLine("The operation failed");
            }

            LoadingGif.Visible = false;

        }

        private async Task ProcessGreeting()
        {
            using (var response = await httpClient.GetAsync($"{apiURL}/greetings/asit"))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }

        private async Task<string> ProcessGreetingReturns()
        {
            using (var response = await httpClient.GetAsync($"{apiURL}/greetings/asit"))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }
        private async Task Retry(Func<Task> f, int retryTimes = 3, int waitTime = 500)
        {
            for (int i = 0; i < retryTimes -1 ; i++)
            {
                try
                {
                    await f();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(waitTime);
                }
            }
            await f();
        }

        private async Task<T> Retry<T>(Func<Task<T>> f, int retryTimes = 3, int waitTime = 500)
        {
            for (int i = 0; i < retryTimes - 1; i++)
            {
                try
                {
                    return await f();                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(waitTime);
                }
            }
            return await f();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
