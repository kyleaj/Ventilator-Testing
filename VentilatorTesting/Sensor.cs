using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace VentilatorTesting
{
    abstract class Sensor : IDisposable
    {
        protected I2cDevice sensor;

        // Read 1 byte from register
        protected byte ReadRegister(byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[1];
            sensor.Read(buffer);
            return buffer[0];
        }

        protected short Read16bitRegister(byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[2];
            sensor.Read(buffer);
            //Debug.WriteLine(String.Join(", ", buffer));
            return (short)(buffer[0] | (buffer[1] << 8));
        }

        protected int Read24bitRegister(byte register)
        {
            // Simultaneous reads make pressure sensor slow, for some reason...
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[3];
            sensor.Read(buffer);
            //Debug.WriteLine(String.Join(", ", buffer));
            return (short)((buffer[0] << 16) | (buffer[1] << 8) | buffer[0]);
        }

        protected void WriteRegister(byte register, byte data)
        {
            sensor.Write(new byte[] { register, data });
        }

        public abstract byte GetDeviceID();

        public virtual void Dispose()
        {
            if (sensor != null)
            {
                sensor.Dispose();
            }
            sensor = null;
        }
    }
}
