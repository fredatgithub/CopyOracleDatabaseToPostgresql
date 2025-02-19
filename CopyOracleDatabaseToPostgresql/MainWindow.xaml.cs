using System.Windows;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Controls;
using System;
using CopyOracleDatabaseToPostgresql.Model;

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
        TextResult.Text = "Aucune case à cocher n'est cochée.";
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
    }
  }
}
