using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VentilatorTestConsole
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StatusPage : TabbedPage
    {
		public StatusPage ()
		{
            Title = "Ventilator Status";
            Children.Add(new PatientStatusView(Patient.A) { Title="Patient 1" });
            Children.Add(new PatientStatusView(Patient.B) { Title="Patient 2" });
            InitializeComponent ();
		}

        private void CollectData_Clicked(object sender , EventArgs e)
        {
            if ((Application.Current as App).StatService.CurrTest != PatientStatusService.TestStatus.Running)
            {
                (Application.Current as App).ComService.StartTest().ContinueWith((res) =>
                {
                    if (!res.IsFaulted)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            StartStopTest.Text = "Stop Test";
                        });
                    }
                });
            } else
            {
                (Application.Current as App).ComService.StopTest().ContinueWith((res) =>
                {
                    if (!res.IsFaulted)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            StartStopTest.Text = "Start Test";
                        });
                    }
                });
            }
        }
	}
}