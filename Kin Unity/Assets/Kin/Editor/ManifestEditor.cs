using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;

namespace Kin
{
    public class ModifyUnityAndroidAppManifest : IPostGenerateGradleAndroidProject
    {
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            var androidManifest = new AndroidManifest(GetManifestPath(basePath));

            // Add these two attributes to the application element in the AndroidManifset
            XmlAttribute ReplaceBackupAttr = androidManifest.GenerateAttribute(
            "tools", "replace", "android:allowBackup", androidManifest.ToolsXmlNamespace);

            XmlAttribute AllowBackupAttr = androidManifest.GenerateAttribute(
            "android", "allowBackup", "true", androidManifest.AndroidXmlNamespace);

            androidManifest.SetAttribute(ReplaceBackupAttr);
            androidManifest.SetAttribute(AllowBackupAttr);
            androidManifest.Save();
        }

        public int callbackOrder { get { return 0; } }

        private string GetManifestPath(string basePath)
        {
                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                return pathBuilder.ToString();
        }
    }


    internal class AndroidXmlDocument : XmlDocument
    {
        private string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        public readonly string ToolsXmlNamespace = "http://schemas.android.com/tools";
        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
            nsMgr.AddNamespace("tools", ToolsXmlNamespace);
        }

        public string Save()
        {
            return SaveAs(m_Path);
        }

        public string SaveAs(string path)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }


    internal class AndroidManifest : AndroidXmlDocument
    {
        internal readonly XmlElement ApplicationElement;

        public AndroidManifest(string path) : base(path)
        {
            ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        internal XmlAttribute GenerateAttribute(string prefix, string key, string value, string XmlNamespace)
        {
            XmlAttribute attr = CreateAttribute(prefix, key, XmlNamespace);
            attr.Value = value;
            return attr;
        }

        internal void SetAttribute(XmlAttribute Attribute)
        {
            ApplicationElement.Attributes.Append(Attribute);
        }
    }
}