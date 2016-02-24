using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using Microsoft.Research.DynamicDataDisplay;
using System.Threading;
using Microsoft.Research.DynamicDataDisplay.Filters;
using Point = System.Windows.Point;


namespace CombinedGraph
{

    public partial class MainWindow : Window
    {
        DataWaterfall WaterfallData = new DataWaterfall();
        DataBaseLoader DataBase = new DataBaseLoader();
        AsynchronousClient SocketData = new AsynchronousClient();
        Bitmap bmpWaterfall;
        List<FreqData> SinglePoint = new List<FreqData>();
        List<FreqData> AmplNormal = new List<FreqData>();
        List<FreqData> Averages = new List<FreqData>();
        List<FreqData> CurrentData = new List<FreqData>();
        List<FreqData> AllPoints = new List<FreqData>();
        List<FreqData> AverageLine = new List<FreqData>();
        DataLine DataLoaderLine = new DataLine();
        //FreqData Points = new FreqData();
        DispatcherTimer timerWaterfall = new DispatcherTimer();
        public volatile bool _stopThread ;
        public int CurrentPosition { get; set; }     
        public int Max
        {
            get;
            set;
        }
        public int Min
        {
            get;
            set;
        }
        EnumerableDataSource<double> animatedDataSource = null;
        public List<FreqData> ValuesData = new List<FreqData>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        //public EventWaitHandle DrawWait = new AutoResetEvent();
        private double[] animatedX = new double[2000];
        private double[] animatedY = new double[2000];
        public int count = 0;
        public Thread  DrawingThread, SocketThread;
        private ObservableDataSource<System.Windows.Point> DataSource = null;
        public DispatcherTimer LineTimer;
        public DataBaseLoader Database;
        public int Socket_State = 0;
        public int Database_State = 0;
        private string Server_Adress;
        private string Port_Number;
        private string Dns_Host;
        ServerData server = new ServerData();


        public MainWindow()
        {
            
            InitializeComponent();
            
            
        }
        public class CustomAxisRestriction : ViewportRestrictionBase
        {
            private double xMin;
            private double yMin;
            private double Height;
            private double Width;
            public CustomAxisRestriction(double xMin, double yMin, double Height, double Width)
            {
                this.xMin = xMin;
                this.yMin = yMin;
                this.Height = Height;
                this.Width = Width;
            }
            public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
            {
                newDataRect.X = xMin;
                newDataRect.Y = yMin;
                newDataRect.Width = Width;
                newDataRect.Height = Height;
                return newDataRect;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Max = 0;
            Min = 0;
            //var Points = await SocketData.StartClient();
            //WaterfallPoints.Add(new FreqData { Freq = Points.Freq, Ampl = Points.Ampl });
            //AllPoints.Add(new FreqData { Freq = Points.Freq, Ampl = Points.Ampl });
            RenderOptions.SetEdgeMode(plotter, EdgeMode.Aliased);
            plotter.Visible = new Rect { X = 10, Width = 10, Y = 0, Height = 22 };
            this.Mapping();
            LineTimer = new DispatcherTimer();
            LineTimer.Tick += OnTimerEvent;
            LineTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            this.CurrentPosition = 0;
            this.CreateBitmap();
            
            CurrentPosition = 0;
            plotter.VerticalAlignment = VerticalAlignment.Top;
            //plotter.Background = Brushes.Black;
            plotter.AxisGrid.Visibility = Visibility.Hidden;
            //SocketThread = new Thread(new ThreadStart(DrawLineSocket)); 
            SocketThread = new Thread(new ThreadStart(LineTimer.Start));
            SocketThread.IsBackground = true;
            SocketThread.Name = "SocketThread";
            SocketThread.SetApartmentState(ApartmentState.STA);
            

        }

        private async void OnTimerEvent(object sender, EventArgs e)
        {
            if (CurrentPosition == 0)
            {
                int i = 0;
                CurrentData.Clear();
                if (Database_State == 1)
                {
                    AllPoints = DataBase.DataLoader();
                }
                else if (Socket_State == 1)
                {
                    AllPoints = await SocketData.StartClient(Server_Adress, Port_Number, Dns_Host);
                }
                 if (Min == 0 && Max == 0)
                 {
                     CurrentData = AllPoints;
                     AverageLine = WaterfallData.ConvertToDouble(CurrentData);
                 }
                 else
                 {
                     while (i < AllPoints.Count)
                     {

                         if (AllPoints[i].Freq > Min && AllPoints[i].Freq < Max)
                         {
                             CurrentData.Add(
                                 new FreqData
                                 {
                                     Ampl = AllPoints[i].Ampl,
                                     Freq = AllPoints[i].Freq
                                 });
                         }
                         i++;
                     }
                     AverageLine = WaterfallData.ConvertToDouble(CurrentData);
                 }
            }
            Dispatcher.Invoke(Change);
            CurrentPosition++;
            if(CurrentPosition == 1000)
            {
                Dispatcher.Invoke(OnTimerClick);
                CurrentPosition = 0;
            }
        }

