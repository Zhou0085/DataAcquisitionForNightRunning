using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// MarkerShape.xaml 的交互逻辑
    /// </summary>
    public partial class MarkerShape : UserControl
    {
        Popup Popup;
        Label Label;
        GMapControl mapControl;

        public MarkerShape(GMapControl map, ImageSource image, double width, double height, string tip = null)
        {
            this.InitializeComponent();

            mapControl = map;

            this.Width = width;
            this.Height = height;

            this.IsHitTestVisible = false;

            if (tip != null)
            {
                this.IsHitTestVisible = true;
                //this.ToolTip = tip;

                this.MouseEnter += MyIcon_MouseEnter;
                this.MouseLeave += MyIcon_MouseLeave;
                this.MouseWheel += MyIcon_MouseWheel;

                Popup = new Popup();
                Label = new Label();
                Popup.Placement = PlacementMode.Mouse;
                Popup.PlacementTarget = this;
                {
                    Label.Background = Brushes.White;
                    Label.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                    Label.BorderBrush = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                    Label.BorderThickness = new Thickness(1);
                    Label.Padding = new Thickness(5);
                    Label.FontSize = 16;
                    Label.Content = tip;
                }
                Popup.Child = Label;
            }

            icon.Source = image;
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

        private void MyIcon_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mapControl._OnMouseWheel(e);
        }

        private void MyIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = true;
        }

        private void MyIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}