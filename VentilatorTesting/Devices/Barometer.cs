using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VentilatorTesting.Enums;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace VentilatorTesting
{
    class Barometer : Sensor
    {

        #region CALIBRATION_VARIABLES
        ushort dig_T1;
        short dig_T2, dig_T3;
        ushort dig_P1;
        short dig_P2, dig_P3, dig_P4, dig_P5, dig_P6, dig_P7, dig_P8, dig_P9;
        #endregion

        public Barometer(DeviceInformation deviceInformation, Patient patient)
        {
            Debug.WriteLine("Creating Barometer");
            if (deviceInformation == null)
            {
                throw new ArgumentNullException("Device Information cannot be null!");
            }

            short address;
            if (patient == Patient.A)
            {
                address = SensorConstants.PRESS_ADDR_A;
            } else
            {
                address = SensorConstants.PRESS_ADDR_B;
            }

            try
            {
                sensor = I2cDevice.FromIdAsync(deviceInformation.Id,
                    new I2cConnectionSettings(
                        address)
                    { BusSpeed = I2cBusSpeed.StandardMode })
                        .AsTask().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not open barometer");
                Debug.WriteLine(e);
                throw e;
            }

            InitializeSensor();
        }

        private void InitializeSensor()
        {
            if (GetDeviceID() != SensorConstants.PRESS_ID)
            {
                throw new Exception("Doesn't look like the correct device is connected at the expected I2C address...");
            }

            // Read calibration data
            Debug.WriteLine("Reading Calibration data...");
            dig_T1 = (ushort)Read16bitRegister(0x88);
            dig_T2 = Read16bitRegister(0x8A);
            dig_T3 = Read16bitRegister(0x8C);
            dig_P1 = (ushort)Read16bitRegister(0x8E);
            dig_P2 = Read16bitRegister(0x90);
            dig_P3 = Read16bitRegister(0x92);
            dig_P4 = Read16bitRegister(0x94);
            dig_P5 = Read16bitRegister(0x96);
            dig_P6 = Read16bitRegister(0x98);
            dig_P7 = Read16bitRegister(0x9A);
            dig_P8 = Read16bitRegister(0x9C);
            dig_P9 = Read16bitRegister(0x9E);

            // Set up device configuration
            Debug.WriteLine("Setting device config...");
            WriteRegister(SensorConstants.PRESS_CONFIG_REG, SensorConstants.PRESS_CONFIG_VAL);
            WriteRegister(SensorConstants.PRESS_MODE_REG, SensorConstants.PRESS_MODE_VAL);

            Debug.WriteLine("Pressure sensor initialization complete!");
        }

        public float GetPressure()
        {
            int adc_T = Read24bitRegister(SensorConstants.PRESS_READ_TEMP_REG) >> 4;
            int adc_P = Read24bitRegister(SensorConstants.PRESS_READ_PRESS_REG) >> 4;

            int var1_t = ((((adc_T >> 3) - (((int)dig_T1) << 1))) * ((int)dig_T2)) >> 11;
            int var2_t = (((((adc_T >> 4) - ((int)dig_T1)) * ((adc_T >> 4) - ((int)dig_T1))) >> 12) * ((int)dig_T3)) >> 14;

            int t_fine = var1_t + var2_t;

            long var1 = ((long)t_fine) - 128000;
            long var2 = var1 * var1 * (long)dig_P6;
            var2 = var2 + ((var1 * (long)dig_P5) << 17);
            var2 = var2 + (((long)dig_P4) << 35);
            var1 = ((var1 * var1 * (long)dig_P3) >> 8) +
                   ((var1 * (long)dig_P2) << 12);
            var1 =
                (((((long)1) << 47) + var1)) * ((long)dig_P1) >> 33;

            if (var1 == 0)
            {
                return 0; // avoid exception caused by division by zero
            }

            long p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = (((long)dig_P9) * (p >> 13) * (p >> 13)) >> 25;
            var2 = (((long)dig_P8) * p) >> 19;

            p = ((p + var1 + var2) >> 8) + (((long)dig_P7) << 4);

            return p / 256F;

        }

        public override byte GetDeviceID()
        {
            return ReadRegister(SensorConstants.PRESS_ID_REG);
        }

        public override void Dispose()
        {
            // TODO: Tell barometer to sleep
            base.Dispose();
        }
    }
}
