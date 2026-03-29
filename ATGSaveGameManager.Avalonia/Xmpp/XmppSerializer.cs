using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ATGSaveGameManager.Avalonia.Xmpp;

public static class XmppSerializer
{
    public static string Serizalize<T>(T obj)
    {
        XDocument doc = new XDocument();
        using (var writer = doc.CreateWriter())
        {
            // write xml into the writer
            var serializer = new DataContractSerializer(obj.GetType());
            serializer.WriteObject(writer, obj);
        }

        return doc.ToString();
    }

    public static XElement ToXElement<T>(T obj)
    {
        var serializer = new XmlSerializer(typeof(T));

        using var ms = new MemoryStream();
        serializer.Serialize(ms, obj);
        ms.Position = 0;

        return XElement.Load(ms);
    }

    public static T Deserialize<T>(XDocument doc)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = doc.CreateReader();
        return (T)serializer.Deserialize(reader);
    }
}