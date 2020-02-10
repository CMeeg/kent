using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kent.Cli.Configuration
{
    internal class SettingsCollection
    {
        private readonly ConcurrentDictionary<string, string> settings;

        public string this[string name] => settings[name];

        public SettingsCollection()
        {
            settings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(string name, string value)
        {
            settings[name] = value;
        }

        public string Get(string name, string defaultValue)
        {
            if (settings.TryGetValue(name, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            foreach (KeyValuePair<string, string> setting in settings)
            {
                yield return setting;
            }
        }
    }
}
