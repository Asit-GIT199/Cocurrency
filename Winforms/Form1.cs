using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            LoadingGif.Visible = true;
            //Thread.Sleep(TimeSpan.FromSeconds(5));
            //await Task.Delay(TimeSpan.FromSeconds(5));  // This will till the task finished, then the next line will be executed

            await Wait();
            MessageBox.Show("5 sec have passed");
            //LoadingGif.Visible = false;
        }

        private async Task Wait()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
