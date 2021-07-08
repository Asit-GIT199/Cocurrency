using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class ParallelismForm1 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        public ParallelismForm1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44342/api";
            httpClient = new HttpClient();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            LoadingGif.Visible = true;
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var destinationSequential = Path.Combine(currentDirectory, @"images\result-sequential");
            var destinationSimultaneous = Path.Combine(currentDirectory, @"images\result-simultaneous");

            PrepareExecution(destinationSimultaneous, destinationSequential);

            Console.WriteLine("begin");

            var images = GetImages();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Sequential Part
            foreach (var image in images)
            {
                await ProcessImage(destinationSequential, image);
            }

            var timeSequential = stopwatch.ElapsedMilliseconds / 1000;

            Console.WriteLine("Sequential - duration: {0} seconds", timeSequential);


            // Simultaneous Part
            stopwatch.Restart();
            var tasks = images.Select(async image => await ProcessImage(destinationSimultaneous, image));
            await Task.WhenAll(tasks);
            var timeSimultaneous = stopwatch.ElapsedMilliseconds / 1000;
            Console.WriteLine("Simultaneous - duration: {0} seconds", timeSimultaneous);

            WriteComparison(timeSequential, timeSimultaneous);

            LoadingGif.Visible = false;
        }


        private static List<ImageDTO> GetImages()
        {
            var images = new List<ImageDTO>();
            for (int i = 0; i < 5; i++)
            {
                {
                    images.Add(
                    new ImageDTO()
                    {
                        Name = $"Spider-Man Spider-Verse {i}.jpg",
                        URL = "https://m.media-amazon.com/images/M/MV5BMjMwNDkxMTgzOF5BMl5BanBnXkFtZTgwNTkwNTQ3NjM@._V1_UY863_.jpg"
                    });
                    images.Add(

                    new ImageDTO()
                    {
                        Name = $"Spider-Man Far From Home {i}.jpg",
                        URL = "https://m.media-amazon.com/images/M/MV5BMGZlNTY1ZWUtYTMzNC00ZjUyLWE0MjQtMTMxN2E3ODYxMWVmXkEyXkFqcGdeQXVyMDM2NDM2MQ@@._V1_UY863_.jpg"
                    });
                    images.Add(

                    new ImageDTO()
                    {
                        Name = $"Moana {i}.jpg",
                        URL = "https://lumiere-a.akamaihd.net/v1/images/r_moana_header_poststreet_mobile_bd574a31.jpeg?region=0,0,640,480"
                    });
                    images.Add(

                    new ImageDTO()
                    {
                        Name = $"Avengers Infinity War {i}.jpg",
                        URL = "https://img.redbull.com/images/c_crop,x_143,y_0,h_1080,w_1620/c_fill,w_1500,h_1000/q_auto,f_auto/redbullcom/2018/04/23/e4a3d8a5-2c44-480a-b300-1b2b03e205a5/avengers-infinity-war-poster"
                    });
                    //images.Add(

                    //new ImageDTO()
                    //{
                    //    Name = $"Avengers Endgame {i}.jpg",
                    //    URL = "https://hipertextual.com/files/2019/04/hipertextual-nuevo-trailer-avengers-endgame-agradece-fans-universo-marvel-2019351167.jpg"
                    //});
                }
            }

            return images;
        }

        public static void WriteComparison(double time1, double time2)
        {
            var difference = time2 - time1;
            difference = Math.Round(difference, 2);
            var porcentualIncrement = ((time2 - time1) / time1) * 100;
            porcentualIncrement = Math.Round(porcentualIncrement, 2);
            Console.WriteLine($"Difference {difference} ({porcentualIncrement}%)");
        }

        private async Task ProcessImage(string directorio, ImageDTO imagen)
        {
            var response = await httpClient.GetAsync(imagen.URL);
            var content = await response.Content.ReadAsByteArrayAsync();

            Bitmap bitmap;
            using (var ms = new MemoryStream(content))
            {
                bitmap = new Bitmap(ms);
            }

            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var destination = Path.Combine(directorio, imagen.Name);
            bitmap.Save(destination);
        }

        public static void PrepareExecution(string destinationParallel, string destinationSequential)
        {
            if (!Directory.Exists(destinationParallel))
            {
                Directory.CreateDirectory(destinationParallel);
            }

            if (!Directory.Exists(destinationSequential))
            {
                Directory.CreateDirectory(destinationSequential);
            }

            DeleteFiles(destinationSequential);
            DeleteFiles(destinationParallel);
        }

        public static void DeleteFiles(string directory)
        {
            var files = Directory.EnumerateFiles(directory);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
