using System.Windows;
using System.IO;
using Newtonsoft.Json;

namespace CopyOracleDatabaseToPostgresql
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
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
      var settings = new {
        Width = this.Width,
        Height = this.Height,
        Left = this.Left,
        Top = this.Top
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
  }
}
