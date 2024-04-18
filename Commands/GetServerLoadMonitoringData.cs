using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels.Enums;

namespace ServerLoadMonitoringServer.Commands
{
    public class GetServerLoadMonitoringData : ICommandServer
    {
        //команда на получение настроек плагина по ключу
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                //получить ключ настроек

                var description = new { ReportKey = ESettingsKeys.Settings };
                var parameters = JsonConvert.DeserializeAnonymousType(elMessageServer.Data, description);

                using (var db =
                        new SqlConnection(elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString))
                {
                    db.Open();
                    //create key for loading current
                    var reportKey = $"{parameters.ReportKey}";

                    //get report from table or return clean object
                    var settings = db.Query<string>("ServerLoadMonitoring_GetPluginSetting", new { PluginName = elMessageServer.PluginName, Setting = reportKey },
                        commandType: CommandType.StoredProcedure).FirstOrDefault();





                    return JsonConvert.SerializeObject(settings);
                }
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                //При возникновении ошибки возвращаем сериализованый объект Exception
                return JsonConvert.SerializeObject(e);
            }
        }
    }
}
