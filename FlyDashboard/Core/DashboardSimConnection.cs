using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyDashboard.Core
{
    public struct DashboardInfo
    {
        // Positions
        public double altitude;
        public double heading; // deg

        // Velocity (m/s)
        public double groundSpeed;
        public double airSpeed;
    }

    public class DashboardEventArgs : EventArgs
    {
        public DashboardInfo Info;
    }

    public class DashboardSimConnection : BaseSimConnection
    {
        public override string FunctionalityName => "Dashboard";
        public event EventHandler<DashboardEventArgs> OnDataReceived;

        protected override double PollingInterval => 0.1;

        protected override void SetupSimData()
        {
            // Positions
            simConnection.AddToDataDefinition(EUserData.Dummy, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnection.AddToDataDefinition(EUserData.Dummy, "PLANE HEADING DEGREES GYRO", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnection.AddToDataDefinition(EUserData.Dummy, "GROUND VELOCITY", "feet/second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnection.AddToDataDefinition(EUserData.Dummy, "AIRSPEED INDICATED", "feet/second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            //simConnection.AddToDataDefinition(UserData.Dummy, "VELOCITY WORLD Y", "feet/second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            //simConnection.AddToDataDefinition(UserData.Dummy, "VELOCITY WORLD Z", "feet/second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            simConnection.RegisterDataDefineStruct<DashboardInfo>(EUserData.Dummy);

            //simConnection.RequestDataOnSimObjectType(UserData.Position, UserData.Position, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            simConnection.RequestDataOnSimObject(EUserData.Dummy, EUserData.Dummy, (uint)SIMCONNECT_SIMOBJECT_TYPE.USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
        }

        protected override void OnRecvObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data) 
        {
            OnDataReceived?.Invoke(sender, new DashboardEventArgs { Info = (DashboardInfo) data.dwData[0] });
        }

        void SetDataOnSim(string variableID, double inValue)
        {
            if(IsConnected)
            {
                simConnection.SetDataOnSimObject(ESimData.Dummy, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, inValue);
            }
        }

        void SetDataOnSim(string variableID, string inValue)
        {
            throw new NotImplementedException();
        }
    }
}
