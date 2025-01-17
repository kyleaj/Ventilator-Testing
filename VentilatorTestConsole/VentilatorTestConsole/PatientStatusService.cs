﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VentilatorTestConsole
{
    public class PatientStatusService
    {
        public enum TestStatus
        {
            Running,
            Stopped,
            Complete,
            Error
        }

        public PatientStatus Patient1;
        public PatientStatus Patient2;

        public TestStatus CurrTest;

        public PatientStatusService()
        {
            Patient1 = new PatientStatus(Patient.A);
            Patient2 = new PatientStatus(Patient.B);
            CurrTest = TestStatus.Stopped;
        }
    }
}
