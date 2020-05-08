using System;
using System.Collections.Generic;
using System.Text;

namespace VentilatorTestConsole
{
    public class PatientStatusService
    {
        public PatientStatus Patient1;
        public PatientStatus Patient2;

        public PatientStatusService()
        {
            Patient1 = new PatientStatus(Patient.A);
            Patient2 = new PatientStatus(Patient.B);
        }
    }
}