        private void OnTimerClick()
        {
            if (SinglePoint == null)
            {
                timerWaterfall.Stop();               
            }
            Averages = this.WaterfallData.ConvertToDouble(CurrentData);
            AmplNormal = DataWaterfall.Normalise(Averages);
            this.ChangeRow(this.bmpWaterfall);
            this.Dispatcher.Invoke(new Action(() => Waterfall.Source = ToWpfBitmap(this.bmpWaterfall)));
            AllPoints.Clear();
            //WaterfallPoints.Clear();
            //WaterfallPoints.Add(new FreqData { Freq = SinglePoint.Last().Freq, Ampl = SinglePoint.Last().Ampl });
        }
       
        private void Mapping()
        {
           
            var dsX = new EnumerableDataSource<double>(animatedX);
            dsX.SetXMapping(ci => ci);
            this.animatedDataSource = new EnumerableDataSource<double>(animatedY);
            this.animatedDataSource.SetYMapping(ci => ci);

            plotter.AddLineGraph(new CompositeDataSource(dsX, this.animatedDataSource),
                new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 1),
                new PenDescription("Frequency"));
            plotter.FitToView();
           
        }

        private void Change()
        {
                     
            this.animatedX[CurrentPosition] = (double)AverageLine[CurrentPosition].Freq;
            this.animatedY[CurrentPosition] = AverageLine[CurrentPosition].Ampl;
            Dispatcher.Invoke(new Action (() =>  animatedDataSource.RaiseDataChanged()));
           // Thread.Sleep(1);
         
        }

        //BITMAP
        public void CreateBitmap()
        {
            Bitmap waterfall = new Bitmap(1000, 256);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, waterfall.Width, waterfall.Height);
            System.Drawing.Imaging.BitmapData waterfallData = waterfall.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, waterfall.PixelFormat);
            IntPtr pointer = waterfallData.Scan0;
            int bytes = Math.Abs(waterfallData.Stride) * waterfall.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(pointer, rgbValues, 0, bytes);
            for (int counter = 2; counter < rgbValues.Length; counter += 3)
                rgbValues[counter] = 40;
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pointer, bytes);
            waterfall.UnlockBits(waterfallData);
            this.bmpWaterfall = waterfall;
            Waterfall.Source = ToWpfBitmap(waterfall);
        }
        private void ChangeRow(Bitmap bitmap)
        {
            MoveBitmapDown(bitmap);
            int r, g, b;

            for (int i = 0; i < bitmap.Width; i++)
            {

                //Sub.ColorFunction(AmplNormal[i].Ampl, out r, out g, out b);
                Sub.ColorFunction(AmplNormal[i].Ampl, out r, out g, out b);
                System.Drawing.Color AmplColor = System.Drawing.Color.FromArgb(r, g, b);

                bitmap.SetPixel(i, 0, AmplColor);
            }
            //return bitmap;
        }

        private static void MoveBitmapDown(Bitmap bitmap)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            BitmapData rowData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            IntPtr pntrStart = rowData.Scan0;
            IntPtr pntrRow1 = rowData.Scan0 + rowData.Stride;
            int bytes = (rowData.Stride) * rowData.Height;
            int bytesToCopy = (rowData.Stride) * (rowData.Height - 1);
            byte[] rgbvalues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(pntrStart, rgbvalues, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, pntrRow1, bytesToCopy);

            bitmap.UnlockBits(rowData);
        }
        public static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }    

        private void GeneralStart_Click(object sender, RoutedEventArgs e)
        {
            if (Max == 0 && Min == 0)
            {
                plotter.Viewport.Restrictions.Add(new CustomAxisRestriction(95, (-140), 60, 3000));
            }
            else
            {
                plotter.Viewport.Restrictions.Add
                    (new CustomAxisRestriction(Min, (-140), 60, (Max - Min)));
            }
            Database_State = 1;
            SocketThread.Start();
        }
        
        private void ReadSocket_Click(object sender, RoutedEventArgs e)
        {
            if (Max == 0 && Min == 0)
            {
                plotter.Viewport.Restrictions.Add(new CustomAxisRestriction(95, (-140), 60, 3000));
            }
            else
            {
                plotter.Viewport.Restrictions.Add
                    (new CustomAxisRestriction(Min, (-140), 60, (Max - Min)));
            }
            Socket_State = 1;
            SocketThread.Start();
        }

        private void MaxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MaxValue.Text == "")
            {
                return;
            }
            else
            {
                Max = Convert.ToInt32(MaxValue.Text);
            }
        }

        private void MinValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MinValue.Text == "")
            {
                return;
            }
            else
            {
                Min = Convert.ToInt32(MinValue.Text);
            }
        }

        private void STOP_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Password_TextChanged(object sender, TextChangedEventArgs e)
        {
         
            server.Password = Password.Text;
        }
        private void User_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            server.UserName = User.Text;
        }

        private void Server_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            server.ServerAdress = Server.Text;
        }

        private void Name_OnTextChanged(object sender, TextChangedEventArgs e)
        {
           
            server.NameDatabase = Name.Text;
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            //airscanEntities database = new airscanEntities();
           
            DbServer init = new DbServer();
            init.Db_Init(server);

        }

        private void Address_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Server_Adress = Address.Text;
        }

        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            Port_Number = Port.Text;
        }

        private void Dns_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dns_Host = Dns.Text;
        }
    }
}
