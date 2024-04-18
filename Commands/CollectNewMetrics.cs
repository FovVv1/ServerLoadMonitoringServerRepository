using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringServer.Commands
{
     class CollectNewMetrics : ICommandServer
     {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try {
                var type = MetricType.ServerUtilization;

                switch (type)
                {
                    case MetricType.ServerUtilization:
                        ServerUtilization serverUtilization = new ServerUtilization();
                        serverUtilization.GetMMetric();
                        return JsonConvert.SerializeObject(new { json = JsonConvert.SerializeObject(serverUtilization), typee = MetricType.ServerUtilization});

                    case MetricType.DatabaseUtilization:
                        DatabaseUtilization databaseUtilization = new DatabaseUtilization();
                        databaseUtilization.GetMMetric(elConnectionClient);
                        return JsonConvert.SerializeObject(new { json = JsonConvert.SerializeObject(databaseUtilization), typee = MetricType.DatabaseUtilization });
                }

                return JsonConvert.SerializeObject(false);
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e, "An error occurred while serializing metrics.");
                return JsonConvert.SerializeObject(false);
            }

        }
     }
}
