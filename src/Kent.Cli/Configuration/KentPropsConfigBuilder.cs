using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Kent.Cli.Modules;
using Microsoft.Configuration.ConfigurationBuilders;

namespace Kent.Cli.Configuration
{
    internal class KentPropsConfigBuilder : KeyValueConfigBuilder
    {
        // TODO: Allow override of props file name through builder attribute
        internal const string PropsFileName = "Meeg.Kentico.Cms.Module.props";

        private SettingsCollection props;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            string currentDir = Environment.CurrentDirectory;

            props = LoadProps(currentDir);
        }

        internal SettingsCollection LoadProps(string propsDir)
        {
            string propsFilePath = Path.Combine(propsDir, PropsFileName);

            SettingsCollection settings = new CmsModulePropsLoader().Load(propsFilePath);

            if (settings == null)
            {
                // TODO: Add a logger instead of throwing exceptions
                //throw new InvalidOperationException($"Could not load props file at '{propsFilePath}'.");

                return new SettingsCollection();
            }

            string cmsApplicationPath = Path.GetFullPath(Path.Combine(propsDir, settings[nameof(CmsModuleSettings.CmsPath)]));

            settings.Add(nameof(CmsModuleSettings.CmsApplicationPath), cmsApplicationPath);
            settings.Add(nameof(CmsModuleSettings.CmsModulePath), propsDir);

            return settings;
        }

        public override string GetValue(string key)
        {
            return props.Get(key, null);
        }

        public override ICollection<KeyValuePair<string, string>> GetAllValues(string prefix)
        {
            return props?
                .GetAll()
                .Where(prop => prop.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
