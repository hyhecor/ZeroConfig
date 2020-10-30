using System.Collections.Generic;
using System.IO;
using System.Net;

namespace DNS
{
    /// <summary>
    /// A (a host address)
    /// </summary>
    public class dns_resource_record_host_address : interface_dns_marshalable, interface_dns_printable
    {
        public byte[] HostAddress;

        public dns_resource_record_host_address()
        {

        }
        public dns_resource_record_host_address(IPAddress address)
        {
            this.HostAddress = address.GetAddressBytes();
        }


        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.HostAddress);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_host_address Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_host_address self = new dns_resource_record_host_address();

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
