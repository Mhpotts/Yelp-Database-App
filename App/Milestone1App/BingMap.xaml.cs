using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using Microsoft.Maps.MapControl.WPF.Design;
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
using System.Windows.Shapes;

namespace YelpApp
{
    /// <summary>
    /// Interaction logic for BingMap.xaml
    /// </summary>
    public partial class BingMap : Window
    {
        private MainWindow form;
        public BingMap(MainWindow form)
        {
            this.form = form;
            InitializeComponent();
            this.Content = CreateMap();
        }

        private Map CreateMap()
        {
            Map map = new Map();
            string cred = "Ap7PyaJT_ohwGU8 - ltzK9BqbNujuoD1JvLnW3AsB5iuE0D5v28yCUI - 99mKynBkl";
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(cred);
            foreach (Business B in form.businessGrid.Items)
            {
                Pushpin pin = new Pushpin();
                Location loc = new Location(B.latitude, B.longitude);
                pin.Location = loc;
                pin.ToolTip = B.name;
                map.Children.Add(pin);
            }

            if (form.businessGrid.SelectedItem != null)   
            {
                Business b = (Business)form.businessGrid.SelectedItem;
                map.Center = new Location(b.latitude, b.longitude);
                
            }
            else if (!string.IsNullOrEmpty(form.latitudeVal.Text) && !string.IsNullOrEmpty(form.longitudeVal.Text))
            {
                double lat = double.Parse(form.latitudeVal.Text);
                double lng = double.Parse(form.longitudeVal.Text);

                map.Center = new Location(lat, lng);
            }
            else if (map.Children.Count > 0)
            {
                Pushpin first = (Pushpin)map.Children[0];
                map.Center = first.Location;
            }
            else
            {
                map.Center = new Location(48.004779, -122.197620);
            }
            map.Mode = new AerialMode(true);
            map.ZoomLevel = 16;
            return map;
        }
    }
}
