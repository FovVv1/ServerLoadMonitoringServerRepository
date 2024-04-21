using ElMessage;
using ElMessage.Interface;
using ServerLoadMonitoringDataModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using NLog;

namespace ServerLoadMonitoringServer.Commands
{
    internal class GetTasksFromTaskManager : ICommandServer
    {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try { 
            List<TaskCountItem> TasksCount = new List<TaskCountItem>();
            using (SqlConnection db = new SqlConnection(elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString))
            {
                db.Open();

                // Создаем команду для вызова хранимой процедуры
                using (SqlCommand command = new SqlCommand("ServerLoadMonitoring_GetTasksCount", db))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Выполняем запрос и получаем результаты
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaskCountItem item = new TaskCountItem();
                            item.LAN = reader["LAN"].ToString();
                            item.Count = Convert.ToInt32(reader["Count"]);
                            item.LANAndCount = $"{item.LAN} - {item.Count}";
                            TasksCount.Add(item);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(new { TasksCount = TasksCount });
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e, "Error");
                return JsonConvert.SerializeObject(false);
            }
        }
    }
}
