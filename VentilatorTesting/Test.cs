using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            VolumeData = new FlushableList<float>($"{Name}_{Created}_VolumeData.csv"); // TODO: Add in local path
            PressureData = new FlushableList<bool>($"{Name}_{Created}_PressureData.csv");
        }
    }
}
