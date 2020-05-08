using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            NearbyVents.ItemsSource = (Application.Current as App).ComService.FoundVentilators;
        }

        private void JoinVentViaIP_Clicked(object sender, EventArgs e)
        {
            string text = IPEntry.Text;
            (Application.Current as App).ComService.ConnectToVentilator(IPAddress.Parse(text)).ContinueWith((res) =>
            {
                Debug.WriteLine(res.IsFaulted);
                Debug.WriteLine(res.Exception);
            });
            this.Navigation.PushAsync(new StatusPage());
        }

        private void NearbyVents_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            (Application.Current as App).ComService.ConnectToVentilator(IPAddress.Parse(((Ventilator)e.Item).IP)).ContinueWith((res) =>
            {
                Debug.WriteLine(res.IsFaulted);
                Debug.WriteLine(res.Exception);
            });
            this.Navigation.PushAsync(new StatusPage());
        }
    }
}
