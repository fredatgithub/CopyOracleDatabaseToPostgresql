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
      foreach (var child in ((StackPanel)this.FindName("ListOfActions")).Children)
      {
        if (child is CheckBox checkBox && checkBox.IsChecked == true)
        {
          checkedItems.Add(checkBox.Content.ToString());
        }
      }

      if (checkedItems.Count == 0)
      {
        TextResult.Text = "Aucune action n'est cochée.";
      }
      else
      {
        TextResult.Text = "Les étapes qui seront faites sont les suivantes :";
        TextResult.Text += Environment.NewLine;
      }

      TextResult.Text += string.Join("\n", checkedItems);

      // create roles
      if (checkedItems.Contains("Create Role"))
      {
        TextResult.Text += Environment.NewLine;
        TextResult.Text += "Création des Rôles";
        TextResult.Text += Environment.NewLine;
        var roleNameList = BddAccess.GetRoleList();
        foreach (var roleName in roleNameList)
        {
          var sqlRequest = BddAccess.GetCreationRoleSqlRequest(roleName);
          var creationRoleResult = BddAccess.ExecuteSqlRequest(sqlRequest);
          if (creationRoleResult.StartsWith("ok"))
          {
            TextResult.Text += $"Le rôle {roleName} a été créé.";
          }
          else
          {
            TextResult.Text += $"Erreur lors de la création du rôle {roleName}, l'erreur est : {creationRoleResult.Substring(3)} ";
          }
        }
      }

      // create schemas
      if (checkedItems.Contains("Create Schemas"))
      {
        TextResult.Text += Environment.NewLine;
        TextResult.Text += "Création des Schémas";
        TextResult.Text += Environment.NewLine;
        var schemaNameList = BddAccess.GetSchemaList();
        foreach (var schemaName in schemaNameList)
        {
          var sqlRequest = BddAccess.GetCreationSchemaSqlRequest(schemaName);
          var creationSchemaResult = BddAccess.ExecuteSqlRequest(sqlRequest);
          if (creationSchemaResult.StartsWith("ok"))
          {
            TextResult.Text += $"Le schéma {schemaName} a été créé.";
          }
          else
          {
            TextResult.Text += $"Erreur lors de la création du schéma {schemaName}, l'erreur est : {creationSchemaResult.Substring(3)} ";
          }
        }
      }

      // create tables
      if (checkedItems.Contains("Create tables"))
      {
        TextResult.Text += Environment.NewLine;
        TextResult.Text += "Création des Tables";
        TextResult.Text += Environment.NewLine;
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
  }
}
