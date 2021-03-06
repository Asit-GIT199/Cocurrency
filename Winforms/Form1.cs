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
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            var progressReport = new Progress<int>(ReportCardProcessingProgress);
            LoadingGif.Visible = true;
            pgCards.Visible = true;

            var stopwatch = new Stopwatch();
           

            var name = txtName.Text;
            try
            {
                //var greeting = await GetGreetings(name);
                var cards = await GetCards(3000, cts.Token);
                stopwatch.Start();
                await ProcessCards(cards, progressReport, cts.Token);

            }
            catch(TaskCanceledException ex)
            {
                MessageBox.Show("Task was cancelled ");
            }
            catch (HttpRequestException ex) // Exception will not throw if await is not mentioned in the try bloack
            {
                MessageBox.Show(ex.Message);                
            }
            finally
            {
                cts.Dispose();
            }

            MessageBox.Show($"Operation done in {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
            LoadingGif.Visible = false;
            pgCards.Visible = false;
            pgCards.Value = 0;
            cts = null;
        }

        private void ReportCardProcessingProgress(int percentage)
        {
            pgCards.Value = percentage;
        }

        private Task ProcessCardMock(List<string> cards, IProgress<int> progress = null, CancellationToken token = default)
        {
            //logic
            return Task.CompletedTask;//Synchronous task
        }

        private  Task<List<string>> GetCardsMock(int amountOfCardGenerate, CancellationToken token = default)
        {
            var cards = new List<string>();
            cards.Add("0001");
            return Task.FromResult(cards);
        }
        private Task CreateTaskWithException()
        {
            return Task.FromException(new ApplicationException());
        }

        private Task CreateTaskCancelled()
        {
            var cts2 = new CancellationTokenSource();
            return Task.FromCanceled(cts2.Token);
        }

        private async Task ProcessCards(List<string> cards, IProgress<int> progress=null, CancellationToken token= default)
        {
            using var semaphore = new SemaphoreSlim(250);
            var tasks = new List<Task<HttpResponseMessage>>();

            //var taskResolved = 0;

            tasks = cards.Select(async card =>
            {
                var json = JsonConvert.SerializeObject(card);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await semaphore.WaitAsync();
                try
                {

                    var internalTask =  await httpClient.PostAsync($"{apiURL}/cards", content, token);
                    //if (progress!=null)
                    //{
                    //    taskResolved++;
                    //    var percentage = (double)taskResolved / cards.Count;
                    //    percentage = percentage * 100;
                    //    var percentageInt = (int)Math.Round(percentage, 0);
                    //    progress.Report(percentageInt);
                    //}
                    return internalTask;
                }
                finally
                {
                    semaphore.Release();
                }
                
            }).ToList();   

            var responsesTasks = Task.WhenAll(tasks);

            if (progress!=null)
            {
                while (await Task.WhenAny(responsesTasks, Task.Delay(TimeSpan.FromSeconds(2))) != responsesTasks)
                {
                    var completedTask = tasks.Where(x => x.IsCompleted).Count();
                    var percentage = (double)completedTask / tasks.Count;
                    percentage = percentage * 100;
                    var percentageInt = (int)Math.Round(percentage, 0);
                    progress.Report(percentageInt);
                }
            }

            var responses = await responsesTasks;

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

        private async Task<List<string>> GetCards(int amountOfCardGenerate, CancellationToken token= default)
        {
            return await Task.Run(async () =>
            {
                var cards = new List<string>();
                for (int i = 0; i < amountOfCardGenerate; i++)
                {
                    //0000000000000001
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    cards.Add(i.ToString().PadLeft(16, '0'));
                    Console.WriteLine($"card number {1} is created");

                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
