using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VentilatorTestConsole
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void JoinVentViaIP_Clicked(object sender, EventArgs e)
        {
            string text = IPEntry.Text;
            (Application.Current as App).ComService.ConnectToVentilator(IPAddress.Parse(text));
        }

        private void NearbyVents_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            (Application.Current as App).ComService.ConnectToVentilator(IPAddress.Parse(((Ventilator)e.Item).IP));
        }
    }
}
