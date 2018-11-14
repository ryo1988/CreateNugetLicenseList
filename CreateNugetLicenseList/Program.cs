using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CreateNugetLicenseList
{
    class Program
    {
        static void Main(string[] args)
        {
            var nupkgs = Directory.GetFiles(args[0], "*.nupkg", SearchOption.AllDirectories);
            var xamls = nupkgs
                .Select(o =>
                {
                    var zipArchive = ZipFile.OpenRead(o);
                    var nuspecEntry = zipArchive.Entries.Single(oo => oo.Name.EndsWith(".nuspec"));
                    using (var nuspec = nuspecEntry.Open())
                    {
                        var xmlDocument = new XmlDocument();
                        xmlDocument.Load(nuspec);
                        var metadata = xmlDocument.DocumentElement.GetElementsByTagName("metadata").OfType<XmlElement>().Single();
                        var id = metadata.GetElementsByTagName("id").OfType<XmlElement>().SingleOrDefault()?.InnerText;
                        var version = metadata.GetElementsByTagName("version").OfType<XmlElement>().SingleOrDefault()?.InnerText;
                        var licenseUrl = metadata.GetElementsByTagName("licenseUrl").OfType<XmlElement>().SingleOrDefault()?.InnerText;

                        return new {Id = id, Version = version, LicenseUrl = licenseUrl};
                    }
                })
                .Select(o => $"<TextBlock HorizontalAlignment=\"Center\">\r\n    <Run Text=\"{o.Id} {o.Version} \" /><Hyperlink NavigateUri=\"{o.LicenseUrl}\" RequestNavigate=\"Hyperlink_OnRequestNavigate\">{o.LicenseUrl}</Hyperlink>\r\n</TextBlock>");
            var xaml = string.Join(Environment.NewLine, xamls);
            Console.WriteLine(xaml);
        }
    }
}
