using System.Windows;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Controls;

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
        checkedItems.Add("Aucune case à cocher n'est cochée.");
      }

      TextResult.Text = string.Join("\n", checkedItems);
    }
  }
}
