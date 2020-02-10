using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Kent.Cli.Modules;
using Microsoft.Configuration.ConfigurationBuilders;

namespace Kent.Cli.Configuration
{
    internal class KentSecretsConfigBuilder : KeyValueConfigBuilder
    {
        // TODO: Allow override of secrets file name through builder attribute
        internal const string SecretsFileName = "ConnectionStrings.config";

        private SettingsCollection secrets;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            string currentDir = Environment.CurrentDirectory;

            secrets = LoadSecrets(currentDir);
        }

        internal SettingsCollection LoadSecrets(string secretsDir)
        {
            // TODO: Can use values from KentPropsConfigBuilder here rather than loading it? Maybe easier to do that with v2 ConfigurationBuilders?

            SettingsCollection props = new KentPropsConfigBuilder().LoadProps(secretsDir);

            string cmsPath = props.Get(nameof(CmsModuleSettings.CmsApplicationPath), null);

            if (cmsPath == null)
            {
                // TODO: Add a logger instead of throwing exceptions
                //throw new InvalidOperationException($"Property not found in module props file '{nameof(CmsModuleSettings.CmsPath)}'.");

                return new SettingsCollection();
            }

            string secretsFilePath = Path.Combine(cmsPath, SecretsFileName);

            SettingsCollection settings = new UserSecretsLoader().Load(secretsFilePath);

            if (settings == null)
            {
                // TODO: Add a logger instead of throwing exceptions
                // throw new InvalidOperationException($"Could not load secrets file at '{secretsFilePath}'.");

                return new SettingsCollection();
            }

            return settings;
        }

        public override string GetValue(string key)
        {
            return secrets?.Get(key, null);
        }

        public override ICollection<KeyValuePair<string, string>> GetAllValues(string prefix)
        {
            return secrets?
                .GetAll()
                .Where(secret => secret.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
