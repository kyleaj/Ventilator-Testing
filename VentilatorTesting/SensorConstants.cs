using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    public class SensorConstants
    {

        public static readonly float ACCEL_POLL_FREQ = 0.2F; // Hz

        public static readonly short ACCEL_ADDR_P_A = 0x53;
        public static readonly short ACCEL_ADDR_P_B = 0x1D;

        public static readonly byte ACCEL_REG_DEV_ID = 0x00;
        public static readonly byte ACCEL_REG_DATA_FORMAT = 0x31;
        public static readonly byte ACCEL_RANGE_2_G = 0b00;
        public static readonly byte ACCEL_REG_POWER_CTL = 0x2D;
        public static readonly float ACCEL_MG2G = 0.038f; // (9.81*2)/(2^9-1), as measurements are 10 bit 2's complement
        public static readonly byte ACCEL_X_REG = 0x32;
        public static readonly byte ACCEL_Y_REG = 0x34;
        public static readonly byte ACCEL_Z_REG = 0x36;

        public static readonly short PRESS_ADDR = 0xC0;
        public static readonly short PRESS_INT_P_A_PIN = 13; // Chosen randomly, can change
        public static readonly short PRESS_INT_P_B_PIN = 23; // Chose randomly, can change
    }
}
