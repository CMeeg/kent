using CMS.Base;
using CMS.DataEngine;

namespace Kent.Cli
{
    internal class CmsInitialiser
    {
        private bool initialised;

        public void Initialise(string cmsAppPath)
        {
            if (initialised)
            {
                return;
            }

            SystemContext.WebApplicationPhysicalPath = cmsAppPath;

            CMSApplication.Init();

            initialised = true;
        }
    }
}
