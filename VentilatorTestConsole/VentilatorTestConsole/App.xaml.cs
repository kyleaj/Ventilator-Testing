using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace VentilatorTestConsole
{
    public partial class App : Application
    {
        public CommunicationService ComService;
        public PatientStatusService StatService;

        public App()
        {
            InitializeComponent();
            StatService = new PatientStatusService();
            ComService = new CommunicationService();
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
