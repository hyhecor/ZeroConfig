using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// PTR (Domain Name Pointer)
    /// </summary>
    public class dns_resource_record_domain_name_pointer : interface_dns_marshalable, interface_dns_printable
    {
        public string[] DomainName;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.DomainName);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_domain_name_pointer Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_domain_name_pointer self = new dns_resource_record_domain_name_pointer();

            macro.read(out self.DomainName, stream, stream.Position + (length - (stream.Position - pre_position)));

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add(string.Join(".", this.DomainName));

            return print.ToArray();
        }
    }
}
