using DSPLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FourierCSharp
{
    public partial class MainForm : Form
    {
        private const string appName = " - Fourier Transform";
        private static double[] timeDomain;
        private static double[] inputSignal; // input data
        private static double[] lmSpectrum; // output magnitude
        private static double[] freqSpan; // output frequency
        private const double samplingRate = 50; // 50 sample in 1s
        private static int IndexMaxMagnitude = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void mnuFileLoadData_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            this.Text = System.IO.Path.GetFileName(ofd.FileName) + appName;

            LoadData(ofd.FileName);
            DFT dft = new DFT();
            dft.Initialize((uint)inputSignal.Length);
            // Gọi DFT
            Complex[] cSpectrum = dft.Execute(inputSignal);

            // Chuyển complex sang magnitude
            lmSpectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);
            // lấy tần số tương ứng với magnitude
            freqSpan = dft.FrequencySpan(samplingRate);
            DrawGraph();

            double max = lmSpectrum.Max();
            IndexMaxMagnitude = lmSpectrum.ToList().IndexOf(max);

            if (IndexMaxMagnitude != -1)
            {
                //đếm số nhịp thở trong 1 phút
                var f = freqSpan[IndexMaxMagnitude];
                var totalBreathsInOneMininute = Math.Ceiling(f * 60); // s -> minute
                MessageBox.Show("Số nhịp thở trong 1 phút là: " + totalBreathsInOneMininute + "", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadData(string filename)
        {
            var lines = System.IO.File.ReadAllLines(filename, Encoding.UTF8);
            if (inputSignal != null) inputSignal = null;
            var inputData = new List<double>();
            var timeSeries = new List<double>();
            var data = 0.0;
            var time = 0.0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Split(',');
                if(line.Length > 1)
                {
                    var check = double.TryParse(line[0], out time);
                    var check2 = double.TryParse(line[1], out data);

                    if (check && check2)
                    {
                        data /= 1000; // convert ms -> s
                        time /= 1000; // ~
                        inputData.Add(data);
                        timeSeries.Add(time);
                    }
                }
            }
            inputSignal = inputData.ToArray();
            timeDomain = timeSeries.ToArray();
        }

        private void DrawGraph()
        {
            if (inputSignal == null) return;

            DrawRealData();
            SetDataGridViewDataInput();


            DrawMagnitudeData();
            SetDataGridViewDataOutput();
        }

        private void DrawRealData()
        {
            if (inputSignal == null) return;
            chtData.Series.Clear();
            chtData.Legends.Clear();
            chtData.ChartAreas.Clear();
            chtData.Titles.Clear();

            var ca = chtData.ChartAreas.Add("Data");
            chtData.Titles.Add("Input Data");

            ca.AxisX.Title = "Time Domain (s)";
            ca.AxisX.Minimum = 0;
            ca.AxisY.Title = "Data";
            var seriesIm = chtData.Series.Add("Imaginary");
            seriesIm.ChartType
                = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            seriesIm.BorderWidth = 2;
            seriesIm.Color = Color.DodgerBlue;

            for (int i = 0; i < inputSignal.Length; i++)
            {
                seriesIm.Points.AddXY(timeDomain[i], inputSignal[i]);
            }
        }

        private void DrawMagnitudeData()
        {
            if (inputSignal == null) return;
            chtFourier.Series.Clear();
            chtFourier.Legends.Clear();
            chtFourier.ChartAreas.Clear();
            chtFourier.Titles.Clear();
            var ca = chtFourier.ChartAreas.Add("Fourier");

            chtFourier.Titles.Add("Fourier Transform");
            ca.AxisX.Title = "Frequency (Hz)";
            ca.AxisX.Minimum = 0;
            ca.AxisY.Title = "Magnitude";
            ca.AxisY.Minimum = 0;

            var s = chtFourier.Series.Add("Data");
            s.SetCustomProperty("PointWidth", "0.9");
            s.ChartType
                = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            s.BorderWidth = 2;
            s.Color = Color.DodgerBlue;

            for (int i = 0; i < lmSpectrum.Length; i++)
            {
                s.Points.AddXY(freqSpan[i], lmSpectrum[i]);
            }
        }

        private void SetDataGridViewDataInput()
        {
            var dgv = dgvchtData;
            if (inputSignal == null) return;

            dgv.Rows.Clear();

            dgv.RowHeadersVisible = false;
            dgv.ColumnCount = 2;
            dgv.ReadOnly = true;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;

            dgv.Columns[0].HeaderText = "Time (s)";
            dgv.Columns[1].HeaderText = "Input Data";
            for (int i = 0; i < inputSignal.Length; i++)
            {
                dgv.Rows.Add(timeDomain[i], inputSignal[i]);
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void SetDataGridViewDataOutput()
        {
            var dgv = dgvFourier;
            if (lmSpectrum == null || freqSpan == null) return;
            dgv.Rows.Clear();

            dgv.RowHeadersVisible = false;
            dgv.ColumnCount = 2;
            dgv.ReadOnly = true;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;

            dgv.Columns[0].HeaderText = "Frequency (Hz)";
            dgv.Columns[1].HeaderText = "Magnitude";

            for (int i = 0; i < lmSpectrum.Length; i++)
            {
                dgv.Rows.Add(freqSpan[i], lmSpectrum[i]);
            }
            dgv.Rows[IndexMaxMagnitude].DefaultCellStyle.ForeColor = Color.Red;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}