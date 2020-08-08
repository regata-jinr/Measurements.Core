using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;

namespace Regata.Measurements.Core
{
  public enum Languages { Russian, English };

  public class BaseParams
  {
    public Languages CurrentLanguage { get; set; }
    public List<string> SubscribedEmails { get; set; }
  }

  public class SessionParams
  {
    // TODO: to be done... 

  }

  public class MeasurementsParams
  {
    // TODO: to be done... 
    // count mode
    // duration
    // height
    // iscyclic
    // pause
  }

  public class SampleChangerParams
  {
    // TODO: to be done... 
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

    public static BaseParams BasicParams { get; private set; }
    public static SessionParams SessionParameters { get; private set; }
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