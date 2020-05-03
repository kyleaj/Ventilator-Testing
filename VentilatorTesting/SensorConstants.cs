using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    public class SensorConstants
    {
        public static readonly short ACCEL_ADDR_P_A = 0x53;
        public static readonly short ACCEL_ADDR_P_B = 0x1D;

        public static readonly byte ACCEL_REG_DEV_ID = 0x00;

        public static readonly short PRESS_ADDR = 0xC0;
        public static readonly short PRESS_INT_P_A_PIN = 13; // Chosen randomly, can change
        public static readonly short PRESS_INT_P_B_PIN = 23; // Chose randomly, can change
    }
}
