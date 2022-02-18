using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace TRMDataManager.Library.DataAccess
{
    public class SqlDataAccess : IDisposable, ISqlDataAccess
    {
        public SqlDataAccess(IConfiguration config, ILogger<ISqlDataAccess> logger)
        {
            _config = config;
            _logger = logger;
        }
        public string GetConnectionString(string name)
        {
            return _config.GetConnectionString(name);
        }

        public List<T> LoadData<T, U>(string storedProcedure, U parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                List<T> rows = cnn.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList();
                return rows;
            }
        }

        public void SaveData<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        private IDbConnection _connection;
        private IDbTransaction _transaction;
        //Open connection/start transaction method
        public void StartTransaction(string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            _connection = new SqlConnection(connectionString);
            _connection.Open();

            _transaction = _connection.BeginTransaction();

            _isClosed = false;
        }

        //Load using the transaction
        public List<T> LoadDataInTransaction<T, U>(string storedProcedure, U parameters)
        {
            List<T> rows = _connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure, transaction: _transaction).ToList();
            return rows;
        }

        //Save using transaction
        public void SaveDataInTransaction<T>(string storedProcedure, T parameters)
        {
            _connection.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure, transaction: _transaction);
        }

        //Close connection/stop transaction

        private bool _isClosed = false;
        private readonly IConfiguration _config;
        private readonly ILogger<ISqlDataAccess> _logger;

        public void CommitTransaction()
        {
            //successed transaction commit && close conn
            _transaction?.Commit();
            _connection?.Close();

            _isClosed = true;
        }

        public void RollBackTransaction()
        {
            //failed transaction rollback && close conn
            _transaction?.Rollback();
            _connection?.Close();

            _isClosed = true;
        }

        //Implement dispose
        public void Dispose()
        {
            if (_isClosed == false)
            {
                try
                {
                    CommitTransaction();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Commit transaction fail in the dispose method");
                }
            }

            _transaction = null;
            _connection = null;
        }
    }
}
