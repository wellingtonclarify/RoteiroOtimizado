using System.Data;

namespace RoteiroOtimizado.DbContext
{
    public static class DbConnectionFactory
    {
        public static IDbConnection GetInstance()
        {
            var connectionString = "Data Source=192.168.15.31;Initial Catalog=v201906.3_LOGA_SGO_V3;Persist Security Info=True;User ID=sa;Password=123456;MultipleActiveResultSets=True";
            var connection = DbConnection.GetInstance(connectionString);
            connection.Open();
            return connection;
        }
    }
}
