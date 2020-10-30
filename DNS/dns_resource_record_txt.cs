using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// TXT (Text strings)
    /// </summary>
    public class dns_resource_record_txt : interface_dns_marshalable, interface_dns_printable
    {
        public string[] TextStrings;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.TextStrings);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_txt Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_txt self = new dns_resource_record_txt();

            macro.read(out self.TextStrings, stream, length - (stream.Position - pre_position));

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            foreach (string s in this.TextStrings)
            {
                print.Add(s);
            }
            return print.ToArray();
        }
    }
}
