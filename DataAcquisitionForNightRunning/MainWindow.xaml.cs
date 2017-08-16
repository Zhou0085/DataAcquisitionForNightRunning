using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using GMap.NET.MapProviders;
using Microsoft.Win32;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;

namespace DataAcquisitionForNightRunning
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<GMapMarker> markerList = new List<GMapMarker>();

        private List<PointLatLng> pllListFromFile = new List<PointLatLng>();

        private PointLatLng? startPll = null;

        private double lengthOfRouth = 1000;

        bool addStart = false;

        public MainWindow()
        {
            InitializeComponent();

            mapControl.ShowCenter = false;
            mapControl.Position = new PointLatLng(30.541, 114.361);
            mapControl.MapProvider = GMapProviders.OpenStreetMap;
            mapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mapControl.MinZoom = 7;
            mapControl.MaxZoom = 20;
            mapControl.Zoom = 12;
            mapControl.DragButton = MouseButton.Right;

            mapControl.MouseLeftButtonDown += MapControl_MouseLeftButtonDown;

            GenerateMarkers();
            if(pllListFromFile.Count != 0)
            {
                foreach(PointLatLng pll in pllListFromFile)
                {
                    Guid id = Guid.NewGuid();

                    GMapMarker marker = new GMapMarker(pll);
                    {
                        marker.Shape = new MarkerShape(mapControl, new BitmapImage(new Uri("pack://application:,,,/Icon/大头针.png")), 16, 30, "Lng:" + pll.Lng.ToString("f2") + " Lat:" + pll.Lat.ToString("f2"));
                        marker.Offset = new Point(-8, -26);
                        marker.ZIndex = 1;
                        marker.Tag = id;
                    }
                    markerList.Add(marker);
                    mapControl.Markers.Add(marker);

                    GMapMarker closeButton = new GMapMarker(pll);
                    {
                        closeButton.Shape = new CloseButton(mapControl, markerList, id, 11, 11);
                        closeButton.Offset = new Point(6, -30);
                        closeButton.ZIndex = 2;
                        closeButton.Tag = id;
                    }
                    mapControl.Markers.Add(closeButton);
                }
            }
        }

        private void MapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(mapControl);
            PointLatLng pll = mapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
            if((aquireData.IsChecked == true ? true : false) && (!addStart))
            {
                Guid id = Guid.NewGuid();

                GMapMarker marker = new GMapMarker(pll);
                {
                    marker.Shape = new MarkerShape(mapControl, new BitmapImage(new Uri("pack://application:,,,/Icon/大头针.png")), 16, 30, "Lng:" + pll.Lng.ToString("f2") + " Lat:" + pll.Lat.ToString("f2"));
                    marker.Offset = new Point(-8, -26);
                    marker.ZIndex = 1;
                    marker.Tag = id;
                }
                markerList.Add(marker);
                mapControl.Markers.Add(marker);

                GMapMarker closeButton = new GMapMarker(pll);
                {
                    closeButton.Shape = new CloseButton(mapControl, markerList, id, 11, 11);
                    closeButton.Offset = new Point(6, -30);
                    closeButton.ZIndex = 2;
                    closeButton.Tag = id;
                }
                mapControl.Markers.Add(closeButton);
            }
            else if(addStart)
            {
                Guid id = Guid.NewGuid();

                startPll = pll;

                GMapMarker marker = new GMapMarker(pll);
                {
                    marker.Shape = new MarkerShape(mapControl, new BitmapImage(new Uri("pack://application:,,,/Icon/起点.png")), 30, 30, "Lng:" + pll.Lng.ToString("f2") + " Lat:" + pll.Lat.ToString("f2"));
                    marker.Offset = new Point(-15, -27);
                    marker.ZIndex = 3;
                    marker.Tag = id;
                }
                mapControl.Markers.Add(marker);

                GMapMarker closeButton = new GMapMarker(pll);
                {
                    closeButton.Shape = new CloseButton(mapControl, markerList, id, 11, 11);
                    closeButton.Offset = new Point(9, -33);
                    closeButton.ZIndex = 3;
                    closeButton.Tag = id;
                }
                mapControl.Markers.Add(closeButton);

                addStart = false;
            }
        }

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "(CSV)|*.csv|(所有文件)|*.*";
            dlg.FileName = "Data.csv";
            if(dlg.ShowDialog() == true ? true : false)
            {
                StreamWriter writer = new StreamWriter(dlg.FileName, false);
                int line = 0;
                foreach (GMapMarker marker in markerList)
                    writer.WriteLine((line++) + "," + marker.Position.Lng + "," + marker.Position.Lat);
                writer.Close();
                this.ShowMessageAsync("Message", "保存成功", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }
            
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            mapControl.Markers.Clear();
            markerList.Clear();
        }

        private void UpLoadData_Click(object sender, RoutedEventArgs e)
        {
            if (markerList.Count <= 0)
                return;
            try
            {
                string M_str_sqlcon = "server=139.129.166.245;user id=admin;password=admin;database=swtx"; //根据自己的设置
                MySqlConnection myCon = new MySqlConnection(M_str_sqlcon);
                myCon.Open();
                MySqlCommand mysqlcom = null;

                string selectMaxIdCmd = "SELECT max(id) FROM EndNodesOfRoutePlanning";
                mysqlcom = new MySqlCommand(selectMaxIdCmd, myCon);
                MySqlDataReader mysqlread = mysqlcom.ExecuteReader(CommandBehavior.CloseConnection);
                mysqlread.Read();

                int idNow = Convert.ToInt32(mysqlread.GetString(0));

                mysqlread.Close();
                foreach (GMapMarker marker in markerList)
                {
                    mysqlcom = new MySqlCommand("insert into EndNodesOfRoutePlanning(id, lng, lat) values(" + idNow + ", " + marker.Position.Lng + ", " + marker.Position.Lat + ")", myCon);
                    mysqlcom.ExecuteNonQuery();
                    idNow++;
                }
                mysqlcom.Dispose();
                myCon.Close();
                myCon.Dispose();
                this.ShowMessageAsync("Message", "上传成功", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }
            catch(Exception ex)
            {
                this.ShowMessageAsync("Exception", ex.ToString(), MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }
        }

        private void SelectStart_Click(object sender, RoutedEventArgs e)
        {
            addStart = true;
        }

        private void GenerateRoute_Click(object sender, RoutedEventArgs e)
        {
            if (startPll == null)
            {
                this.ShowMessageAsync("Warning", "未指定开始位置", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            pllListFromFile.Clear();
            GenerateMarkers();
            if (pllListFromFile.Count == 0)
            {
                this.ShowMessageAsync("Warning", "文件无内容", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }

            double x = double.MaxValue;
            double y = double.MaxValue;
            double z = double.MaxValue;
            mapControl.MapProvider.Projection.FromGeodeticToCartesian(((PointLatLng)startPll).Lat, ((PointLatLng)startPll).Lng, 0, out x, out y, out z);
            if (x == double.MaxValue || y == double.MaxValue)
            {
                this.ShowMessageAsync("Warning", "投影错误", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            double maxX = x + lengthOfRouth;
            double minX = x - lengthOfRouth;
            double maxY = y + lengthOfRouth;
            double minY = y - lengthOfRouth;
            double maxLng = double.MaxValue;
            double minLng = double.MaxValue;
            double maxLat = double.MaxValue;
            double minLat = double.MaxValue;
            mapControl.MapProvider.Projection.FromCartesianTGeodetic(maxX, maxY, z, out minLat, out minLng);
            mapControl.MapProvider.Projection.FromCartesianTGeodetic(minX, minY, z, out maxLat, out maxLng);
            if (maxLat == double.MaxValue || maxLng == double.MaxValue || minLat == double.MaxValue || maxLng == double.MaxValue)
            {
                this.ShowMessageAsync("Warning", "投影错误", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            List<PointLatLng> pllForRouteDesign = new List<PointLatLng>();
            foreach(PointLatLng pll in pllListFromFile)
            {
                if (pll.Lng < maxLng && pll.Lng > minLng && pll.Lat < maxLat && pll.Lat > minLat)
                    pllForRouteDesign.Add(pll);
            }
            if (pllForRouteDesign.Count == 0)
            {
                this.ShowMessageAsync("Warning", "无可用站点", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            double[] lengthForRoutes = new double[pllForRouteDesign.Count];
            GMapRoute[] routes = new GMapRoute[pllForRouteDesign.Count];
            int count = 0;
            foreach(PointLatLng pll in pllForRouteDesign)
            {
                MapRoute temp = GMapProviders.OpenStreetMap.GetRoute((PointLatLng)startPll, pll, false, false, (int)mapControl.Zoom);
                if(temp == null)
                {
                    routes[count] = null;
                    lengthForRoutes[count] = double.MaxValue;
                    count++;
                }
                else
                {
                    routes[count] = new GMapRoute(temp.Points);
                    lengthForRoutes[count] = temp.Distance * 1000;
                    count++;
                }
            }
            int indexOfNeareatRoute = 0;
            double NearestDistence = lengthForRoutes[0];
            for (int i=1;i< lengthForRoutes.Length;i++)
            {
                if (Math.Abs(lengthForRoutes[i] - lengthOfRouth) < Math.Abs(NearestDistence - lengthOfRouth))
                {
                    indexOfNeareatRoute = i;
                    NearestDistence = lengthForRoutes[i];
                }
            }
            mapControl.Markers.Add(routes[indexOfNeareatRoute]);
            this.ShowMessageAsync("Successful", "路径规划成功，长度：" + lengthForRoutes[indexOfNeareatRoute].ToString("f4"), MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
        }

        private void GenerateMarkers()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "(CSV)|*.csv|(所有文件)|*.*";
            dlg.FileName = "Data.csv";
            if(dlg.ShowDialog() == true ? true : false)
            {
                StreamReader reader = new StreamReader(dlg.FileName);
                string content = null;
                while((content = reader.ReadLine()) != null)
                {
                    string[] subContents = content.Split(',');
                    double lng = Convert.ToDouble(subContents[1]);
                    double lat = Convert.ToDouble(subContents[2]);
                    PointLatLng pll = new PointLatLng(lat, lng);
                    pllListFromFile.Add(pll);
                }
            }
        }
    }
}
