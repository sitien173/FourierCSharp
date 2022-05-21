﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

using System.Numerics;
using DSPLib;
using PlotWrapper;

namespace FourierCSharp
{
    public partial class MainForm : Form
    {
        private string appName = " - Fourier Transform";
        private static double[] inputSignal;
        private static double[] lmSpectrum;
        private static double[] freqSpan;
        private static double samplingRate = 10.0; // trong 1s lấy 10 lần
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

            // Instantiate a new DFT
            DFT dft = new DFT();

            // Initialize the DFT
            // You only need to do this once or if you change any of the DFT parameters.
            dft.Initialize((uint)inputSignal.Length);
            // Call the DFT and get the scaled spectrum back
            Complex[] cSpectrum = dft.Execute(inputSignal);

            // Convert the complex spectrum to magnitude
            lmSpectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);

            // For plotting on an XY Scatter plot, generate the X Axis frequency Span
            freqSpan = dft.FrequencySpan(samplingRate);
            DrawGraph();

            // Find magnitude max then multify frequency with 60
            double maxValue = lmSpectrum.Max();
            int indexMax = lmSpectrum.ToList().IndexOf(maxValue);

            // finded
            if(indexMax != -1)
            {
                //Count the number of breaths in 1 second
                var f = freqSpan[indexMax];
                var totalBreathsInOneMininute = f != 0 ? f * 60 : 60;
                MessageBox.Show("Số lần thở trong 1 phút của bạn là: " + totalBreathsInOneMininute);
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
            inputSignal = new double[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "") continue;
                //var linesData = lines[i].Split(',');
                inputSignal[i] = double.Parse(lines[i]) / 1000;
            }
        }

        private void DrawGraph()
        {
            if (inputSignal == null) return;
            DrawRealData();
            SetDataGridViewDataInput();
            /*Plot plot1 = new Plot("Signal Time Domain", "Second (s)", "Input data");
            plot1.PlotData(inputTimeDomain, inputSignal);
            plot1.Show();*/

            DrawMagnitudeData();
            SetDataGridViewDataOutput();
            /*Plot plot2 = new Plot("Signal frequency Domain", "Frequency (Hz)", "Magnitude");
            plot2.PlotData(freqSpan, lmSpectrum);
            plot2.Show();*/
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
                seriesIm.Points.AddXY(i * 0.2, inputSignal[i]);
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
                dgv.Rows.Add(i * 0.2, inputSignal[i]);
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

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
