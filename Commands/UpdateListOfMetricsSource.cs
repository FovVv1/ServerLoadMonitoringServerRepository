using Dapper;
using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerLoadMonitoringServer;
using System.Data;
using System.Collections.ObjectModel;

namespace ServerLoadMonitoringServer.Commands
{
    class UpdateListOfMetricsSource : ICommandServer
    {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {

            try
            {
                List<MetricsControlConfig> Metrics = new List<MetricsControlConfig>();
                ServerLoadMonitoring.MetricsSources = new List<IMetric>();
                using (SqlConnection db = new SqlConnection(elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString))
                {
                    db.Open();

                    var resultList = db.Query<dynamic>("ServerLoadMonitoring_GetMetricsSources", commandType: CommandType.StoredProcedure);

                    foreach (var result in resultList)
                    {


                        // Преобразование строки в соответствующее значение перечисления MetricType
                        if (Enum.TryParse(result.Type, out MetricType type))
                        {
                            Metrics.Add(new MetricsControlConfig(result.Ip, type, result.CheckInterval));
                            switch (type)
                            {
                                case MetricType.DatabaseUtilization:
                                    var existingCollection1 = ServerLoadMonitoring.DataBaseMetricsCollectors.FirstOrDefault(m => (m as DatabaseMetricCollector)?.Ip == result.Ip);
                                    if (existingCollection1 == null)
                                    {
                                        DatabaseUtilization newDatabaseUtilizationMetric = new DatabaseUtilization
                                        {
                                            Ip = result.Ip,
                                            Type = type,
                                            CheckInterval = result.CheckInterval
                                            // Дополнительные свойства 
                                        };
                                        newDatabaseUtilizationMetric.GetMetric(elConnectionClient, elMessageServer);
                                        newDatabaseUtilizationMetric.CollectionNumber = ServerLoadMonitoring.DataBaseMetricsCollectors.Count;
                                        ServerLoadMonitoring.MetricsSources.Add((DatabaseUtilization)newDatabaseUtilizationMetric.Clone());
                                        ServerLoadMonitoring.DataBaseMetricsCollectors.Add(new DatabaseMetricCollector(newDatabaseUtilizationMetric.Ip, new ObservableCollection<DatabaseUtilization> { newDatabaseUtilizationMetric }));

                                    }
                                    else
                                    {
                                        LogManager.GetCurrentClassLogger().Error(result.Ip.ToString() + " используется более одного раза в таблице MetricSource с одним и тем же типом метрики.");
                                    }
                                    break;
                                case MetricType.ServerUtilization:
                                    var existingCollection2 = ServerLoadMonitoring.ServerMetricsCollectors.FirstOrDefault(m => (m as ServerMetricCollector)?.Ip == result.Ip);
                                    if (existingCollection2 == null)
                                    {
                                        ServerUtilization newServerUtilizationMetric = new ServerUtilization
                                        {
                                            Ip = result.Ip,
                                            Type = type,
                                            CheckInterval = result.CheckInterval
                                            // Дополнительные свойства 
                                        };
                                        newServerUtilizationMetric.GetMetric(elConnectionClient, elMessageServer);
                                        newServerUtilizationMetric.CollectionNumber = ServerLoadMonitoring.ServerMetricsCollectors.Count;
                                        ServerLoadMonitoring.MetricsSources.Add((ServerUtilization)newServerUtilizationMetric.Clone());
                                        ServerLoadMonitoring.ServerMetricsCollectors.Add(new ServerMetricCollector(newServerUtilizationMetric.Ip, new ObservableCollection<ServerUtilization> { newServerUtilizationMetric }));

                                    }
                                    else
                                    {
                                        LogManager.GetCurrentClassLogger().Error(result.Ip.ToString() + " используется более одного раза в таблице MetricSource с одним и тем же типом метрики.");
                                    }
                                    break;

                                case MetricType.StorageUtilization:
                                    var existingCollection3 = ServerLoadMonitoring.StorageMetricsCollectors.FirstOrDefault(m => (m as StorageMetricCollector)?.Ip == result.Ip);
                                    if (existingCollection3 == null)
                                    {
                                        StorageUtilization newStorageUtilizationMetric = new StorageUtilization
                                        {
                                            Ip = result.Ip,
                                            Type = type,
                                            CheckInterval = result.CheckInterval
                                            // Дополнительные свойства 
                                        };

                                        newStorageUtilizationMetric.GetMetric(elConnectionClient, elMessageServer);
                                        newStorageUtilizationMetric.CollectionNumber = ServerLoadMonitoring.StorageMetricsCollectors.Count;
                                        ServerLoadMonitoring.MetricsSources.Add((StorageUtilization)newStorageUtilizationMetric.Clone());
                                        ServerLoadMonitoring.StorageMetricsCollectors.Add(new StorageMetricCollector(newStorageUtilizationMetric.Ip, new ObservableCollection<StorageUtilization> { newStorageUtilizationMetric }));

                                    }
                                    else
                                    {
                                        LogManager.GetCurrentClassLogger().Error(result.Ip.ToString() + " используется более одного раза в таблице MetricSource с одним и тем же типом метрики.");
                                    }
                                    break;

                            }
                            //if (metric.Type == MetricType.DatabaseUtilization)
                            //{
                            //    ServerLoadMonitoring.MetricsSources.Add(new DatabaseUtilization(metric.Ip, metric.Type, metric.CheckInterval));
                            //}
                            //else if (metric.Type == MetricType.ServerUtilization)
                            //{
                            //    ServerLoadMonitoring.MetricsSources.Add(new ServerUtilization(metric.Ip, metric.Type, metric.CheckInterval));
                            //}
                            //else
                            //{
                            //    ServerLoadMonitoring.MetricsSources.Add(new StorageUtilization(metric.Ip, metric.Type, metric.CheckInterval));
                            //}
                        }
                    }
                    return JsonConvert.SerializeObject(new { MetricsSources = Metrics });
                }
            }

            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                return JsonConvert.SerializeObject(new { result = false }); ;
            }
        }
    }
}
