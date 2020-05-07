using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    public class SensorConstants
    {
        public static readonly float ACCEL_POLL_FREQ = 10F; // Hz

        public static readonly short ACCEL_ADDR_P_A = 0x53;
        public static readonly short ACCEL_ADDR_P_B = 0x1D;

        public static readonly byte ACCEL_REG_DEV_ID = 0x00;
        public static readonly byte ACCEL_REG_DATA_FORMAT = 0x31;
        public static readonly byte ACCEL_RANGE = 0x08;
        public static readonly byte ACCEL_REG_POWER_CTL = 0x2D;
        public static readonly byte ACCEL_X_REG = 0x32;
        public static readonly byte ACCEL_Y_REG = 0x34;
        public static readonly byte ACCEL_Z_REG = 0x36;

#if MAP_PRESS_SENS
        public static readonly short PRESS_ADDR = 0x60;
        public static readonly byte PRESS_ID_REG = 0x0C;
        public static readonly byte PRESS_ID = 0xC4;
        public static readonly short PRESS_INT_P_A_PIN = 13; // Chosen randomly, can change
        public static readonly short PRESS_INT_P_B_PIN = 23; // Chose randomly, can change
        public static readonly byte PRESS_MODE_REG = 0x26;
        public static readonly byte PRESS_MODE_VAL = 0x38; // Oversampling on, in barometer mode, measurement off
        public static readonly byte PRESS_EVENT_SET_REG = 0x13;
        public static readonly byte PRESS_EVENT_SET_VAL = 0x06;
        public static readonly byte PRESS_MODE_MEAS_VAL = 0x39; // Oversampling on, in barometer mode, measurement on
#else

        public static readonly short PRESS_ADDR_A = 0x76;
        public static readonly short PRESS_ADDR_B = 0x77;
        public static readonly byte PRESS_ID_REG = 0xD0;
        public static readonly byte PRESS_ID = 0x58;
        public static readonly byte PRESS_MODE_REG = 0xF4;
        public static readonly byte PRESS_MODE_VAL = 0x2b; // Oversampling on x8, temp on, press on, normal mode
        public static readonly byte PRESS_CONFIG_REG = 0xF5;
        public static readonly byte PRESS_CONFIG_VAL = 0x05; // Fastest sampling, limited iir, 3 wire spi
        public static readonly byte PRESS_READ_TEMP_REG = 0xF7;
        public static readonly byte PRESS_READ_PRESS_REG = 0xFA;
#endif
    }
}
