using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.File;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Dynamic;

namespace MusicLibrary.Config
{
    namespace Settings
    {
        public static class Settings
        {
            public static string? GetUserSettingsPath(IConfiguration appsettings)
            {
                var userSettingsPath = appsettings.Get<AppSettings.AppSettings>().MulibUserSettingsPath;

                return userSettingsPath;
            }

            public static bool HasUserSettings(IConfiguration appsettings)
            {
                var path = GetUserSettingsPath(appsettings);

                return path != "";
            }

            public static bool ConfigureSettings(IConfiguration appsettings)
            {
                // Copy mulibusersettings.json to default path.
                var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic
                , Environment.SpecialFolderOption.Create), "mulib");
                var configPath = Path.Combine(defaultPath, "mulibusersettings.json");

                try
                {
                    if (!Directory.Exists(defaultPath)) Directory.CreateDirectory(defaultPath);
                    File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mulibusersettings.json"),
                    configPath, overwrite: true);
                }
                catch (FileNotFoundException e)
                {
                    Log.Error("Copying file is failed: Original mulibusersettings.json was NOT found");
                    Log.Error(e!.StackTrace);

                    return false;
                }
                catch (UnauthorizedAccessException e)
                {
                    Log.Error($"Copying file is failed: Cannot access to the destination path: {configPath}");
                    Log.Error(e!.StackTrace);

                    return false;
                }
                catch (IOException e)
                {
                    Log.Error($"Copying file is failed: I/O error was occurred");
                    Log.Error(e!.StackTrace);

                    return false;
                }


                // Update mulibusersettings.json.
                var configs = GetSerializedSettings(configPath, out JsonSerializerSettings settings);
                configs.Library.DatabasePath = Path.Combine(defaultPath, "musiclibrary.db");
                configs.Serilog.WriteTo[0].Args.path = Path.Combine(defaultPath, "mulib_.log");

                var saved = JsonConvert.SerializeObject(configs, Formatting.Indented, settings);
                File.WriteAllText(configPath, saved);


                // Update appsettings.json by serialization
                // appsettings.Get<AppSettings.AppSettings>().MulibUserSettingsPath
                //    = Path.Combine(configPath);

                return true;
            }

            public static bool WriteToSettings(string path, IConfiguration configTarget)
            {
                var writeOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                };
                writeOptions.Converters.Add(new JsonStringEnumConverter());

                var updated = System.Text.Json.JsonSerializer.Serialize(configTarget, writeOptions);
                File.WriteAllText(path, updated);

                return true;
            }

            public static dynamic GetSerializedSettings(string path, out JsonSerializerSettings settings)
            {
                // source: https://makolyte.com/csharp-how-to-update-appsettings-json-programmatically/#Step_3_-_Change_values
                // Read file by using Json.NET
                var settingsFile = File.ReadAllText(path);

                settings = new JsonSerializerSettings();
                settings.Converters.Add(new ExpandoObjectConverter());
                settings.Converters.Add(new StringEnumConverter());
                return JsonConvert.DeserializeObject<ExpandoObject>(settingsFile, settings)!;
            }
        }
    }
}
