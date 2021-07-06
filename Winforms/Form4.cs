using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form4 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        public Form4()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }        
        private async void btnStart_Click(object sender, EventArgs e)
        {
            LoadingGif.Visible = true;

            var resultStartNew = await Task.Factory.StartNew(async () =>
            {
                 await Task.Delay(TimeSpan.FromSeconds(1));
                return 7;
            }).Unwrap();

            var resultRun = await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return 7;
            });
            Console.WriteLine($"Startnew Result : {resultStartNew}" );
            Console.WriteLine("-----------------");
            Console.WriteLine($"Task. Run Result : {resultRun}");

            LoadingGif.Visible = false;
        }

        private async Task<string> GetValue()
        {
            //await Task.Delay(TimeSpan.FromSeconds(3));
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            return "Concurrency";
        }

        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private async Task ProcessNames(IAsyncEnumerable<string> namesEnumarable)
        {
            cts = new CancellationTokenSource();
            try
            {
                await foreach (var name in namesEnumarable.WithCancellation(cts.Token))
                {
                    Console.WriteLine(name);
                }
            }
            catch (TaskCanceledException ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                cts.Dispose();
                cts = null;
            }

        }
        private async IAsyncEnumerable<string> GenerateNames([EnumeratorCancellation] CancellationToken token = default)
        {
            yield return "Asit";
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            yield return "Jack";
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            yield return "John";
        }

    }
}
