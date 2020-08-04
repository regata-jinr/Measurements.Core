using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Configuration.Json;

namespace Regata.Measurements.Core
{
  public enum Languages { Russian, English };

  public class BaseParams
  {
    public Languages CurrentLanguage { get; set; }
    public string UserID { get; set; }
  }

  public class SessionParams
  {

  }

  public class Secrets
  {
    public string LogConnectionString { get; set; }
    public string GenConnectionStringBase { get; set; }
  }

  public static class SettingsManager
  {
    public static string FilePath
    {
      get
      {
        if (string.IsNullOrEmpty(AssemblyName)) throw new ArgumentNullException("You must specify name of calling assembly. Just use 'System.Reflection.Assembly.GetExecutingAssembly().GetName().Name' as argument.");
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Regata\\{AssemblyName}\\settings.json";
      }
    }

    public static string UserId { get; private set; }

    public static BaseParams BasicParams { get; private set; }
    public static SessionParams SessionParameters { get; private set; }
    public static Secrets ConnectionParameters { get; private set; }

    private static string _assmName;

    public static string AssemblyName
    {
      get { return _assmName; }
      set
      {
        _assmName = value;
        ReadSettings();
      }
    }

    private static Languages _currentLanguage;

    public static Languages CurrentLanguage
    {
      get { return _currentLanguage; }
      set
      {
        _currentLanguage = value;
        LanguageChanged?.Invoke();
        SaveSettings();
      }
    }

    public static event Action LanguageChanged;

    private static void ReadSettings()
    {
      try
      {
        if (!File.Exists(FilePath))
          ResetFileSettings();

        var config = new ConfigurationBuilder()
        .AddUserSecrets<Secrets>()
        .AddJsonFile(FilePath, optional: false, reloadOnChange: true)
        .Build();

        BasicParams = new BaseParams();
        SessionParameters = new SessionParams();
        ConnectionParameters = new Secrets();

        config.GetSection(nameof(BaseParams)).Bind(BasicParams);
        config.GetSection(nameof(SessionParams)).Bind(SessionParameters);
        config.GetSection(nameof(Secrets)).Bind(ConnectionParameters);

      }
      catch (JsonException)
      {
        ResetFileSettings();
      }
    }

    private static void ResetFileSettings()
    {
      Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
      using (var f = File.CreateText(FilePath))
      { }

      CurrentLanguage = Languages.English;
      SaveSettings();
    }

    public static void SaveSettings()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
      options.WriteIndented = true;
      File.WriteAllText(FilePath, JsonSerializer.Serialize(new BaseParams { CurrentLanguage = CurrentLanguage }, options));
      File.AppendAllText(FilePath, JsonSerializer.Serialize(new SessionParams { }, options));
    }

  } // public class Settings
} // namespace Regata.UITemplates