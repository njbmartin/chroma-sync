﻿using Corale.Colore.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ultrabox.ChromaSync
{
    class CodemastersAPI
    {
        public static Thread _clientThread;
        private const int listenPort = 20777;
        internal static void RunServer()
        {
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            string received_data;
            byte[] receive_byte_array;
            try
            {
                //Chroma.Instance.SetAll(Color.White);
                while (!done)
                {
                    Debug.WriteLine("Waiting for broadcast");
                    // this is the line of code that receives the broadcase message.
                    // It calls the receive function from the object listener (class UdpClient)
                    // It passes to listener the end point groupEP.
                    // It puts the data from the broadcast message into the byte array
                    // named received_byte_array.
                    // I don't know why this uses the class UdpClient and IPEndPoint like this.
                    // Contrast this with the talker code. It does not pass by reference.
                    // Note that this is a synchronous or blocking call.
                    receive_byte_array = listener.Receive(ref groupEP);
                    var latestData = ConvertToPacket(receive_byte_array);
                    //Debug.WriteLine("data follows \n{0}\n\n", receive_byte_array[7]);
                    Debug.WriteLine(latestData);
                    /*
                    for(var i = 0; i < 10; i++)
                    {
                        if ((int)latestData.Gear == i && Chroma.Instance.Keyboard[1, 1+ i] != Color.Red)
                        {
                            Chroma.Instance.Keyboard[1, 1 + i]= Color.Red;
                        }
                        else if((int)latestData.Gear != i && Chroma.Instance.Keyboard[1, 1 + i] != Color.White)
                        {
                            Chroma.Instance.Keyboard[1, 1 + i]= Color.White;
                        }
                    }
                    */
                    var revs = Math.Floor(latestData.EngineRevs / latestData.NewField26 * 10);

                    for (var i = 1; i <= 10; i++)
                    {
                        if (revs >= i && Chroma.Instance.Keyboard[1, 1 + i] != Color.White)
                        {
                            Chroma.Instance.Keyboard[1,1 + i] = Color.White;
                        }
                        else if (revs < i && Chroma.Instance.Keyboard[1, 1 + i] != new Color(2,2,2))
                        {
                            Chroma.Instance.Keyboard[1,1 + i] = new Color(2, 2, 2);
                        }
                    }

                    //Chroma.Instance.Keyboard[0, 1 + (int)(latestData.EngineRevs / 100)] = Color.Red;
                    // Debug.WriteLine(receive_byte_array);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
            return;
        }

        public static TelemetryPacket ConvertToPacket(byte[] bytes)
        {
            // Marshal the byte array into the telemetryPacket structure
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stuff = (TelemetryPacket)Marshal.PtrToStructure(
                handle.AddrOfPinnedObject(), typeof(TelemetryPacket));
            handle.Free();
            return stuff;
        }
    }


    [Serializable]
    internal struct TelemetryPacket : ISerializable
    {
        public TelemetryPacket(SerializationInfo info, StreamingContext context)
        {
            Time = info.GetValue<float>("Time");
            LapTime = info.GetValue<float>("LapTime");
            LapDistance = info.GetValue<float>("LapDistance");
            Distance = info.GetValue<float>("Distance");
            Speed = info.GetValue<float>("Speed");
            Lap = info.GetValue<float>("Lap");
            X = info.GetValue<float>("X");
            Y = info.GetValue<float>("Y");
            Z = info.GetValue<float>("Z");
            WorldSpeedX = info.GetValue<float>("WorldSpeedX");
            WorldSpeedY = info.GetValue<float>("WorldSpeedY");
            WorldSpeedZ = info.GetValue<float>("WorldSpeedZ");
            XR = info.GetValue<float>("XR");
            Roll = info.GetValue<float>("Roll");
            ZR = info.GetValue<float>("ZR");
            XD = info.GetValue<float>("XD");
            Pitch = info.GetValue<float>("Pitch");
            ZD = info.GetValue<float>("ZD");
            SuspensionPositionRearLeft = info.GetValue<float>("SuspensionPositionRearLeft");
            SuspensionPositionRearRight = info.GetValue<float>("SuspensionPositionRearRight");
            SuspensionPositionFrontLeft = info.GetValue<float>("SuspensionPositionFrontLeft");
            SuspensionPositionFrontRight = info.GetValue<float>("SuspensionPositionFrontRight");
            SuspensionVelocityRearLeft = info.GetValue<float>("SuspensionVelocityRearLeft");
            SuspensionVelocityRearRight = info.GetValue<float>("SuspensionVelocityRearRight");
            SuspensionVelocityFrontLeft = info.GetValue<float>("SuspensionVelocityFrontLeft");
            SuspensionVelocityFrontRight = info.GetValue<float>("SuspensionVelocityFrontRight");
            WheelSpeedBackLeft = info.GetValue<float>("WheelSpeedBackLeft");
            WheelSpeedBackRight = info.GetValue<float>("WheelSpeedBackRight");
            WheelSpeedFrontLeft = info.GetValue<float>("WheelSpeedFrontLeft");
            WheelSpeedFrontRight = info.GetValue<float>("WheelSpeedFrontRight");
            Throttle = info.GetValue<float>("Throttle");
            Steer = info.GetValue<float>("Steer");
            Brake = info.GetValue<float>("Brake");
            Clutch = info.GetValue<float>("Clutch");
            Gear = info.GetValue<float>("Gear");
            LateralAcceleration = info.GetValue<float>("LateralAcceleration");
            LongitudinalAcceleration = info.GetValue<float>("LongitudinalAcceleration");
            EngineRevs = info.GetValue<float>("EngineRevs");

            NewField1 = info.GetValue<float>("NewField1");
            RacePosition = info.GetValue<float>("RacePosition");
            KersRemaining = info.GetValue<float>("KersRemaining");
            KersRecharge = info.GetValue<float>("KersRecharge");
            DrsStatus = info.GetValue<float>("DrsStatus");
            Difficulty = info.GetValue<float>("Difficulty");
            Assists = info.GetValue<float>("Assists");
            FuelRemaining = info.GetValue<float>("FuelRemaining");
            SessionType = info.GetValue<float>("SessionType");
            NewField10 = info.GetValue<float>("NewField10");
            Sector = info.GetValue<float>("Sector");
            TimeSector1 = info.GetValue<float>("TimeSector1");
            TimeSector2 = info.GetValue<float>("TimeSector2");
            BrakeTemperatureRearLeft = info.GetValue<float>("BrakeTemperatureRearLeft");
            BrakeTemperatureRearRight = info.GetValue<float>("BrakeTemperatureRearRight");
            BrakeTemperatureFrontLeft = info.GetValue<float>("BrakeTemperatureFrontLeft");
            BrakeTemperatureFrontRight = info.GetValue<float>("BrakeTemperatureFrontRight");
            NewField18 = info.GetValue<float>("NewField18");
            NewField19 = info.GetValue<float>("NewField19");
            NewField20 = info.GetValue<float>("NewField20");
            NewField21 = info.GetValue<float>("NewField21");
            CompletedLapsInRace = info.GetValue<float>("CompletedLapsInRace");
            TotalLapsInRace = info.GetValue<float>("TotalLapsInRace");
            TrackLength = info.GetValue<float>("TrackLength");
            PreviousLapTime = info.GetValue<float>("PreviousLapTime");
            NewField26 = info.GetValue<float>("NewField26");
            NewField27 = info.GetValue<float>("NewField27");
            NewField28 = info.GetValue<float>("NewField28");
        }

        public float Time;
        public float LapTime;
        public float LapDistance;
        public float Distance;
        [XmlIgnore]
        public float X;
        [XmlIgnore]
        public float Y;
        [XmlIgnore]
        public float Z;
        public float Speed;
        [XmlIgnore]
        public float WorldSpeedX;
        [XmlIgnore]
        public float WorldSpeedY;
        [XmlIgnore]
        public float WorldSpeedZ;
        [XmlIgnore]
        public float XR;
        [XmlIgnore]
        public float Roll;
        [XmlIgnore]
        public float ZR;
        [XmlIgnore]
        public float XD;
        [XmlIgnore]
        public float Pitch;
        [XmlIgnore]
        public float ZD;
        [XmlIgnore]
        public float SuspensionPositionRearLeft;
        [XmlIgnore]
        public float SuspensionPositionRearRight;
        [XmlIgnore]
        public float SuspensionPositionFrontLeft;
        [XmlIgnore]
        public float SuspensionPositionFrontRight;
        [XmlIgnore]
        public float SuspensionVelocityRearLeft;
        [XmlIgnore]
        public float SuspensionVelocityRearRight;
        [XmlIgnore]
        public float SuspensionVelocityFrontLeft;
        [XmlIgnore]
        public float SuspensionVelocityFrontRight;
        [XmlIgnore]
        public float WheelSpeedBackLeft;
        [XmlIgnore]
        public float WheelSpeedBackRight;
        [XmlIgnore]
        public float WheelSpeedFrontLeft;
        [XmlIgnore]
        public float WheelSpeedFrontRight;
        [XmlIgnore]
        public float Throttle;
        [XmlIgnore]
        public float Steer;
        [XmlIgnore]
        public float Brake;
        [XmlIgnore]
        public float Clutch;
        [XmlIgnore]
        public float Gear;
        [XmlIgnore]
        public float LateralAcceleration;
        [XmlIgnore]
        public float LongitudinalAcceleration;
        public float Lap;
        [XmlIgnore]
        public float EngineRevs;

        /* New Fields in Patch 12 */
        [XmlIgnore]
        public float NewField1;     // Always 1?
        [XmlIgnore]
        public float RacePosition;     // Position in race
        [XmlIgnore]
        public float KersRemaining;     // Kers Remaining
        [XmlIgnore]
        public float KersRecharge;     // Always 400000? 
        [XmlIgnore]
        public float DrsStatus;     // Drs Status
        [XmlIgnore]
        public float Difficulty;     // 2 = Medium or Easy, 1 = Hard, 0 = Expert
        [XmlIgnore]
        public float Assists;     // 0 = All assists are off.  1 = some assist is on.
        [XmlIgnore]
        public float FuelRemaining;      // Not sure if laps or Litres?
        [XmlIgnore]
        public float SessionType;   // 9.5 = race, 10 = time trail / time attack, 170 = quali, practice, championsmode
        [XmlIgnore]
        public float NewField10;
        [XmlIgnore]
        public float Sector;    // Sector (0, 1, 2)
        [XmlIgnore]
        public float TimeSector1;    // Time Intermediate 1
        [XmlIgnore]
        public float TimeSector2;    // Time Intermediate 2
        [XmlIgnore]
        public float BrakeTemperatureRearLeft;
        [XmlIgnore]
        public float BrakeTemperatureRearRight;
        [XmlIgnore]
        public float BrakeTemperatureFrontLeft;
        [XmlIgnore]
        public float BrakeTemperatureFrontRight;
        [XmlIgnore]
        public float NewField18;    // Always 0?
        [XmlIgnore]
        public float NewField19;    // Always 0?
        [XmlIgnore]
        public float NewField20;    // Always 0?
        [XmlIgnore]
        public float NewField21;    // Always 0?
        [XmlIgnore]
        public float CompletedLapsInRace;    // Number of laps Completed (in GP only)
        [XmlIgnore]
        public float TotalLapsInRace;    // Number of laps in GP (GP only)
        [XmlIgnore]
        public float TrackLength;    // Track Length
        [XmlIgnore]
        public float PreviousLapTime;    // Lap time of previous lap

        //  The next three fields are new for F1 2013

        [XmlIgnore]
        public float NewField26;    // Always 0?
        [XmlIgnore]
        public float NewField27;    // Always 0?
        [XmlIgnore]
        public float NewField28;    // Always 0?

        /* End new Fields */

        [XmlIgnore]
        public float SpeedInKmPerHour
        {
            get { return Speed * 3.60f; }
        }

        [XmlIgnore]
        public bool IsSittingInPits
        {
            get { return Math.Abs(LapTime - 0) < Constants.Epsilon && Math.Abs(Speed - 0) < Constants.Epsilon; }
        }

        [XmlIgnore]
        public bool IsInPitLane
        {
            get { return Math.Abs(LapTime - 0) < Constants.Epsilon; }
        }

        [XmlIgnore]
        public string SessionTypeName
        {
            get
            {
                if (Math.Abs(this.SessionType - 9.5f) < 0.0001f)
                    return "Race";
                if (Math.Abs(this.SessionType - 10f) < 0.0001f)
                    return "Time Trial";
                if (Math.Abs(this.SessionType - 170f) < 0.0001f)
                    return "Qualifying or Practice";
                return "Other";
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var fields = this.GetType().GetFields();
            foreach (var field in fields)
            {
                sb.AppendFormat("{0}({1}) : ", field.Name, field.GetValue(this));
            }

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
                sb.AppendFormat("{0}({1}) : ", prop.Name, prop.GetValue(this, null));

            return sb.ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Time", Time);
            info.AddValue("LapTime", LapTime);
            info.AddValue("LapDistance", LapDistance);
            info.AddValue("Distance", Distance);
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
            info.AddValue("Speed", Speed);
            info.AddValue("WorldSpeedX", WorldSpeedX);
            info.AddValue("WorldSpeedY", WorldSpeedY);
            info.AddValue("WorldSpeedZ", WorldSpeedZ);
            info.AddValue("XR", XR);
            info.AddValue("Roll", Roll);
            info.AddValue("ZR", ZR);
            info.AddValue("XD", XD);
            info.AddValue("Pitch", Pitch);
            info.AddValue("ZD", ZD);
            info.AddValue("SuspensionPositionRearLeft", SuspensionPositionRearLeft);
            info.AddValue("SuspensionPositionRearRight", SuspensionPositionRearRight);
            info.AddValue("SuspensionPositionFrontLeft", SuspensionPositionFrontLeft);
            info.AddValue("SuspensionPositionFrontRight", SuspensionPositionFrontRight);
            info.AddValue("SuspensionVelocityRearLeft", SuspensionVelocityRearLeft);
            info.AddValue("SuspensionVelocityRearRight", SuspensionVelocityRearRight);
            info.AddValue("SuspensionVelocityFrontLeft", SuspensionVelocityFrontLeft);
            info.AddValue("SuspensionVelocityFrontRight", SuspensionVelocityFrontRight);
            info.AddValue("WheelSpeedBackLeft", WheelSpeedBackLeft);
            info.AddValue("WheelSpeedBackRight", WheelSpeedBackRight);
            info.AddValue("WheelSpeedFrontLeft", WheelSpeedFrontLeft);
            info.AddValue("WheelSpeedFrontRight", WheelSpeedFrontRight);
            info.AddValue("Throttle", Throttle);
            info.AddValue("Steer", Steer);
            info.AddValue("Brake", Brake);
            info.AddValue("Clutch", Clutch);
            info.AddValue("Gear", Gear);
            info.AddValue("LateralAcceleration", LateralAcceleration);
            info.AddValue("LongitudinalAcceleration", LongitudinalAcceleration);
            info.AddValue("Lap", Lap);
            info.AddValue("EngineRevs", EngineRevs);

            info.AddValue("NewField1", NewField1);
            info.AddValue("RacePosition", RacePosition);
            info.AddValue("KersRemaining", KersRemaining);
            info.AddValue("KersRecharge", KersRecharge);
            info.AddValue("DrsStatus", DrsStatus);
            info.AddValue("Difficulty", Difficulty);
            info.AddValue("Assists", Assists);
            info.AddValue("FuelRemaining", FuelRemaining);
            info.AddValue("SessionType", SessionType);
            info.AddValue("NewField10", NewField10);
            info.AddValue("Sector", Sector);
            info.AddValue("TimeSector1", TimeSector1);
            info.AddValue("TimeSector2", TimeSector2);
            info.AddValue("BrakeTemperatureRearLeft", BrakeTemperatureRearLeft);
            info.AddValue("BrakeTemperatureRearRight", BrakeTemperatureRearRight);
            info.AddValue("BrakeTemperatureFrontLeft", BrakeTemperatureFrontLeft);
            info.AddValue("BrakeTemperatureFrontRight", BrakeTemperatureFrontRight);
            info.AddValue("NewField18", NewField18);
            info.AddValue("NewField19", NewField19);
            info.AddValue("NewField20", NewField20);
            info.AddValue("NewField21", NewField21);
            info.AddValue("CompletedLapsInRace", CompletedLapsInRace);
            info.AddValue("TotalLapsInRace", TotalLapsInRace);
            info.AddValue("TrackLength", TrackLength);
            info.AddValue("PreviousLapTime", PreviousLapTime);
            info.AddValue("NewField26", NewField26);
            info.AddValue("NewField27", NewField27);
            info.AddValue("NewField28", NewField28);
        }
    }

    public static class Extensions
    {
        public static string AsTimeString(this float timeInSeconds, bool hideIfZero = false)
        {
            if (timeInSeconds <= 0)
            {
                if (hideIfZero)
                    return "";
                return "--:--.---";
            }

            var ts = TimeSpan.FromSeconds((double)timeInSeconds);
            return ts.ToString(@"m\:ss\.fff");
        }

        public static string AsGapString(this float gapInSeconds, bool hideIfZero = false, bool excludeSign = false)
        {
            if (Math.Abs(gapInSeconds - 0f) < 0.0001)
            {
                if (hideIfZero)
                    return "";
                return "-.---";
            }

            var gs = string.Format("{0:f3}", Math.Abs(gapInSeconds));
            if (excludeSign)
                return gs;
            return (gapInSeconds < 0 ? "-" : "+") + gs;
        }

        public static T GetValue<T>(this SerializationInfo info, string fieldName)
        {
            return (T)info.GetValue(fieldName, typeof(T));
        }
    }

    public static class Constants
    {
        public const float Epsilon = 0.00001f;
    }


}

