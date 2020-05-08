using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VentilatorTestConsole
{
    public class PatientStatus
    {
        public Patient Patient;
        public ShiftList<float> RecentVolumMeasurements;
        public ShiftList<float> RecentPressMeasurements;
        // More could be added, but we're running low on time

        public float Peep;
        public float TV;
        public float IE;

        public PatientStatus(Patient patient)
        {
            Patient = patient;
            RecentVolumMeasurements = new ShiftList<float>(50, 0f);
            RecentPressMeasurements = new ShiftList<float>(50, 0f);

            Peep = 0;
            TV = 0;
            IE = 0;
        }

    }
}
