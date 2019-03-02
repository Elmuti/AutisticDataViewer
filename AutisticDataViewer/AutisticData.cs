using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace AutisticDataViewer
{
    [Serializable]
    public class AutisticData
    {
        public string Folder { get; set; }
        public int Entry { get; set; }
        public string EntryName { get; set; }
        public string Source { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string RealName { get; set; }
        public string DateAdded { get; set; }
        public string Notes { get; set; }

        public AutisticData()
        {
        }

        public AutisticData(string en)
        {
            EntryName = en;
        }
    }

    public static class ObjectCopier
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public static class DataParser
    {
        public static void PrintToFile(string path, Dictionary<string, List<AutisticData>> data)
        {
            List<string> lines = new List<string>();
            lines.Add(@"This File was created with Autistic Data Viewer - https://github.com/Elmuti/AutisticDataViewer");
            lines.Add("File Created: " + DateTime.Now.ToString());
            lines.Add("");
            foreach (KeyValuePair<string, List<AutisticData>> pair in data)
            {
                lines.Add(pair.Key + "\n\n");
                lines.Add("");
                int ce = 1;
                foreach (AutisticData d in pair.Value)
                {
                    lines.Add(ce.ToString() + ")\n");
                    lines.Add(d.EntryName + "\n");
                    lines.Add(d.Source + "\n");
                    lines.Add(d.Author + "\n");
                    lines.Add(d.Keywords + "\n");
                    lines.Add(d.RealName + "\n");
                    lines.Add(d.DateAdded + "\n");
                    lines.Add(d.Notes + "\n\n\n");
                    lines.Add("");
                    lines.Add("");
                    ce++;
                }
            }
            File.WriteAllLines(path, lines.ToArray());
        }
    }
}
