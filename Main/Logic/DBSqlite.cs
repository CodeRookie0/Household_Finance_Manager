using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        private SqliteTransaction _transaction;

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

        public SqliteTransaction BeginTransaction()
        {
            using (_connection = new SqliteConnection(_connectionString))
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
                _transaction = _connection.BeginTransaction();
                return _transaction;
            }
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

        public object ExecuteScalar(string query, params SqliteParameter[] parameters)
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

                    return command.ExecuteScalar();
                }
            }
        }

        public List<Dictionary<string, object>> ExecuteQueryToList(string query, params SqliteParameter[] parameters)
        {
            var results = new List<Dictionary<string, object>>();

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
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader[i];
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return results;
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
