using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Graph_UserControl;
//using System.Drawing;

namespace UIwithTimer
{

    public delegate void ReadDele(int runtime);
    public delegate void DrawDele(string runtime, string yvalue, bool IsSV);

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //public event ReadDele ReadinvokeEvent = null;
        //public event ReadDele CurrinvokeEvent = null;

        
        
        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            DrawGraphUC drawgraphuc = new DrawGraphUC();
            graphgrid.Children.Add(drawgraphuc);
        }

        private void WindowClosing(object sender, EventArgs e)
        {
        }       
        
    }
}
