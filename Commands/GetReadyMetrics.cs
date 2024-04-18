using Dapper;
using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringServer.Commands
{
    class GetReadyMetrics : ICommandServer
    {

        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                var LastReadedMetricPlacement = new int();
                var description = new { LastReadedMetricPlacement = new int(), type = new MetricType(), Ip = "" };
                var parameters = JsonConvert.DeserializeAnonymousType(elMessageServer.Data, description);

                //var connections = elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections.Count;
                //ElMessageServer server = new ElMessageServer("ServerLoadMonitoring", "UpdateListOfMetricsSource", "");
                
                //var connections2 = elConnectionClient.ServerControlManager.PluginControl(server,elConnectionClient);
                
                LastReadedMetricPlacement = parameters.LastReadedMetricPlacement;
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto // или TypeNameHandling.All
                };
                switch (parameters.type)
                {
                    case MetricType.ServerUtilization:
                        var targetCollector = ServerLoadMonitoring.ServerMetricsCollectors.FirstOrDefault(c => c.Ip == parameters.Ip);
                        if (targetCollector != null)
                        {
                            //Создаем новый сборщик для метриков, что бы отправить только нужные метрики
                            ServerMetricCollector ClearMetricsCollector = (ServerMetricCollector)targetCollector.Clone();
                            ClearMetricsCollector.MetricCollection = new ObservableCollection<ServerUtilization>();
                            LastReadedMetricPlacement = parameters.LastReadedMetricPlacement;
                            foreach (var Metric in targetCollector.MetricCollection)
                            {
                                if (Metric.Placement > LastReadedMetricPlacement)
                                    ClearMetricsCollector.MetricCollection.Add(Metric);
                            }

                            return JsonConvert.SerializeObject(new { json = JsonConvert.SerializeObject(ClearMetricsCollector.MetricCollection), type = parameters.type.ToString() });
                        }
                        else 
                        {
                            return JsonConvert.SerializeObject(false);
                        }
                    case MetricType.DatabaseUtilization:
                        var targetCollector2 = ServerLoadMonitoring.DataBaseMetricsCollectors.FirstOrDefault(c => c.Ip == parameters.Ip);
                        if (targetCollector2 != null)
                        {
                            //Создаем новый сборщик для метриков, что бы отправить только нужные метрики
                            DatabaseMetricCollector ClearMetricsCollector2 = (DatabaseMetricCollector)targetCollector2.Clone();
                            ClearMetricsCollector2.MetricCollection = new ObservableCollection<DatabaseUtilization>();
                            LastReadedMetricPlacement = parameters.LastReadedMetricPlacement;
                            foreach (var Metric in targetCollector2.MetricCollection)
                            {
                                if (Metric.Placement > LastReadedMetricPlacement)
                                    ClearMetricsCollector2.MetricCollection.Add(Metric);
                            }
                            return JsonConvert.SerializeObject(new { json = JsonConvert.SerializeObject(ClearMetricsCollector2.MetricCollection), type = parameters.type.ToString() });
                        }
                        else { return JsonConvert.SerializeObject(false);}
                    case MetricType.StorageUtilization:
                        var targetCollector3 = ServerLoadMonitoring.StorageMetricsCollectors.FirstOrDefault(c => c.Ip == parameters.Ip);
                        if (targetCollector3 != null)
                        {
                            //Создаем новый сборщик для метриков, что бы отправить только нужные метрики
                            StorageMetricCollector ClearMetricsCollector3 = (StorageMetricCollector)targetCollector3.Clone();
                            ClearMetricsCollector3.MetricCollection = new ObservableCollection<StorageUtilization>();
                            LastReadedMetricPlacement = parameters.LastReadedMetricPlacement;
                            foreach (var Metric in targetCollector3.MetricCollection)
                            {
                                if (Metric.Placement > LastReadedMetricPlacement)
                                    ClearMetricsCollector3.MetricCollection.Add(Metric);
                            }
                            return JsonConvert.SerializeObject(new { json = JsonConvert.SerializeObject(ClearMetricsCollector3.MetricCollection), type = parameters.type.ToString() });
                        }
                        else { return JsonConvert.SerializeObject(false);}
                }
                return JsonConvert.SerializeObject(false);

            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e, "An error occurred while serializing metrics.");
                return JsonConvert.SerializeObject(false);
            }

        }
        public Dictionary<string, int> setMetricsPlacmentForStart()
        {
            try
            {
                var LastReadedMetricPlacement = new Dictionary<string, int>();
                foreach (var MetricCollector in ServerLoadMonitoring.ServerMetricsCollectors)
                {
                    LastReadedMetricPlacement.Add(MetricCollector.MetricCollection.Last().Type + MetricCollector.Ip, -1);
                }
                foreach (var MetricCollector in ServerLoadMonitoring.DataBaseMetricsCollectors)
                {
                    LastReadedMetricPlacement.Add(MetricCollector.MetricCollection.Last().Type + MetricCollector.Ip, -1);
                }
                foreach (var MetricCollector in ServerLoadMonitoring.StorageMetricsCollectors)
                {
                    LastReadedMetricPlacement.Add(MetricCollector.MetricCollection.Last().Type + MetricCollector.Ip, -1);
                }
                return LastReadedMetricPlacement;
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e, "An error occurred while serializing metrics.");
                return new Dictionary<string, int>();
            }
        }
    }
}
