using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Graph_UserControl.Class;
using System.Windows.Threading;

namespace Graph_UserControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public delegate void ReadDele(int runtime);
    public delegate void DrawDele(string runtime, string yvalue, bool IsSV);

    public partial class DrawGraphUC : UserControl
    {
        const int SAMPLE5SEC = 5;
        const int SAMPLE10SEC = 10;
        const int SAMPLE20SEC = 20;
        const int SAMPLE30SEC = 30;

        const int HOURS3 = 10801;
        const int HOURS12 = 43201;
        const int HOURS18 = 64801;
        const int HOURS24 = 86401;
        const int HOURS72 = 259200;

        const int MIN_TEMP = -100;
        const int MAX_TEMP = 100;
        const int TEMP_STEP = 10;

        const int X_OFFSET = -4;

        const double SCALERATE = 1.002;
        const double SCALEPLUS = 1.1;
        const double SCALEMINUS = 1 / 1.1;

        const double THRESHOLD = 5.0;

        private static System.Timers.Timer timerwrite;
        WriteLineTask writethread;

        private DateTime StartTime;

        int runtime = 1;
        int x1 = 0;

        //double gridwidth;
        bool IsClicked = false;
        bool StopbuttonClicked = false;
        int Samplerate = 1;

        bool IsSVchecked = false;
        bool IsPVchecked = false;

        double gridX = 20;

        Stack<Point> Positions = new Stack<Point>();
        List<TroubleShoot> items = new List<TroubleShoot>();

        Dictionary<string, Ellipse> pointdictionary = new Dictionary<string, Ellipse>();

        public DrawGraphUC()
        {
            InitializeComponent();
        }

        public DrawGraphUC(int sv)
        {
            InitializeComponent();

            plotcanvas.Width = graphgrid.ColumnDefinitions[3].ActualWidth;  //problem here after stop - start button click
            plotcanvas.Height = graphgrid.RowDefinitions[0].ActualHeight + graphgrid.RowDefinitions[1].ActualHeight + graphgrid.RowDefinitions[2].ActualHeight +
                graphgrid.RowDefinitions[3].ActualHeight + graphgrid.RowDefinitions[4].ActualHeight;

            AxisCanvas.Height = graphgrid.RowDefinitions[0].ActualHeight + graphgrid.RowDefinitions[1].ActualHeight + graphgrid.RowDefinitions[2].ActualHeight +
                graphgrid.RowDefinitions[3].ActualHeight + graphgrid.RowDefinitions[4].ActualHeight;


            // Draw Y-Axis on the left
            DrawAxis();

            StartTime = DateTime.Now;

            StopbuttonClicked = false;

            if (writethread == null)
            {
                writethread = new WriteLineTask("write", runtime, sv, 0);
            }

            if (IsClicked)
            {
                timerwrite.Stop();
            }

            timerwrite = new System.Timers.Timer(100);       //event occurs every 1 second
            timerwrite.Elapsed += OnTimedEvent;
            timerwrite.AutoReset = true;
            timerwrite.Start();

            IsClicked = true;
        }

        ///// <summary>
        ///// Read thread starts when button is clicked. 
        ///// Read from (runtime - seconds) to (runtime), and plot the graph.
        ///// </summary>
        ///// <param name="sender">not used</param>
        ///// <param name="e">not used</param>

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // gridwidth = graphgrid.ColumnDefinitions[3].ActualWidth;

            //Initialize after button click

            plotcanvas.Width = graphgrid.ColumnDefinitions[3].ActualWidth;  //problem here after stop - start button click
            plotcanvas.Height = graphgrid.RowDefinitions[0].ActualHeight + graphgrid.RowDefinitions[1].ActualHeight + graphgrid.RowDefinitions[2].ActualHeight +
                graphgrid.RowDefinitions[3].ActualHeight + graphgrid.RowDefinitions[4].ActualHeight;

            AxisCanvas.Height = graphgrid.RowDefinitions[0].ActualHeight + graphgrid.RowDefinitions[1].ActualHeight + graphgrid.RowDefinitions[2].ActualHeight +
                graphgrid.RowDefinitions[3].ActualHeight + graphgrid.RowDefinitions[4].ActualHeight;


            // Draw Y-Axis on the left

            DrawAxis();

            StartTime = DateTime.Now;

            StopbuttonClicked = false;

            if (writethread == null)
            {
                writethread = new WriteLineTask("write", runtime, int.Parse(svbox.Text), 0);
            }

            DrawGraphUC drawgraphuc = new DrawGraphUC();

            if (IsClicked)
            {
                timerwrite.Stop();
            }

            timerwrite = new System.Timers.Timer(100);       //event occurs every 1 second
            timerwrite.Elapsed += OnTimedEvent;
            timerwrite.AutoReset = true;
            timerwrite.Start();

            IsClicked = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopbuttonClicked = true;
            MessageBox.Show("Graph stops");
        }

        /// <summary>
        /// Draw Y-Axis from MAX_TEMP to MIN_TEMP by TEMP_STEP
        /// Y축 맥스 온도부터 민까지 TEMP_STEP에 맞춰서 캔버스에 그려줌
        /// </summary>
        private void DrawAxis()
        {
            for (int Yposition = MIN_TEMP; Yposition < MAX_TEMP + 1; Yposition += TEMP_STEP)
            {
                TextBlock txt = new TextBlock();
                txt.Text = Yposition.ToString();
                Canvas.SetLeft(txt, 0);
                Canvas.SetBottom(txt, ConverttoCanvasY(Yposition));
                AxisCanvas.Children.Add(txt);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            writethread.Run();


            if (writethread.Runtime > HOURS72)       //259200 : 72hours
            {
                timerwrite.Stop();
                /*
                 * Move screen to the right, and delete ui with dictionary before.
                 * Plotafter3days function
                */

                //if (plotcanvas.Children. <= writethread.Runtime - HOURS72)
                //{
                //    plotcanvas.Children.RemoveAt(0);
                //}

                //Dispatcher.Invoke(DispatcherPriority.Normal, new ReadDele(Plotafter3days));

                //plotcanvas.RenderTransform = new TranslateTransform { X = HOURS72 };
            }

            else
            {

                if ((Samplerate != SAMPLE5SEC) && (writethread.Runtime >= HOURS3) && (writethread.Runtime < HOURS12))                //3200 ~ 25600
                {
                    Samplerate = SAMPLE5SEC;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new ReadDele(Removepoints), Samplerate);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new ReadDele(Intervaltextchange), Samplerate);
                }

                else if ((Samplerate != SAMPLE10SEC) && (writethread.Runtime >= HOURS12))         //38400 ~ 57600
                {
                    Samplerate = SAMPLE10SEC;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new ReadDele(Removepoints), Samplerate);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new ReadDele(Intervaltextchange), Samplerate);
                }

                // CurrInvoke

                runtime = writethread.Runtime - 1;

                if ((runtime) % Samplerate == 0 && !(StopbuttonClicked))
                {
                    if (writethread.SV.ToString() != writethread.PV.ToString())     // Remove same point
                        Dispatcher.Invoke(DispatcherPriority.Normal, new DrawDele(PlotGraph), runtime.ToString(), writethread.SV.ToString(), true);

                    Dispatcher.Invoke(DispatcherPriority.Normal, new DrawDele(PlotGraph), runtime.ToString(), writethread.PV.ToString(), false);
                }
            }
        }

        private void Plotafter3days(int runtime)
        {
            //foreach (UIElement ui in plotcanvas.Children)
            //{
            //    Canvas.SetLeft(ui, Canvas.GetLeft(ui) + 1);
            //}

            // plotcanvas.RenderTransform = new TranslateTransform { X = 10 };

        }

        //private void M_ReadinvokeEvent(int runtime)
        //{
        //    writethread.IsReading = true;
        //    readtask = new ReadTask("read", runtime, Samplerate);
        //    readtask = new Thread(readtask.Run);
        //    readtask.Start();
        //    while (readtask.IsAlive) ;
        //    writethread.IsReading = false;
        //}

        //Blue Circle SV, Red Circle PV
        public void PlotGraph(string runtime, string temperature, bool IsSV)
        {
            if (IsClicked == true)      //only initialize once
            {
                IsClicked = false;
            }

            int positionx = ConverttoCanvasX(int.Parse(runtime));
            double positiony = ConverttoCanvasY(double.Parse(temperature));
            string uid = "";

            Ellipse Dot = new Ellipse();
            Dot.Width = 5;
            Dot.Height = 5;

            if (IsSV)
            {
                Dot.Fill = Brushes.Blue;
                uid = "sv," + runtime.ToString() + "," + temperature.ToString();
                Dot.Uid = uid;
                Dot.MouseEnter += ellipse_MouseEnter;

                if (!pointdictionary.ContainsKey(uid))
                    pointdictionary.Add(uid, Dot);

                if (!IsSVchecked)
                {
                    return;
                }
            }
            else
            {
                if (Convert.ToDouble(temperature) > writethread.SV + THRESHOLD || Convert.ToDouble(temperature) < writethread.SV - THRESHOLD)
                {
                    Dot.Fill = Brushes.Red;
                    Dot.Width = 10;
                    Dot.Height = 10;
                    Dot.StrokeThickness = 1;
                    Dot.Stroke = Brushes.Black;
                    uid = "out," + runtime.ToString() + "," + temperature.ToString();
                    Dot.Uid = uid;
                    Dot.MouseEnter += ellipse_MouseEnter;

                    pvblock.Foreground = Brushes.Red;
                    pvblock.Text = temperature.ToString();

                    if (!pointdictionary.ContainsKey(uid))
                        pointdictionary.Add(uid, Dot);

                    ShowLog();
                }
                else
                {
                    Dot.Fill = Brushes.Green;
                    uid = "pv," + runtime.ToString() + "," + temperature.ToString();
                    Dot.Uid = uid;
                    Dot.MouseEnter += ellipse_MouseEnter;

                    pvblock.Foreground = Brushes.Red;
                    pvblock.Text = temperature.ToString();

                    if (!pointdictionary.ContainsKey(uid))
                        pointdictionary.Add(uid, Dot);

                    if (!IsPVchecked)
                    {
                        return;
                    }
                }
            }

            //increase canvas width as points are out of canvas
            if ((int.Parse(runtime) - 1) > plotcanvas.Width)
            {
                plotcanvas.Width *= SCALERATE;
                st.ScaleX /= SCALERATE;

                gridX *= SCALERATE;

                gridline.Viewbox = new Rect(0, 0, gridX, 20);
                gridline.Viewport = new Rect(0, 0, gridX, 20);
                gridrect.Width = gridX;

            }

            Canvas.SetLeft(Dot, positionx);
            Canvas.SetBottom(Dot, positiony);

            plotcanvas.Children.Add(Dot);

            x1++;
        }


        private void ellipse_MouseEnter(object sender, EventArgs e)
        {
            Ellipse el = sender as Ellipse;

            string[] words = el.Uid.Split(',');

            if (words[0] == ("sv"))
            {
                pvvalue.Text = words[0] + " : " + words[2];
                pvvalue.Foreground = Brushes.Blue;
            }
            else if (words[0] == ("pv"))
            {
                pvvalue.Text = words[0] + " : " + words[2];
                pvvalue.Foreground = Brushes.Green;
            }
            else
            {
                pvvalue.Text = words[0] + " : " + words[2];
                pvvalue.Foreground = Brushes.Red;
            }

        }

        /// <summary>
        /// Remove dots from graph as the sampling rate changes after hours.
        /// 그래프에 그려진 점들을 샘플링 레이트에 맞춰서 지워줍니다. (샘플레이트가 변경되었을때만)
        /// </summary>
        /// <param name="samplingrate"></param>
        private void Removepoints(int samplingrate)
        {
            List<Ellipse> itemstoremove = new List<Ellipse>();

            foreach (UIElement ui in plotcanvas.Children)
            {
                if (ui is Ellipse)
                {
                    string[] words = ui.Uid.Split(',');
                    string word = words[0];
                    string runtime1 = words[1];
                    if ((int.Parse(words[1]) % samplingrate != 0))
                    {
                        itemstoremove.Add((Ellipse)ui);
                    }
                }
            }

            foreach (Ellipse ui in itemstoremove)
            {
                if (pointdictionary.ContainsKey(ui.Uid))
                {
                    pointdictionary.Remove(ui.Uid);
                }

                plotcanvas.Children.Remove(ui);
            }

        }

        private void Intervaltextchange(int samplingrate)
        {
            intervaltext.Text = samplingrate.ToString();
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            //if (writethread != null)
            //    writethread.Abort();
            //if (currentthread != null)
            //    currentthread.Abort();
            //MessageBox.Show("Write&Curr Thread Abort");
        }

        private void Plotcanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MatrixTransform matTrans = mt;
            Point posx = e.GetPosition(CanvasGrid);

            //posx.X = posx.X * (originwidth / chart_Canvas.Width);     //창모드에서도 줌 가능하게 해줌.
            //posx.Y = posx.Y * (originheight / chart_Canvas.Height);

            // double width = canvasScroll.ActualWidth;
            double scale = e.Delta > 0 ? SCALEPLUS : SCALEMINUS;

            Matrix mat = matTrans.Matrix;

            if (e.Delta > 0)
            {
                Positions.Push(posx);   //push posx into stack.
                mat.ScaleAt(scale, scale, posx.X, posx.Y);
                matTrans.Matrix = mat;
                e.Handled = true;

            }
            else
            {
                if ((mat.M11 * SCALEMINUS) < 1)
                {
                    return;
                }
                else
                {
                    posx = Positions.Pop(); //pop posx from stack.
                    mat.ScaleAt(scale, scale, posx.X, posx.Y);
                    matTrans.Matrix = mat;
                    e.Handled = true;
                }

            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            //CanvasClear("", "");
            plotcanvas.Children.RemoveAt(0);

        }

        private void CanvasClear(string runtime, string pv)
        {
            plotcanvas.Children.Clear();
        }

        private void SV_Checked(object sender, RoutedEventArgs e)
        {
            IsSVchecked = true;

            foreach (KeyValuePair<string, Ellipse> entry in pointdictionary)
            {
                string[] words = entry.Key.Split(',');

                bool isSV = false;
                string word = words[0];
                if (words[0].StartsWith("sv"))
                {
                    isSV = true;
                    PlotGraph(words[1], words[2], isSV);
                }
                else
                {
                    isSV = false;
                }
            }
        }

        private void SV_Unchecked(object sender, RoutedEventArgs e)
        {
            List<Ellipse> itemstoremove = new List<Ellipse>();

            IsSVchecked = false;

            foreach (UIElement ui in plotcanvas.Children)
            {
                if (ui is Ellipse)
                {
                    if (ui.Uid.StartsWith("sv"))
                    {
                        itemstoremove.Add((Ellipse)ui);
                    }
                }
            }

            foreach (Ellipse ui in itemstoremove)
            {
                plotcanvas.Children.Remove(ui);
            }

        }

        private void PV_Checked(object sender, RoutedEventArgs e)
        {
            IsPVchecked = true;

            foreach (KeyValuePair<string, Ellipse> entry in pointdictionary)
            {
                string[] words = entry.Key.Split(',');

                bool isSV = false;
                if (words[0].StartsWith("pv"))
                {
                    PlotGraph(words[1], words[2], isSV);
                }
            }

        }

        private void PV_Unchecked(object sender, RoutedEventArgs e)
        {
            List<Ellipse> itemstoremove = new List<Ellipse>();

            foreach (UIElement ui in plotcanvas.Children)
            {
                if (ui is Ellipse)
                {
                    if (ui.Uid.StartsWith("pv"))
                    {
                        itemstoremove.Add((Ellipse)ui);
                    }
                }
            }
            foreach (Ellipse ui in itemstoremove)
            {
                plotcanvas.Children.Remove(ui);
            }

            IsPVchecked = false;
        }

        private void ShowLog()
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:s");

            string text = getTimenow();

            items.Add(new TroubleShoot()
            {
                Time = text,
                SV = writethread.SV.ToString(),
                Error = writethread.PV.ToString()
            });

            logbox.ItemsSource = items;
            logbox.Items.Refresh();
        }

        private string getTimenow()
        {
            TimeSpan elapsed = DateTime.Now - StartTime;

            // Start with the days if greater than 0.
            string text = "";
            if (elapsed.Days > 0)
                text += elapsed.Days.ToString() + ".";

            //Compose the rest of the elapsed time.
            text +=
                elapsed.Hours.ToString("00") + ":" +
                elapsed.Minutes.ToString("00") + ":" +
                elapsed.Seconds.ToString("00");
            return text;
        }


        /// <summary>
        /// Convert from runtime to Canvas x coordinates. 
        /// </summary>
        /// <param name="runtime">runtime value</param>
        /// <returns></returns>
        private int ConverttoCanvasX(int runtime)
        {
            return runtime + X_OFFSET;
        }

        /// <summary>
        /// Convert from temperature to Canvas y coordinates.
        /// </summary>
        /// <param name="temperature">sv, pv value</param>
        /// <returns></returns>
        private double ConverttoCanvasY(double temperature)
        {
            return (plotcanvas.Height / 2) + 4 * temperature;  // Center zero point
        }

    }
}
