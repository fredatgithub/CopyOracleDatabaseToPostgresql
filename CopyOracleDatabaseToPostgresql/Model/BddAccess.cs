using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Npgsql;

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
      var connectionString = GetConnectionString();
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

    private static string GetConnectionString()
    {
      const string filename = "connectionString.txt";
      if (!File.Exists(filename))
      {
        CreateConnectionStringFile(filename);
      }

      return File.ReadAllText(filename);
    }

    private static void CreateConnectionStringFile(string filename)
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

      var tableList =  new List<string>(File.ReadAllLines(tableListFilename));
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

    internal static string GetDataFromOracle(string connectionString, string tableName)
    {
      string result = "ok";
      // TODO: get data from oracle

      return result;
    }

    internal static string InsertDataIntoPostgresql(object data)
    {
      var result = "ok";
      // TODO: insert data into postgresql

      return result;
    }
  }
}
