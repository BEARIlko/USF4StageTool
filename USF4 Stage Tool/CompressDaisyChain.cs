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

namespace USF4_Stage_Tool
{
    public partial class CompressDaisyChain : Form
    {
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();
        public static CancellationToken token = tokenSource.Token;
        public static bool InProgress = false;
        public SubModel sm;
        public static int attempts;

        public CompressDaisyChain(SubModel submodel)
        {
            InitializeComponent();

            attempts = 0;
            sm = submodel;

            lb_SubmodelName.Text = Encoding.ASCII.GetString(sm.SubModelName);
            lb_InitCompressionValue.Text = $"{ 100 * sm.DaisyChainLength / (GeometryIO.FaceIndicesFromDaisyChain(sm.DaisyChain).Count * 3)}%";
            lb_BestCompressionValue.Text = lb_InitCompressionValue.Text;
        }

        private async void bt_StartPause_Click(object sender, EventArgs e)
        {
            if (!InProgress)
            {
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                InProgress = true;

                bt_StartPause.Text = "Pause";

                List<int> dc = new List<int>();

                try
                {
                    dc = await Task.Run(() =>
                    {
                        List<int[]> indices = GeometryIO.FaceIndicesFromDaisyChain(sm.DaisyChain);

                        while (1 == 1)
                        {
                            attempts++;
                            lb_AttemptsValue.Text = attempts.ToString();
                            if (token.IsCancellationRequested)
                            {
                                return dc;
                            }
                            dc = GeometryIO.BruteForceChain(indices, 1);

                            if (dc.Count < sm.DaisyChain.Length)
                            {
                                sm.DaisyChain = dc.ToArray();
                                sm.DaisyChainLength = sm.DaisyChain.Length;
                                lb_BestCompressionValue.Text = $"{ 100 * dc.Count / (indices.Count * 3)}%";
                            }
                        }

                    }, token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Cancelled exception");
                }

                InProgress = false;
                bt_StartPause.Text = "Start";


                if (dc.Count < sm.DaisyChain.Length)
                {
                    sm.DaisyChain = dc.ToArray();
                    sm.DaisyChainLength = sm.DaisyChain.Length;
                }

                if (this.DialogResult == DialogResult.OK)
                {
                    Close();
                }
            }
            else
            {
                InProgress = !InProgress;
                bt_StartPause.Text = "Start";

                Console.WriteLine("cancelled...");

                tokenSource.Cancel();
            }
        }

        private void bt_Cancel_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void bt_Confirm_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            DialogResult = DialogResult.OK;

            if (!InProgress)
            {
                Close();
            }
        }
    }
}
