using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace VentilatorTesting
{
    abstract class Sensor
    {
        // Read 1 byte from register
        protected static byte ReadRegister(I2cDevice sensor, byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[1];
            sensor.Read(buffer);
            return buffer[0];
        }

        protected static short Read16bitRegister(I2cDevice sensor, byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[2];
            sensor.Read(buffer);
            //Debug.WriteLine(String.Join(", ", buffer));
            return (short)(buffer[0] | (buffer[1] << 8));
        }

        protected static int Read32bitRegister(I2cDevice sensor, byte register)
        {
            // Simultaneous reads make pressure sensor slow, for some reason...
            byte msb = ReadRegister(sensor, register);
            byte csb = ReadRegister(sensor, (byte)(register+1));
            byte lsb = ReadRegister(sensor, (byte)(register+2));
            int result = (msb << 8) | csb;
            return (result << 8) | lsb;
        }

        protected static void WriteRegister(I2cDevice sensor, byte register, byte data)
        {
            sensor.Write(new byte[] { register, data });
        }

    }
}
