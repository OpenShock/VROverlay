#nullable enable
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ShockLink.API
{
    public static class Config
    {
        private static Conf? _internalConfig;
        private static readonly string Path = System.IO.Path.Combine(Application.dataPath, "config.json");

        public static Conf ConfigInstance
        {
            get
            {
                TryLoad();
                return _internalConfig!;
            }
        }

        static Config()
        {
            TryLoad();
        }

        private static void TryLoad()
        {
            if (_internalConfig != null) return;
            Debug.Log("Config file found, trying to load config from " + Path);
            if (File.Exists(Path))
            {
                Debug.Log("Config file exists");
                var json = File.ReadAllText(Path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    Debug.Log("Config file is not empty");
                    try
                    {
                        _internalConfig = JsonConvert.DeserializeObject<Conf>(json);
                        Debug.Log("Successfully loaded config");
                    }
                    catch (JsonException e)
                    {
                        Debug.LogError("Error during deserialization/loading of config. " + e);
                        return;
                    }
                }
            }

            if (_internalConfig != null) return;
            Debug.Log("No valid config file found, generating new one at " + Path);
            _internalConfig = GetDefaultConfig();
            Save();
        }

        public static void Save()
        {
            Debug.Log("Saving config");
            try
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(_internalConfig, Formatting.Indented));
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while saving new config file. " + e);
            }
        }

        private static Conf GetDefaultConfig() => new()
        {
            ShockLink = new Conf.ShockLinkConf
            {
                UserHub = new Uri("https://api.shocklink.net/1/hubs/user"),
                ApiToken = "SET THIS TO YOUR SHOCKLINK API TOKEN"
            }
        };

        public class Conf
        {
            public ShockLinkConf ShockLink { get; set; }

            public class ShockLinkConf
            {
                public Uri UserHub { get; set; } = new("https://api.shocklink.net/1/hubs/user");
                public string ApiToken { get; set; }
            }
        }
    }
}