using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    class Message
    {
        public enum MessageType
        {
            VolumeUpdate,
            PressureUpdate,
            TestStatusUpdate,
            StartTestRequest,
            StopTestRequest,
            TestIndexRequest,
            TestIndexResponse,
            PeepSet
        }

        public MessageType Type;
        public Enums.Patient AffectedPatient;
        public object Data;
    }
}
