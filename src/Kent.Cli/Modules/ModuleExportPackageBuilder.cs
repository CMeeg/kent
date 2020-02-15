using System;
using System.Xml.Serialization;
using CMS.Base;
using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Modules;

namespace Kent.Cli.Modules
{
    internal class ModuleExportPackageBuilder
    {
        private const string ExportFileName = "ModuleExport.zip";
        private const string MetaDataFileName = "ModuleMetaData.xml";

        private readonly CmsModuleSettings settings;
        private readonly ResourceInfo module;
        private readonly IUserInfo user;
        private readonly ModuleDataProvider moduleDataProvider;

        public string Version { get; private set; }

        public ModuleExportPackageBuilder(CmsModuleSettings settings)
        {
            this.settings = settings;

            module = ResourceInfoProvider.GetResourceInfo(settings.CmsModuleName);

            if (module == null)
            {
                throw new InvalidOperationException($"Module '{settings.CmsModuleName}' does not exist.");
            }

            user = UserInfoProvider.GetUserInfo("administrator") ?? CMSActionContext.CurrentUser;

            moduleDataProvider = new ModuleDataProvider(module);
        }

        public ModuleExportPackageBuilder WithVersion(string version)
        {
            Version = version;

            module.ResourceVersion = version;

            return this;
        }

        public Result Build(string targetPath)
        {
            ValidateBuild();

            SiteExportSettings exportSettings = CreateNewSettings(targetPath);

            EnsureFolders(exportSettings);

            AddModule(exportSettings);

            AddModuleObjects(exportSettings);

            ExportProvider.DeleteTemporaryFiles(exportSettings, true);

            new ExportManager(exportSettings).Export(null);

            ExportProvider.DeleteTemporaryFiles(exportSettings, true);

            SaveInstallationMetaData(exportSettings);

            return Result.Create(module, exportSettings);
        }

        public class Result
        {
            public string ModuleName { get; private set; }
            public string Version { get; private set; }
            public string Path { get; private set; }

            public static Result Create(ResourceInfo module, SiteExportSettings exportSettings)
            {
                return new Result
                {
                    ModuleName = module.ResourceName,
                    Version = module.ResourceVersion,
                    Path = exportSettings.TargetPath
                };
            }
        }

        private void ValidateBuild()
        {
            if (string.IsNullOrEmpty(Version))
            {
                throw new ArgumentException($"{nameof(Version)} has not been set.");
            }

            if (user == null)
            {
                throw new ArgumentException($"{nameof(user)} has not been set.");
            }
        }

        private SiteExportSettings CreateNewSettings(string targetPath)
        {
            string targetFolderPath = GetFullPath(targetPath);
            string tempFolderPath = GetFullPath(Path.Combine(targetFolderPath, "_temp", Guid.NewGuid().ToString()));

            var siteExportSettings = new SiteExportSettings(user)
            {
                TargetPath = targetFolderPath,
                TargetFileName = ExportFileName,
                TemporaryFilesPath = tempFolderPath,
                WebsitePath = SystemContext.WebApplicationPhysicalPath
            };

            siteExportSettings.SetInfo("ModuleName", module.ResourceName);
            siteExportSettings.CopyFiles = false;
            siteExportSettings.SetSettings("BizFormData", false);
            siteExportSettings.SetSettings("CustomTableData", false);
            siteExportSettings.SetSettings("ForumPosts", false);
            siteExportSettings.SetSettings("BoardMessages", false);
            siteExportSettings.SetSettings("GlobalFolders", false);
            siteExportSettings.SetSettings("SiteFolders", false);
            siteExportSettings.SetSettings("CopyASPXTemplatesFolder", false);
            siteExportSettings.SiteId = 0;
            siteExportSettings.DefaultProcessObjectType = ProcessObjectEnum.Selected;
            siteExportSettings.ExportType = ExportTypeEnum.None;
            siteExportSettings.TimeStamp = DateTimeHelper.ZERO_TIME;

            return siteExportSettings;
        }

        private string GetFullPath(string path)
        {
            return Path.GetFullPath(DirectoryHelper.EnsurePathBackSlash(path));
        }

        private void EnsureFolders(SiteExportSettings exportSettings)
        {
            DirectoryHelper.EnsureDiskPath(exportSettings.TargetPath, settings.CmsModulePath);

            if (!DirectoryHelper.CheckPermissions(exportSettings.TargetPath, true, true, false, false))
            {
                throw new UnauthorizedAccessException($"Missing read and write permissions necessary for export package target folder '{exportSettings.TargetPath}'.");
            }

            DirectoryHelper.EnsureDiskPath(exportSettings.TemporaryFilesPath, settings.CmsModulePath);

            if (!DirectoryHelper.CheckPermissions(exportSettings.TemporaryFilesPath, true, true, false, false))
            {
                throw new UnauthorizedAccessException($"Missing read and write permissions for export package temporary folder '{exportSettings.TemporaryFilesPath}'.");
            }
        }

        private void AddModule(SiteExportSettings exportSettings)
        {
            exportSettings.Select("cms.resource", module.ResourceName, false);
        }

        private void AddModuleObjects(SiteExportSettings exportSettings)
        {
            foreach (string supportedObjectType in moduleDataProvider.SupportedObjectTypes)
            {
                string localObjectType = supportedObjectType;

                ObjectQuery moduleObjects = moduleDataProvider.GetModuleObjects(supportedObjectType);

                string codeNameColumn = moduleObjects.TypeInfo.CodeNameColumn;

                moduleObjects.Column(codeNameColumn).ForEachRow(obj => exportSettings.Select(
                    localObjectType,
                    (string)obj[codeNameColumn],
                    false
                ));
            }
        }

        private void SaveInstallationMetaData(SiteExportSettings exportSettings)
        {
            var metaData = new ModuleInstallationMetaData
            {
                Name = module.ResourceName,
                Version = Version
            };

            string targetFolderPath = exportSettings.TargetPath;
            string targetFilePath = Path.Combine(targetFolderPath, MetaDataFileName);

            using (var fileStream = FileStream.New(targetFilePath, FileMode.Create, FileAccess.Write))
            {
                new XmlSerializer(typeof(ModuleInstallationMetaData))
                    .Serialize(fileStream, metaData);
            }
        }
    }
}
