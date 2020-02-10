using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kent.Cli.Configuration
{
    internal class CmsModulePropsLoader
    {
        public SettingsCollection Load(string propsFilePath)
        {
            if (!File.Exists(propsFilePath))
            {
                return null;
            }

            var xml = XDocument.Load(propsFilePath);

            XElement propertyGroupElem = xml.XPathSelectElement("//PropertyGroup");

            var props = new SettingsCollection();

            if (propertyGroupElem == null)
            {
                return props;
            }

            foreach (XElement propertyElem in propertyGroupElem.Descendants())
            {
                props.Add(
                    propertyElem.Name.LocalName,
                    propertyElem.Value
                );
            }

            return props;
        }
    }
}
