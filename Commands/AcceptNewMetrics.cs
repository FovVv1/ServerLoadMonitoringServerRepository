using ElMessage;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels.Enums;
using ServerLoadMonitoringDataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace ServerLoadMonitoringServer.Commands
{
    class AcceptNewMetrics
    {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                try
                {
                    var data = "";
                    var definition = new { json = "", type = "" };
                    var tmp = JsonConvert.DeserializeAnonymousType(data, definition);
                    if (tmp.type == "ServerUtilization")
                    {
                        var newMetric= JsonConvert.DeserializeObject<ServerUtilization>(tmp.json);
                        AddMetricToCollection(newMetric);
                    }
                    else if (tmp.type == "DatabaseUtilization")
                    {
                        var newMetric = JsonConvert.DeserializeObject<DatabaseUtilization>(tmp.json);
                        AddMetricToCollection(newMetric);
                    }
                    else
                    {
                        
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));

                }



                return JsonConvert.SerializeObject(false);
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e, "An error occurred while serializing metrics.");
                return JsonConvert.SerializeObject(false);
            }

        }
        private static void AddMetricToCollection(IMetric metric) 
        {
            try
            {
                
                switch (metric.Type)
                {
                    case MetricType.ServerUtilization:
                        var targetCollector = ServerLoadMonitoring.ServerMetricsCollectors.FirstOrDefault(c => c.Ip == metric.Ip);
                        if (targetCollector.MetricCollection.Count > 60)
                        {
                            targetCollector.MetricCollection.Remove(targetCollector.MetricCollection.First());
                        }
                        targetCollector.MetricCollection.Add((ServerUtilization)metric);
                        break;
                    case MetricType.DatabaseUtilization:
                        var targetCollector2 = ServerLoadMonitoring.DataBaseMetricsCollectors.FirstOrDefault(c => c.Ip == metric.Ip);
                        if (targetCollector2.MetricCollection.Count > 60)
                        {
                            targetCollector2.MetricCollection.Remove(targetCollector2.MetricCollection.First());
                        }
                        targetCollector2.MetricCollection.Add((DatabaseUtilization)metric);
                        break;
                }
                
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
            }
        }
    }
}
