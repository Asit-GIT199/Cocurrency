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
    public partial class ParallelismForm2 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        public ParallelismForm2()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            LoadingGif.Visible = false;
            cts = new CancellationTokenSource();

            //Parallel.For(1, 11, number => Console.WriteLine(number));

            var colMatrixA = 1100;
            var rows = 1000;
            var colMatrixB = 1750;
            var matrixA = Matrices.InitializeMatrix(rows, colMatrixA);
            var matrixB = Matrices.InitializeMatrix(colMatrixA, colMatrixB);
            var result = new double[rows, colMatrixB];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Sequential Part
            await Task.Run(()=> Matrices.MultiplyMatricesSequential(matrixA, matrixB,result));

            var timeSequential = stopwatch.ElapsedMilliseconds / 1000;

            Console.WriteLine("Sequential - duration: {0} seconds", timeSequential);

            stopwatch.Restart();

            try
            {
                await Task.Run(() => Matrices.MultiplyMatricesParallel(matrixA, matrixB, result));

                var timeSimultaneous = stopwatch.ElapsedMilliseconds / 1000;
                Console.WriteLine("Simultaneous - duration: {0} seconds", timeSimultaneous);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The task was cancelled");
                
            }
            finally
            {
                cts.Dispose();
                cts = null;
            }
            


            LoadingGif.Visible = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
