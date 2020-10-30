using System;
using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// SRV (Server Selection)
    /// </summary>
    public class dns_resource_record_server_selection : interface_dns_marshalable, interface_dns_printable
    {
        public UInt16 Priority;
        public UInt16 Weight;
        public UInt16 Port;
        public string[] Target;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.Priority);
                macro.write(stream, this.Weight);
                macro.write(stream, this.Port);
                macro.write(stream, this.Target);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_server_selection Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_server_selection self = new dns_resource_record_server_selection();

            macro.read(out self.Priority, stream);
            macro.read(out self.Weight, stream);
            macro.read(out self.Port, stream);
            macro.read(out self.Target, stream, length - (stream.Position - pre_position));

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add($"Priority={this.Priority}");
            print.Add($"Weight={this.Weight}");
            print.Add($"Port={this.Port}");
            print.Add($"Target={string.Join(".", this.Target)}");

            return print.ToArray();
        }
    }
}
