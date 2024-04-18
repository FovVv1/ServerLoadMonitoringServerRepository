using System;
using ElMessage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElMessage.Interface;
using NLog;
using ServerLoadMonitoringDataModels;
using ServerLoadMonitoringServer.Commands;
using Dapper;
using System.Data;
using ServerLoadMonitoringDataModels.Enums;
using System.Windows.Input;
using System.Threading;

namespace ServerLoadMonitoringServer
{
    public class ServerLoadMonitoring : IPluginServer
    {
        public string Name { get; set; }
        public string Dll { get; set; }
        public long ServerVersion { get; set; }
        public long OldServerVersion { get; set; }

        public static List<IMetric> MetricsSources { get; set; }

        public static Enum ServerUtilizationMetricsEnum;

        public static BlockingCollection<ServerMetricCollector> ServerMetricsCollectors = new BlockingCollection<ServerMetricCollector>();
        public static BlockingCollection<DatabaseMetricCollector> DataBaseMetricsCollectors = new BlockingCollection<DatabaseMetricCollector>();
        public static BlockingCollection<StorageMetricCollector> StorageMetricsCollectors = new BlockingCollection<StorageMetricCollector>();

        public ElServerControlManager ServerControlManager { get; }
        public ConcurrentDictionary<string, ICommandServer> AllCommands { get; set; }

        public ServerLoadMonitoring(ElServerControlManager serverControlManager)
        {
            ServerControlManager = serverControlManager;
            AllCommands = new ConcurrentDictionary<string, ICommandServer>();
            //Регистрация команд для расширения
            AllCommands.TryAdd("GetServerLoadMonitoringData", new GetServerLoadMonitoringData());
            AllCommands.TryAdd("RefreshData", new RefreshData());
            AllCommands.TryAdd("UpdateListOfMetricsSource", new UpdateListOfMetricsSource());
            AllCommands.TryAdd("UpdateReadyMetrics", new UpdateReadyMetrics());
            AllCommands.TryAdd("GetReadyMetrics", new GetReadyMetrics());

        }

        public string GetCommand(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            return AllCommands[elMessageServer.Command].Execute(elMessageServer, elConnectionClient);
        }

        public bool ContainsCommand(string commandName)
        {
            return AllCommands.ContainsKey(commandName);
        }
    }
}
