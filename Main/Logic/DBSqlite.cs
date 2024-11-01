using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Logic
{
    public class DBSqlite : IDisposable
    {
        // Database Connection
        private readonly string _connectionString;
        private SqliteConnection _connection;

        public DBSqlite()
        {
            _connectionString = "Data Source=../../Data/FinanceManagerDataBase.db"; //Default DataBase 
        }
        public DBSqlite(string connectionString)
        {
            _connectionString = $"Data Source={connectionString}";

        }

        // Execute SQL Command
        public DataTable ExecuteQuery(string query, params SqliteParameter[] parameters)
        {
            DataTable resultTable = new DataTable();

            using (_connection = new SqliteConnection(_connectionString))
            {
                _connection.Open();

                using (SqliteCommand command = new SqliteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        resultTable.Load(reader);
                    }
                }
            }

            return resultTable;
        }

        public void ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using (_connection = new SqliteConnection(_connectionString))
            {
                _connection.Open();
                using (SqliteCommand command = new SqliteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    command.ExecuteNonQuery();
                }
            }

        }
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }

        }
    }
}
