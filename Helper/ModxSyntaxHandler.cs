using System;

namespace ServerLoadMonitoringServer.Helper {
	public class ModxSyntaxHandler {
		public static string ConvertModxKey(string property) {
			//Обработка замены наименования дня
			property = property.Replace("[[+day]]", EnumConverter.GetDescription((EWeek)DateTime.Now.DayOfWeek));

			return property;
		}
	}
}
