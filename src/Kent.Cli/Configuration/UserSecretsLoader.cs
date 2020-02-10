using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kent.Cli.Configuration
{
    internal class UserSecretsLoader
    {
        public SettingsCollection Load(string secretsFilePath)
        {
            if (!File.Exists(secretsFilePath))
            {
                return null;
            }

            var xml = XDocument.Load(secretsFilePath);

            XElement secretsElem = xml.XPathSelectElement("//secrets");

            var secrets = new SettingsCollection();

            if (secretsElem == null)
            {
                return secrets;
            }

            foreach (XElement secretElem in secretsElem.Descendants("secret"))
            {
                secrets.Add(
                    (string)secretElem.Attribute("name"),
                    (string)secretElem.Attribute("value")
                );
            }

            return secrets;
        }
    }
}
