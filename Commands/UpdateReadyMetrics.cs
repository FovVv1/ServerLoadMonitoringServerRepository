using Dapper;
using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace ServerLoadMonitoringServer.Commands
{
    // временное ноухао

    class UpdateReadyMetrics : ICommandServer
    {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                ParallelLoopResult result = Parallel.ForEach(ServerLoadMonitoring.MetricsSources, (obj) => {
                    var source = (IMetric)obj.Clone();
                    source.Placement += 1;
                    obj.Placement += 1;
                    //var source = (IMetric)ServerLoadMonitoring.MetricsSources[i].Clone(); 
                    // Проверка последнего обновления метрика
                    if (source.LastCheckTime + source.CheckInterval < DateTime.Now.Ticks)
                    {
                        // Вызов метода GetMetric для текущего источника
                        source.RefreshingData = DateTime.Now;

                        source.GetMetric(elConnectionClient, elMessageServer);


                        // Обновление времени последней проверки
                        source.LastCheckTime = DateTime.Now.Ticks;
                        // Получение IP-адреса текущего источника
                        string ip = source.Ip;



                        switch (source.Type)
                        {
                            case MetricType.ServerUtilization:
                                    AddOrUpdateMetric(GetElementByIndex(ServerLoadMonitoring.ServerMetricsCollectors,source.CollectionNumber).MetricCollection, ip, (ServerUtilization)source.Clone());
                                break;
                            case MetricType.DatabaseUtilization:
                                    AddOrUpdateMetric(GetElementByIndex(ServerLoadMonitoring.DataBaseMetricsCollectors,source.CollectionNumber).MetricCollection, ip, (DatabaseUtilization)source.Clone());
                                break;
                            case MetricType.StorageUtilization:
                                    AddOrUpdateMetric(GetElementByIndex(ServerLoadMonitoring.StorageMetricsCollectors,source.CollectionNumber).MetricCollection, ip, (StorageUtilization)source.Clone());
                                break;
                        }
                    }
                });
               
                return JsonConvert.SerializeObject(true);
            }
            catch (Exception e)
            {

                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                return JsonConvert.SerializeObject(false);
            }
        }
        private static T GetElementByIndex<T>(BlockingCollection<T> collection, int index)
        {
            // Создаем массив элементов, чтобы получить доступ к элементам по индексу
            T[] elements = collection.ToArray();

            // Проверяем, находится ли индекс в пределах допустимого диапазона
            if (index >= 0 && index < elements.Length)
            {
                // Возвращаем элемент по указанному индексу
                return elements[index];
            }
            else
            {
                // Если индекс находится за пределами диапазона, выбрасываем исключение
                throw new IndexOutOfRangeException("Индекс находится за пределами диапазона коллекции.");
            }
        }
        private static void AddOrUpdateMetric<T>(ObservableCollection<T> metricCollection, string ip, T metric) where T : IMetric, ICloneable
        {
            try
            {

                if (metricCollection.Count > 60)
                {
                    metricCollection.Remove(metricCollection.First());
                }
                metricCollection.Add(metric);
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
            }
        }
    }
}
