using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Schema;
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

    private static string GetConnectionString()
    {
      const string filename = "connectionString.txt";
      if (File.Exists(filename))
      {
        return File.ReadAllText(filename);
      }
      else
      {
        CreateConnectionStringFile(filename);
        return File.ReadAllText(filename);
      }
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
      if (File.Exists(roleListFilename))
      {
        return new List<string>(File.ReadAllLines(roleListFilename));
      }
      else
      {
        CreateRoleListFile(roleListFilename);
        return new List<string>(File.ReadAllLines(roleListFilename));
      }
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

    internal static IEnumerable<string> GetSchemaList()
    {
      const string schemaListFilename = "schemaList.txt";
      if (File.Exists(schemaListFilename))
      {
        return new List<string>(File.ReadAllLines(schemaListFilename));
      }
      else
      {
        CreateSchemaListFile(schemaListFilename);
        return new List<string>(File.ReadAllLines(schemaListFilename));
      }
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

    internal static IEnumerable<string> GetTableList()
    {
      const string tableListFilename = "tableList.txt";
      if (File.Exists(tableListFilename))
      {
        return new List<string>(File.ReadAllLines(tableListFilename));
      }
      else
      {
        CreateTableListFile(tableListFilename);
        return new List<string>(File.ReadAllLines(tableListFilename));
      }
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

    internal static string GetCreationTableSqlRequest(string tableName, string sqlSchema)
    {
      return $"CREATE TABLE {sqlSchema}.{tableName} (dual int2 NULL);";
    }
  }
}
