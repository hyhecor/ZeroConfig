using System;
using System.Collections.Generic;
using System.IO;

namespace DNS
{
    public class dns_resource_record_undefined : interface_dns_marshalable, interface_dns_printable
    {
        public byte[] UNDEFINED_DATA;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.UNDEFINED_DATA);

                return stream.ToArray();
            }
        }

        public static dns_resource_record_undefined Unmarshal(MemoryStream stream, int length)
        {
            long pre_position = stream.Position;

            dns_resource_record_undefined self = new dns_resource_record_undefined();
            
            macro.read(out self.UNDEFINED_DATA, stream, pre_position + length);

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add($"{BitConverter.ToString(this.UNDEFINED_DATA)}");

            return print.ToArray();
        }
    }
}
