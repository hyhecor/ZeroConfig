using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// AAAA (a host address (IPv6))
    /// </summary>
    public class dns_resource_record_host_address_ipv6 : interface_dns_marshalable, interface_dns_printable
    {
        public byte[] HostAddress;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.HostAddress);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_host_address_ipv6 Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_host_address_ipv6 self = new dns_resource_record_host_address_ipv6();

            macro.read(out self.HostAddress, stream, length - (stream.Position - pre_position));

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add(string.Join(".", this.HostAddress));

            return print.ToArray();
        }
    }
}
