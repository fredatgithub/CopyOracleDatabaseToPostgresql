using System;
using System.Data;
using System.Dynamic;
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
        var command = new NpgsqlCommand(sqlRequest, connexion);
        command.CommandType = CommandType.Text;
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
      string filename = "connectionString.txt";
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
  }
}
