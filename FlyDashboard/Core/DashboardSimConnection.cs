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

            // Setter
            simConnection.AddToDataDefinition(EUserData.Heading, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnection.RegisterDataDefineStruct<DoubleStruct>(EUserData.Heading);

            simConnection.AddToDataDefinition(EUserData.Altitude, "AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnection.RegisterDataDefineStruct<DoubleStruct>(EUserData.Altitude);

            // events
            simConnection.MapClientEventToSimEvent(EventsID.ToggleHeading, "AP_HDG_HOLD");
            simConnection.AddClientEventToNotificationGroup(NotificationGroups.GROUP0, EventsID.ToggleHeading, false);

            simConnection.RequestDataOnSimObject(EUserData.Dummy, EUserData.Dummy, (uint)SIMCONNECT_SIMOBJECT_TYPE.USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
        }

        protected override void OnRecvObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data) 
        {
            OnDataReceived?.Invoke(sender, new DashboardEventArgs { Info = (DashboardInfo) data.dwData[0] });

            //simConnection.SetNotificationGroupPriority(NOTIFICATION_GROUPS.GROUP0, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);
        }

        public void SetDataOnSim(EUserData userDataType, double inValue)
        {
            if(IsConnected)
            {
                DoubleStruct structValue = new DoubleStruct();
                structValue.value = inValue;

                simConnection.SetDataOnSimObject(userDataType, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, structValue);
            }
        }

        public void TriggerAPHeadingEvent()
        {
            if(IsConnected)
            {
                simConnection.TransmitClientEvent(0, EventsID.ToggleHeading, 1, NotificationGroups.GROUP0, SIMCONNECT_EVENT_FLAG.DEFAULT);
            }
        }

        public void SetDataOnSim(string variableID, string inValue)
        {
            throw new NotImplementedException();
        }
    }
}
