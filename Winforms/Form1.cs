using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form1 : Form
    {
        private string apiURL;
        private HttpClient httpClient;

        public Form1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            var progressReport = new Progress<int>(ReportCardProcessingProgress);
            LoadingGif.Visible = true;
            pgCards.Visible = true;

            var stopwatch = new Stopwatch();
           

            var name = txtName.Text;
            try
            {
                //var greeting = await GetGreetings(name);
                var cards = await GetCards(2000);
                stopwatch.Start();
                await ProcessCards(cards, progressReport);

            }
            catch (HttpRequestException ex) // Exception will not throw if await is not mentioned in the try bloack
            {
                MessageBox.Show(ex.Message);                
            }

            MessageBox.Show($"Operation done in {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
            LoadingGif.Visible = false;
            pgCards.Visible = false;
            pgCards.Value = 0;
        }

        private void ReportCardProcessingProgress(int percentage)
        {
            pgCards.Value = percentage;
        }

        private async Task ProcessCards(List<string> cards, IProgress<int> progress=null)
        {
            using var semaphore = new SemaphoreSlim(250);
            var tasks = new List<Task<HttpResponseMessage>>();

            var taskResolved = 0;

            tasks = cards.Select(async card =>
            {
                var json = JsonConvert.SerializeObject(card);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await semaphore.WaitAsync();
                try
                {

                    var internalTask =  await httpClient.PostAsync($"{apiURL}/cards", content);
                    if (progress!=null)
                    {
                        taskResolved++;
                        var percentage = (double)taskResolved / cards.Count;
                        percentage = percentage * 100;
                        var percentageInt = (int)Math.Round(percentage, 0);
                        progress.Report(percentageInt);
                    }
                    return internalTask;
                }
                finally
                {
                    semaphore.Release();
                }
                
            }).ToList();   

            var responses = await Task.WhenAll(tasks);

            var rejectedCards = new List<string>();
            foreach (var response in responses)
            {
                var content = await response.Content.ReadAsStringAsync();
                var responseCard = JsonConvert.DeserializeObject<CardResponse>(content);
                if (!responseCard.Approved)
                {
                    rejectedCards.Add(responseCard.Card);
                }
            }
            foreach (var card in rejectedCards)
            {
                Console.WriteLine($"Card {card} was rejected");
            }
        }

        private async Task<List<string>> GetCards(int amountOfCardGenerate)
        {
            return await Task.Run(() =>
            {
                var cards = new List<string>();
                for (int i = 0; i < amountOfCardGenerate; i++)
                {
                    //0000000000000001
                    cards.Add(i.ToString().PadLeft(16, '0'));
                }
                return cards;
            });
            
        }

        private async Task Wait()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        private async Task<string> GetGreetings(string name)
        {
            using (var response = await httpClient.GetAsync($"{apiURL}/greetings2/{name}")) 
            {
                response.EnsureSuccessStatusCode();
                var greeting = await response.Content.ReadAsStringAsync();
                return greeting;
            }
        }
    }
}
