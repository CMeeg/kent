using System.Collections.Generic;
using CMS.DataEngine;
using CMS.Modules;

namespace Kent.Cli.Modules
{
    internal class ModuleDataProvider
    {
        private static readonly string[] supportedObjectTypes = {
            "cms.documenttype",
            "cms.webpart",
            "cms.webpartcategory",
            "cms.formusercontrol",
            "cms.settingskey",
            "cms.settingscategory",
            "cms.systemtable"
        };

        private readonly ResourceInfo resource;
        private readonly string codeNamePrefix;

        public IEnumerable<string> SupportedObjectTypes => supportedObjectTypes;

        public ModuleDataProvider(ResourceInfo module)
        {
            resource = module;
            codeNamePrefix = $"{module.ResourceName}.";
        }

        public ObjectQuery GetModuleObjects(string objectType)
        {
            switch (objectType.ToLowerInvariant())
            {
                case "cms.documenttype":
                    return GetModulePageTypes();
                case "cms.formusercontrol":
                    return GetModuleFormControls();
                case "cms.settingscategory":
                    return GetModuleSettingsCategories();
                case "cms.settingskey":
                    return GetModuleSettingsKeys();
                case "cms.systemtable":
                    return GetModuleSystemTables();
                case "cms.webpart":
                    return GetModuleWebParts();
                case "cms.webpartcategory":
                    return GetModuleWebPartCategories();
                default:
                    return null;
            }
        }
        private ObjectQuery GetModulePageTypes()
        {
            return new ObjectQuery("cms.documenttype")
                .WhereEquals("ClassResourceID", resource.ResourceID);
        }

        private ObjectQuery GetModuleWebParts()
        {
            return new ObjectQuery("cms.webpart")
                .WhereEquals("WebPartResourceID", resource.ResourceID)
                .Or(new WhereCondition()
                    .WhereNull("WebPartResourceID")
                    .And()
                    .WhereStartsWith("WebPartName", codeNamePrefix)
                );
        }

        private ObjectQuery GetModuleWebPartCategories()
        {
            return new ObjectQuery("cms.webpartcategory")
                .WhereStartsWith("CategoryName", codeNamePrefix);
        }

        private ObjectQuery GetModuleFormControls()
        {
            return new ObjectQuery("cms.formusercontrol")
                .WhereEquals("UserControlResourceID", resource.ResourceID)
                .Or(new WhereCondition()
                    .WhereNull("UserControlResourceID")
                    .And()
                    .WhereStartsWith("UserControlCodeName", codeNamePrefix)
                );
        }

        private ObjectQuery GetModuleSettingsKeys()
        {
            return new ObjectQuery("cms.settingskey")
                .WhereIn("KeyCategoryID", GetModuleSettingsCategories().AsIDQuery());
        }

        private ObjectQuery GetModuleSettingsCategories()
        {
            return new ObjectQuery("cms.settingscategory")
                .WhereEquals("CategoryResourceID", resource.ResourceID)
                .Or(new WhereCondition()
                    .WhereNull("CategoryResourceID")
                    .And()
                    .WhereStartsWith("CategoryName", codeNamePrefix)
                );
        }

        private ObjectQuery GetModuleSystemTables()
        {
            return new ObjectQuery("cms.systemtable")
                .WhereEquals("ClassResourceID", resource.ResourceID);
        }
    }
}
