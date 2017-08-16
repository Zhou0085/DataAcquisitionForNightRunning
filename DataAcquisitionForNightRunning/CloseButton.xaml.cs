using GMap.NET.WindowsPresentation;
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

namespace DataAcquisitionForNightRunning
{
    /// <summary>
    /// Interaction logic for MyIcon.xaml
    /// </summary>
    public partial class CloseButton
    {
        GMapControl _MapControl;
        Guid _Id;
        List<GMapMarker> _MarkerList;
        public CloseButton(GMapControl mapControl, List<GMapMarker> markerList, Guid id, double width, double height)
        {
            this.InitializeComponent();

            this.Width = width;
            this.Height = height;

            this._MapControl = mapControl;
            this._Id = id;
            this._MarkerList = markerList;

            this.Loaded += new RoutedEventHandler(CloseButton_Loaded);
            this.MouseDown += CloseButton_MouseDown;
            this.MouseWheel += CloseButton_MouseWheel;
        }

        private void CloseButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _MapControl._OnMouseWheel(e);
        }

        void CloseButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int count = this._MapControl.Markers.Count;
            for (int i = 0; i < count; i++)
            {
                if (_MapControl.Markers[i].Tag != null && ((Guid)_MapControl.Markers[i].Tag) == this._Id)
                {
                    if(_MapControl.Markers[i].ZIndex == 1)
                        _MarkerList.Remove(_MapControl.Markers[i]);
                    _MapControl.Markers.RemoveAt(i);
                    i--; count--;
                }
            }
            e.Handled = true;
        }
    }
}