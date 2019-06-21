using System;
using System.IO;
using System.Xml.Serialization;

namespace LibNitro.Intermediate
{
    public class CreateInfo
    {
        public CreateInfo()
        {
            User = Environment.UserName;
            Host = Environment.UserDomainName;
            Date = DateTime.Now;
        }

        [XmlAttribute("user")] public string User;

        [XmlAttribute("host")] public string Host;

        [XmlAttribute("date")] public DateTime Date;

        [XmlAttribute("source")] public string Source;
    }

    public class GeneratorInfo
    {
        [XmlAttribute("name")] public string Name;

        [XmlAttribute("version")] public string Version;
    }

    public abstract class Intermediate<T1, T2> where T2 : IntermediateBody, new()
    {
        [XmlElement("head")] public IntermediateHead Head = new IntermediateHead();

        [XmlElement("body")] public T2 Body;

        [XmlAttribute("version")] public string Version = "1.6.0";

        public static T1 Read(string path)
        {
            MemoryStream m = new MemoryStream(File.ReadAllBytes(path));

            XmlSerializer deserializer = new XmlSerializer(typeof(T1));
            TextReader reader = new StreamReader(m);
            T1 intermediate = (T1)deserializer.Deserialize(reader);
            reader.Close();

            return intermediate;
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(typeof(T1));
            using (TextWriter writer = new StreamWriter(m))
            {
                serializer.Serialize(writer, this);
                writer.Close();
            }
            return m.ToArray();
        }
    }

    public class IntermediateHead
    {
        public IntermediateHead()
        {
            CreateInfo = new CreateInfo();
        }

        [XmlElement("create")] public CreateInfo CreateInfo;

        [XmlElement("title")] public string Title;

        [XmlElement("generator")] public GeneratorInfo GeneratorInfo;
    }

    public abstract class IntermediateBody
    {
        protected IntermediateBody()
        {

        }
    }
}
