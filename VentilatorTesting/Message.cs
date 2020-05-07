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
            TestResultRequest,
            TestResultResponse
        }

        public MessageType Type;
        public object Data;
    }
}
