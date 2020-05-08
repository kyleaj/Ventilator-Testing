using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VentilatorTesting
{
    public class Test
    {
        public enum TestStatus
        {
            Running,
            Stopped,
            Complete,
            Error
        }


        public string Name;
        DateTime Created;
        public FlushableList<float> VolumeDataP1;
        public FlushableList<float> PressureDataP1;
        public FlushableList<float> VolumeDataP2;
        public FlushableList<float> PressureDataP2;
        public bool Running;

        public Test(string name)
        {
            Name = name;
            Created = DateTime.Now;
            Running = true;
            string localPath = ApplicationData.Current.LocalFolder.Path;
            string date = String.Format("{0:MM_dd_yyyy}", Created);
            VolumeDataP1 = new FlushableList<float>(Path.Combine(localPath, $"{Name}_{date}_VolumeData_P1.csv"));
            PressureDataP1 = new FlushableList<float>(Path.Combine(localPath, $"{Name}_{date}_PressureData_P1.csv"));
            VolumeDataP2 = new FlushableList<float>(Path.Combine(localPath, $"{Name}_{date}_VolumeData_P2.csv"));
            PressureDataP2 = new FlushableList<float>(Path.Combine(localPath, $"{Name}_{date}_PressureData_P2.csv"));
        }
    }
}
