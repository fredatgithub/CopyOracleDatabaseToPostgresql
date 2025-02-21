using System.Windows;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Controls;
using System;
using CopyOracleDatabaseToPostgresql.Model;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CopyOracleDatabaseToPostgresql
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    private const string SettingsFile = "settings.json";

    public MainWindow()
    {
      InitializeComponent();
      Loaded += MainWindow_Loaded;
      Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      RestoreWindowSettings();
      LoadSqlSchemas();
    }

    private void LoadSqlSchemas()
    {
      List<string> schemaList = BddAccess.GetSchemaList();
      for (int i = 0; i < schemaList.Count; i++)
      {
        switch (i)
        {
          case 0:
            Schema1.Content = schemaList[i];
            CreateTablesSchema1.Content = "Create tables in " + schemaList[i];
            FillTablesSchema1.Content = "Fill tables in " + schemaList[i];
            break;
          case 1:
            Schema2.Content = schemaList[i];
            CreateTablesSchema2.Content = "Create tables in " + schemaList[i];
            FillTablesSchema2.Content = "Fill tables in " + schemaList[i];
            break;
          case 2:
            Schema3.Content = schemaList[i];
            CreateTablesSchema3.Content = "Create tables in " + schemaList[i].Replace("_", "__");
            FillTablesSchema3.Content = "Fill tables in " + schemaList[i].Replace("_", "__");
            break;
          case 3:
            Schema4.Content = schemaList[i];
            CreateTablesSchema4.Content = "Create tables in " + schemaList[i].Replace("_", "__");
            FillTablesSchema4.Content = "Fill tables in " + schemaList[i].Replace("_", "__");
            break;
          default:
            break;
        }
      }
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveWindowSettings();
    }

    private void SaveWindowSettings()
    {
      var settings = new
      {
        this.Width,
        this.Height,
        this.Left,
        this.Top
      };

      File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(settings));
    }

    private void RestoreWindowSettings()
    {
      if (File.Exists(SettingsFile))
      {
        var settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(SettingsFile));
        this.Width = settings.Width;
        this.Height = settings.Height;
        this.Left = settings.Left;
        this.Top = settings.Top;
      }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
      var checkedItems = new List<string>();
      foreach (var child in ((StackPanel)FindName("ListOfActions")).Children)
      {
        if (child is CheckBox checkBox && checkBox.IsChecked == true)
        {
          checkedItems.Add(checkBox.Content.ToString());
        }
      }

      if (checkedItems.Count == 0)
      {
        TextResult.Text = "Aucune action n'est cochée.";
        return;
      }
      else
      {
        TextResult.Text = "Les étapes qui seront faites sont les suivantes :";
        AddNewLine();
      }

      TextResult.Text += string.Join("\n", checkedItems);

      foreach (string item in checkedItems)
      {
        // create roles
        string firstTwoWords = item.Split(' ').Take(2).Aggregate((a, b) => a + " " + b);
        switch (firstTwoWords)
        {
          case "Create Roles":
            CreateRoleFor(item);
            break;
          case "Create Schemas":
            CreateSchemasFor(item);
            break;
          case "Create tables":
            CreateTables(item);
            break;
          case "Fill tables":
            FillTables(item);
            break;
          default:
            AddNewLine();
            TextResult.Text += "Pas d'action trouvée.";
            AddNewLine();
            break;
        }
      }
    }

    private void FillTables(string item)
    {
      // Fill Tables
      if (item.Contains("Fill tables"))
      {
        var schemaName = item.Replace("Fill tables in ", "");
        var tablesList = BddAccess.GetTableList();
        var tableName = tablesList.FirstOrDefault();
        // get data from oracle
        var oracleConnectionString = BddAccess.GetOracleConnectionString();
        var data = BddAccess.GetDataFromOracle(oracleConnectionString, tableName);
        // insert data into postgresql
        var insertDataResult = BddAccess.InsertDataIntoPostgresql(data);
        if (insertDataResult.StartsWith("ok"))
        {
          TextResult.Text += $"Les données ont été insérées dans la table {tableName}";
        }
        else
        {
          TextResult.Text += $"Erreur lors de l'insertion des données dans la table {tableName}, l'erreur est : {insertDataResult.Substring(3)} ";
        }
      }
    }

    private void CreateTables(string item)
    {
      // create tables
      if (item.Contains("Create tables"))
      {
        AddNewLine();
        TextResult.Text += "Création des Tables";
        AddNewLine();
        var tableNameList = BddAccess.GetTableList();
        foreach (var tableName in tableNameList)
        {
          var sqlRequest = BddAccess.GetCreationTableSqlRequest(tableName, "schema1");
          var creationTableResult = BddAccess.ExecuteSqlRequest(sqlRequest);
          if (creationTableResult.StartsWith("ok"))
          {
            TextResult.Text += $"La table {tableName} a été créée.";
          }
          else
          {
            TextResult.Text += $"Erreur lors de la création de la table {tableName}, l'erreur est : {creationTableResult.Substring(3)} ";
          }
        }
      }
    }

    private void CreateSchemasFor(string item)
    {
      // create schemas
      if (item.Contains("Create Schemas"))
      {
        AddNewLine();
        TextResult.Text += "Création des Schémas";
        AddNewLine();
        var schemaNameList = BddAccess.GetSchemaList();
        foreach (var schemaName in schemaNameList)
        {
          var sqlRequest = BddAccess.GetCreationSchemaSqlRequest(schemaName);
          var creationSchemaResult = BddAccess.ExecuteSqlRequest(sqlRequest);
          if (creationSchemaResult.StartsWith("ok"))
          {
            TextResult.Text += $"Le schéma {schemaName} a été créé.";
            AddNewLine();
          }
          else
          {
            TextResult.Text += $"Erreur lors de la création du schéma {schemaName}, l'erreur est : {creationSchemaResult.Substring(3)} ";
            AddNewLine();
          }
        }
      }
    }

    private void CreateRoleFor(string item)
    {
      // create roles
      if (item.Contains("Create Roles"))
      {
        AddNewLine();
        TextResult.Text += "Création des Rôles";
        AddNewLine();
        var roleNameList = BddAccess.GetRoleList();
        foreach (var roleName in roleNameList)
        {
          var sqlRequest = BddAccess.GetCreationRoleSqlRequest(roleName);
          var creationRoleResult = BddAccess.ExecuteSqlRequest(sqlRequest);
          if (creationRoleResult.StartsWith("ok"))
          {
            TextResult.Text += $"Le rôle {roleName} a été créé.";
            AddNewLine();
          }
          else
          {
            TextResult.Text += $"Erreur lors de la création du rôle {roleName}, l'erreur est : {creationRoleResult.Substring(3)} ";
            AddNewLine();
          }
        }
      }
    }

    private void AddNewLine()
    {
      TextResult.Text += Environment.NewLine;
    }
  }
}