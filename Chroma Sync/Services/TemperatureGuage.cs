using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync.Services
{
    class Temperature
    {
        public double CurrentValue { get; set; }
        public string InstanceName { get; set; }
        public static List<Temperature> Temperatures
        {
            get
            {
                List<Temperature> result = new List<Temperature>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                try
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                        temp = (temp - 2732) / 10.0;
                        result.Add(new Temperature { CurrentValue = temp, InstanceName = obj["InstanceName"].ToString() });
                    }
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return result;

            }
        }
    }

    public class Win32_TemperatureProbe
    {
        public int? Accuracy;
        public ushort? Availability;
        public string Caption;
        public uint? ConfigManagerErrorCode;
        public bool? ConfigManagerUserConfig;
        public string CreationClassName;
        public int? CurrentReading;
        public string Description;
        public string DeviceID;
        public bool? ErrorCleared;
        public string ErrorDescription;
        public DateTime? InstallDate;
        public bool? IsLinear;
        public uint? LastErrorCode;
        public int? LowerThresholdCritical;
        public int? LowerThresholdFatal;
        public int? LowerThresholdNonCritical;
        public int? MaxReadable;
        public int? MinReadable;
        public string Name;
        public int? NominalReading;
        public int? NormalMax;
        public int? NormalMin;
        public string PNPDeviceID;
        public ushort[] PowerManagementCapabilities;
        public bool? PowerManagementSupported;
        public uint? Resolution;
        public string Status;
        public ushort? StatusInfo;
        public string SystemCreationClassName;
        public string SystemName;
        public int? Tolerance;
        public int? UpperThresholdCritical;
        public int? UpperThresholdFatal;
        public int? UpperThresholdNonCritical;
    }
}
