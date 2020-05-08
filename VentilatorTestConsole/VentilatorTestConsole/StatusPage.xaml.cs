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
            Children.Add(new PatientStatusView(Patient.A));
            Children.Add(new PatientStatusView(Patient.B));
            InitializeComponent ();
		}
	}
}