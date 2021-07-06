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
            foreach (var name in GenerateNames())
            {
                Console.WriteLine(name);
            }

            
        }
        private IEnumerable<string> GenerateNames()
        {
            yield return "Asit";
            //read.Sleep(3000);
            yield return "Jack";
            yield return "John";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
