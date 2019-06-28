﻿using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace RoteiroOtimizado.DbContext
{
    public class DbConnection : IDbConnection
    {
        private readonly IDbConnection _dbConnection = null;

        private DbConnection(string connectionString)
        {
            _dbConnection = new SqlConnection(connectionString);
        }

        public static IDbConnection GetInstance(string connectionString)
        {
            return new DbConnection(connectionString)._dbConnection;
        }

        public string ConnectionString { get => _dbConnection.ConnectionString; set => _dbConnection.ConnectionString = value; }

        public int ConnectionTimeout => _dbConnection.ConnectionTimeout;

        public string Database => _dbConnection.Database;

        public ConnectionState State => _dbConnection.State;

        public IDbTransaction BeginTransaction()
        {
            return _dbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _dbConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            _dbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            _dbConnection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return _dbConnection.CreateCommand();
        }

        public void Dispose()
        {
            _dbConnection.Dispose();
        }

        public void Open()
        {
            _dbConnection.Open();
        }
    }
}
