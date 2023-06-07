using System.Globalization;
using System.Xml.Linq;
using PowerSupplies.Core;

namespace PowerSupplies.Xml;

public static class CurrentPointExtensions
{
    public static void Save(this IEnumerable<IReadOnlyCurrentPoint> collection, string path)
    {
        var doc = new XDocument();
        var elements = new XElement("points");

        foreach (var t in collection)
        {
            string arg = string.Format(CultureInfo.InvariantCulture, "{0:0.000}", t.Time.TotalSeconds);
            string val = string.Format(CultureInfo.InvariantCulture, "{0:0.000}", t.Value);

            elements.Add(new XElement("point", new XAttribute("time", arg), new XAttribute("value", val)));
        }
        doc.Add(elements);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        doc.Save(path);
    }

    public static void LoadFromFile(this ICollection<IReadOnlyCurrentPoint> collection, string path)
    {
        collection.Clear();

        var doc = XDocument.Load(path);

        if (doc.Root != null)
        {
            foreach (var t in doc.Root.Elements())
            {
                double time = double.Parse(t.Attribute("time")?.Value ?? "0.0", CultureInfo.InvariantCulture);
                double value = double.Parse(t.Attribute("value")?.Value ?? "0.0", CultureInfo.InvariantCulture);
                collection.Add(new CurrentPoint(TimeSpan.FromSeconds(time), value));
            }
        }
    }
}