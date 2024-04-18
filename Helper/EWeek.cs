using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringServer.Helper {
	public enum EWeek {
		[Description("monday")] Monday = 1,
		[Description("tuesday")] Tuesday = 2,
		[Description("wednesday")] Wednesday = 3,
		[Description("thursday")] Thursday = 4,
		[Description("friday")] Friday = 5,
		[Description("saturday")] Saturday = 6,
		[Description("sunday")] Sunday = 7,
	}
}
