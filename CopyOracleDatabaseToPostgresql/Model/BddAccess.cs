using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace CopyOracleDatabaseToPostgresql.Model
{
  public static class BddAccess
  {
    public static string GetCreationRoleSqlRequest(string roleName)
    {
      return $"CREATE ROLE {roleName} WITH NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT LOGIN NOREPLICATION NOBYPASSRLS CONNECTION LIMIT -1;";
    }

    public static string ExecuteSqlRequest(string sqlRequest)
    {
      var connectionString = GetPostgresqlConnectionString();
      var result = ExecuteNonQuery(connectionString, sqlRequest);
      return result;
    }

    public static string ExecuteNonQuery(string connexionString, string sqlRequest)
    {
      var result = string.Empty;
      var connexion = new NpgsqlConnection(connexionString);
      try
      {
        connexion.Open();
        var command = new NpgsqlCommand(sqlRequest, connexion)
        {
          CommandType = CommandType.Text
        };
        int returnRows = command.ExecuteNonQuery();
        result = $"ok|{returnRows}";
      }
      catch (Exception exception)
      {
        result = $"ko|{exception.Message}";
      }
      finally
      {
        connexion.Close();
      }

      return result;
    }

    public static string GetOracleConnectionString()
    {
      const string filename = "OracleConnectionString.txt";
      if (!File.Exists(filename))
      {
        CreateOracleConnectionStringFile(filename);
      }

      return File.ReadAllText(filename);
    }

    private static void CreateOracleConnectionStringFile(string filename)
    {
      try
      {
        using (var file = File.CreateText(filename))
        {
          const string connectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SID=OID)));User Id=username;Password=password;";

          file.WriteLine(connectionString);
        }
      }
      catch (Exception exception)
      {
        throw new ArgumentException(exception.Message);
      }
    }

    public static string GetPostgresqlConnectionString()
    {
      const string filename = "PostgresqlConnectionString.txt";
      if (!File.Exists(filename))
      {
        CreatePostgresqlConnectionStringFile(filename);
      }

      return File.ReadAllText(filename);
    }

    private static void CreatePostgresqlConnectionStringFile(string filename)
    {
      try
      {
        using (var file = File.CreateText(filename))
        {
          file.WriteLine("Server=localhost;Port=5432;Database=databaseName;User Id=username;Password=password;");
        }
      }
      catch (Exception exception)
      {
        throw new ArgumentException(exception.Message);
      }
    }

    public static List<string> GetRoleList()
    {
      const string roleListFilename = "roleList.txt";
      if (!File.Exists(roleListFilename))
      {
        CreateRoleListFile(roleListFilename);
      }

      return new List<string>(File.ReadAllLines(roleListFilename));
    }

    private static void CreateRoleListFile(string filename)
    {
      try
      {
        using (var file = File.CreateText(filename))
        {
          file.WriteLine("role1");
        }
      }
      catch (Exception exception)
      {
        throw new ArgumentException(exception.Message);
      }
    }

    internal static List<string> GetSchemaList()
    {
      const string schemaListFilename = "schemaList.txt";
      if (!File.Exists(schemaListFilename))
      {
        CreateSchemaListFile(schemaListFilename);
      }

      return new List<string>(File.ReadAllLines(schemaListFilename));
    }

    private static void CreateSchemaListFile(string filename)
    {
      try
      {
        using (var file = File.CreateText(filename))
        {
          file.WriteLine("schema1");
        }
      }
      catch (Exception exception)
      {
        throw new ArgumentException(exception.Message);
      }
    }

    internal static string GetCreationSchemaSqlRequest(string schemaName)
    {
      return $"CREATE SCHEMA {schemaName} AUTHORIZATION {schemaName};";
    }

    internal static List<string> GetTableListForSchema(string schema)
    {
      // TODO : get table list from schema from database
      return new List<string>();
    }

    internal static IEnumerable<string> GetTableList(string tableListFilename)
    {
      if (!File.Exists(tableListFilename))
      {
        CreateTableListFile(tableListFilename);
      }

      var tableList = new List<string>(File.ReadAllLines(tableListFilename));
      return tableList;
    }

    private static void CreateTableListFile(string filename)
    {
      try
      {
        using (var file = File.CreateText(filename))
        {
          file.WriteLine("table1");
        }
      }
      catch (Exception exception)
      {
        throw new ArgumentException(exception.Message);
      }
    }

    internal static string GetCreationTableSqlRequest(string tableName, string sqlSchema, bool forDualTable = false)
    {
      string sqlRequest = $"CREATE TABLE {sqlSchema}.{tableName} ";
      if (forDualTable)
      {
        return $"{sqlRequest}(dual int2 NULL);";
      }

      return $"{sqlRequest};";
    }

    internal static OracleDataReader GetDataFromOracle(string connectionString, string tableName)
    {
      string request = $"SELECT * FROM {tableName}";
      using (var connection = new OracleConnection(connectionString))
      {
        connection.Open();
        using (var command = new OracleCommand(request, connection))
        {
          var reader = command.ExecuteReader();
          return reader;
        }
      }
    }

    internal static string InsertDataIntoPostgresql(string connectionString, string tablename, OracleDataReader data)
    {
      var result = "ok";
      if (data == null)
      {
        return result;
      }

      using (var connection = new NpgsqlConnection(connectionString))
      {
        connection.Open();
        using (var transaction = connection.BeginTransaction()) 
        using (var command = new NpgsqlCommand())
        {
          command.Connection = connection;
          command.Transaction = transaction; 

          try
          {
            var columnNames = new List<string>();
            var parameterNames = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            for (int i = 0; i < data.FieldCount; i++)
            {
              columnNames.Add(data.GetName(i));
              parameterNames.Add("@param" + i);
            }

            command.CommandText = $"INSERT INTO {tablename} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)})";

            while (data.Read()) 
            {
              command.Parameters.Clear(); 

              for (int i = 0; i < data.FieldCount; i++)
              {
                var param = new NpgsqlParameter(parameterNames[i], data.GetValue(i) ?? DBNull.Value);
                command.Parameters.Add(param);
              }

              command.ExecuteNonQuery();
            }

            transaction.Commit(); 
          }
          catch (Exception exception)
          {
            transaction.Rollback(); 
            result = $"ko|Erreur : {exception.Message}";
          }
        }
      }

      return result;
    }

    internal static bool DisableAllConstraints()
    {
      // get disable constraints sql request
      var sqlRequest = "ALTER TABLE table_name DISABLE CONSTRAINT ALL;";
      var result = ExecuteSqlRequest(sqlRequest);
      return result.StartsWith("ok");
    }
  }
}
