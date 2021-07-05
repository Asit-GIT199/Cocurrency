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
    public partial class Form3 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        public Form3()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async  void btnStart_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var names = new string[] { "a", "x", "f" };

            //var taskHttp = names.Select(x => GetGreetings(x, token));
            //var task = await Task.WhenAny(taskHttp);
            //var content = await task;
            //cts.Cancel();
            //Console.WriteLine(content);

            //var tasks = names.Select(name =>
            //{
            //    Func<CancellationToken, Task<string>> func = (ct) => GetGreetings(name, ct);
            //    return func;
            //});

            //var content = await OnlyOne(tasks);
            //Console.WriteLine(content);


            var content = await OnlyOne(
                (ct) => GetGoodbye("Asit", ct),
                (ct) => GetGreetings("Asit", ct)
                );
            Console.WriteLine(content);
        }

        private async Task<T> OnlyOne<T>(IEnumerable<Func<CancellationToken, Task<T>>> functions)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var tasks = functions.Select(function => function(cancellationTokenSource.Token));
            var task = await Task.WhenAny(tasks);
            cancellationTokenSource.Cancel();
            return await task;

        }

        private async Task<T> OnlyOne<T>(params Func<CancellationToken, Task<T>>[] functions)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var tasks = functions.Select(function => function(cancellationTokenSource.Token));
            var task = await Task.WhenAny(tasks);
            cancellationTokenSource.Cancel();
            return await task;

        }

        private async Task<string> GetGreetings(string name, CancellationToken token)
        {
            using (var response = await httpClient.GetAsync($"{apiURL}/greetings/async/{name}", token))
            {
                //response.EnsureSuccessStatusCode();
                var greeting = await response.Content.ReadAsStringAsync();
                return greeting;
            }
        }

        private async Task<string> GetGoodbye(string name, CancellationToken token)
        {
            using (var response = await httpClient.GetAsync($"{apiURL}/greetings/goodbye/{name}", token))
            {
                //response.EnsureSuccessStatusCode();
                var greeting = await response.Content.ReadAsStringAsync();
                return greeting;
            }
        }
    }
}
