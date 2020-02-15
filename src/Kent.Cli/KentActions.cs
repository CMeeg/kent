using System;
using System.IO;
using PowerArgs;
using Kent.Cli.Modules;
using Meeg.Configuration;

namespace Kent.Cli
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    internal class KentActions
    {
        private readonly AppConfiguration config;
        private readonly CmsInitialiser cmsInitialiser;

        public KentActions()
        {
            var configManager = new ConfigurationManagerAdapter();
            config = new AppConfiguration(configManager);

            cmsInitialiser = new CmsInitialiser();
        }

        [HelpHook]
        [ArgShortcut("-?")]
        [ArgDescription("Shows this help.")]
        public bool Help { get; set; }

        [ArgActionMethod]
        [ArgDescription("Exports a module - must be executed from within a module project directory.")]
        public void Export(ExportArgs args)
        {
            var moduleSettings = config.Get<CmsModuleSettings>();

            if (moduleSettings == null || string.IsNullOrEmpty(moduleSettings.CmsModuleName))
            {
                throw new ArgException("Can't locate module name - please ensure you are running this command within a module directory.");
            }

            cmsInitialiser.Initialise(moduleSettings.CmsApplicationPath);

            var result = new ModuleExportPackageBuilder(moduleSettings)
                .WithVersion(args.Version)
                .Build(args.OutputDirectory);

            Console.WriteLine($"Successfully exported module `{result.ModuleName}` version `{result.Version}` to `{result.Path}`.");
        }
    }
}
