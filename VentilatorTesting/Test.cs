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
        public FlushableList<float> VolumeData;
        public FlushableList<bool> PressureData;
        public bool Running;

        public Test(string name)
        {
            Name = name;
            Created = DateTime.Now;
            Running = true;
            string localPath = ApplicationData.Current.LocalFolder.Path;
            VolumeData = new FlushableList<float>(Path.Combine(localPath, $"{Name}_{Created}_VolumeData.csv"));
            PressureData = new FlushableList<bool>(Path.Combine(localPath, $"{Name}_{Created}_PressureData.csv"));
        }
    }
}
