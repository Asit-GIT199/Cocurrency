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


        public Task EvaluateValue(string value)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (value=="1")
            {
                tcs.SetResult(null);
            }
            else if(value=="2")
            {
                tcs.SetCanceled();

            }
            else
            {
                tcs.SetException(new ApplicationException($"Invalid Value {value}"));
            }

            return tcs.Task;
        }
        private async void btnStart_Click(object sender, EventArgs e)
        { 
            cts = new CancellationTokenSource();

            try
            {
                var result = await Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    return 7;
                }).WithCancellation(cts.Token);

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cts.Dispose();
                cts = null;
            }

            
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
