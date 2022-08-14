using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Serilog;
using SerilogTimings;
using System.Dynamic;

namespace MusicLibrary.Config
{
    namespace Settings
    {
        public static class Settings
        {
            /// <summary>
            /// Find settings files' location in appsettings.json.
            /// </summary>
            /// <param name="settings"></param>
            /// <returns>Returns tuple which contains retrieval result. Returns false if the setting path is not in appsettings.json.</returns>
            /// <exception cref="FileNotFoundException">Throw the exception the file path is exist in appsettings.json, but the actual file is not exist in the path.</exception>
            public static (bool, bool) HasConfigFiles(IConfiguration settings)
            {
                bool hasUserSettings = settings["MulibUserSettingsPath"] != "" ? true : false;
                bool hasLoggingSettings = settings["MulibLoggingSettingsPath"] != "" ? true : false;

                if (hasUserSettings && !File.Exists(settings["MulibUserSettingsPath"]))
                    throw new FileNotFoundException($"mulibusersettings.json is NOT found in {settings["MulibUserSettingsPath"]}");
                if (hasLoggingSettings && !File.Exists(settings["MulibLoggingSettingsPath"]))
                    throw new FileNotFoundException($"mulibloggingsettings.json is NOT found in {settings["MulibLoggingSettingsPath"]}");

                return (hasUserSettings, hasLoggingSettings);
            }

            public static void ChangeSettings(IConfiguration settings, string key, string value)
            {
                settings[key] = value;
            }

            public static void ChangeSettings<T>(ref T target, T value)
            {
                target = value;
            }

            public static void AddElement<T, U>(ref T target, U value) where T : IList<U>
            {
                target.Add(value);
            }

            public static void InsertElement<T, U>(ref T target, U value, int idx) where T : IList<U>
            {
                if (target.Count <= idx) throw new IndexOutOfRangeException("The index is out of range.");
                target.Insert(idx, value);
            }

            public static void ChangeElement<T, U>(ref T target, U value, int idx) where T : IList<U>
            {
                if (target.Count <= idx) throw new IndexOutOfRangeException("The index is out of range.");
                target[idx] = value;
            }

            /// <summary>
            /// Save settings to the file.
            /// </summary>
            /// <typeparam name="T">ISettings implemented classes</typeparam>
            /// <param name="settings">Settings class regarding configuration.</param>
            /// <param name="path">Configuration file path.</param>
            public static void SaveToSettings<T>(T settings, string path) where T : ISettings
            {
                // Serailize ISettings class to file.
                using (var op = Operation.Begin("Updating {path}", path))
                {
                    var serialized = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(path, serialized);
                    op.Complete();
                }
            }


            /// <summary>
            /// Initialize settings that are necessary to run the program.
            /// </summary>
            /// <param name="settings">settings class from appsettings.json.</param>
            /// <param name="status">The status of configuration files.</param>
            /// <param name="defaultDirectory">The default directory of files. Default value is null.</param>
            public static void InitializeSettings(IConfiguration settings, (bool, bool) status, string? defaultDirectory = null)
            {
                bool isChanged = false;
                // Copy mulibusersettings.json and update value
                defaultDirectory = defaultDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "mulib");
                Directory.CreateDirectory(defaultDirectory);

                if (!status.Item1)
                {
                    // Copy mulibusersettings.json default template file to designated path.
                    string origPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mulibusersettings.json");
                    string targetPath = Path.Combine(defaultDirectory, "mulibappsettings.json");
                    File.Copy(origPath, targetPath, overwrite: true);
                    Log.Information("Copied {original} to {target}", origPath, targetPath);

                    using (var op = Operation.Begin("Saving default database file location into {userConfig}", targetPath))
                    {
                        try
                        {
                            // Deserialize mulibusersettings.json
                            JObject jobj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(targetPath));
                            _ = jobj ?? throw new ArgumentNullException();
                            if (!jobj.TryGetValue("Library", out JToken token))
                            {
                                Log.Fatal("Failed to deserialize {target}", targetPath);
                                throw new ArgumentNullException("Invalid config file.");
                            }
                            Log.Debug("Deserialized {file} to {name}", targetPath, nameof(jobj));

                            // Apply new default path to the file.
                            var libConfig = JsonConvert.DeserializeObject<UserSettings.Library>(token.ToString());
                            Log.Debug("Deserialized {target} class to {name}", nameof(UserSettings.Library), nameof(libConfig));
                            libConfig.DatabasePath = Path.Combine(defaultDirectory, "musiclibrary.db");
                            Log.Information("Set default database path to {path}", libConfig.DatabasePath);

                            jobj["Library"] = JObject.Parse(JsonConvert.SerializeObject(libConfig, Formatting.Indented));
                            Log.Debug("Serialized {object} and updated {JObject}", nameof(libConfig), nameof(jobj));

                            File.WriteAllText(targetPath, JsonConvert.SerializeObject(jobj, Formatting.Indented));
                            op.Complete();
                        }
                        catch (ArgumentNullException e)
                        {
                            op.Abandon();
                            throw;
                        }
                    }

                    // Save mulibusersettings.json path to appsettings.json
                    isChanged = true;
                    ChangeSettings(settings, "MulibUserSettingsPath", targetPath);
                }

                if (!status.Item2)
                {
                    // Copy mulibloggingsettings.json default template file to designated path.
                    string origPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mulibloggingsettings.json");
                    string targetPath = Path.Combine(defaultDirectory, "mulibloggingsettings.json");
                    File.Copy(origPath, targetPath);
                    Log.Information("Copied {original} to {target}", origPath, targetPath);

                    // Get deserialized dynamic object
                    using (var op = Operation.Begin("Saving default database file location into {userConfig}", targetPath))
                    {
                        // Set serializer options
                        var serializerSettings = new JsonSerializerSettings();
                        serializerSettings.Converters.Add(new ExpandoObjectConverter());
                        serializerSettings.Converters.Add(new StringEnumConverter());

                        // Deserialize file
                        dynamic deserialized = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(targetPath), serializerSettings)!;
                        Log.Debug("Deserialized {targetPath} to {name}", targetPath, nameof(deserialized));

                        // Update configuration
                        deserialized.Serilog.WriteTo[0].Args.path = Path.Combine(defaultDirectory, Path.Combine("log", "mulib_.log"));
                        Log.Information("Set default log file path to {path}", deserialized.Serilog.WriteTo[0].Args.path);
                        File.WriteAllText(targetPath, JsonConvert.SerializeObject(deserialized, Formatting.Indented, serializerSettings));
                        Log.Debug("Serialized {obj}", nameof(deserialized));
                        op.Complete();
                    }

                    isChanged = true;
                    ChangeSettings(settings, "MulibLoggingSettingsPath", targetPath);
                }

                if (isChanged)
                {
                    SaveToSettings<AppSettings.AppSettings>(settings.Get<AppSettings.AppSettings>(),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
                    File.Copy("appsettings.json", "appsettings.bak.json", overwrite: true); // Create a backup file.
                }
            }
        }
    }
}