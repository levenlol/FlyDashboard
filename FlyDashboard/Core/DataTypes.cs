using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlyDashboard.Core
{
    public enum EUserData
    {
        Dummy,
        Heading
    }

    // String properties must be packed inside of a struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct StringWrapper
    {
        // this is how you declare a fixed size string
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String sValue;

        // other definitions can be added to this struct
        // ...
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct AltitudeStruct
    {
        public double altitude;
    };

    enum EventsID
    {
        ToggleHeading
    }

    enum NotificationGroups
    {
        GROUP0 = 20
    }
}
