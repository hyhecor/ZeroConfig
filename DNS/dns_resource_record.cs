using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// 4.1.3. Resource record format
    /// </summary>
    public class dns_resource_record : interface_dns_marshalable, interface_dns_printable
    {
        public string[] NAME;
        public UInt16 TYPE;
        public UInt16 CLASS;
        public UInt32 TTL;
        public UInt16 RDLENGTH;
        public object RDATA;

        static public Dictionary<TypeValues, Func<MemoryStream, int, interface_dns_marshalable>> unmashal_table = new Dictionary<TypeValues, Func<MemoryStream, int, interface_dns_marshalable>>()
        {
            { TypeValues.UNDEFINED, dns_resource_record_undefined.Unmarshal },
            { TypeValues.A, dns_resource_record_host_address.Unmarshal },
            { TypeValues.PTR, dns_resource_record_domain_name_pointer.Unmarshal },
            { TypeValues.TXT, dns_resource_record_txt.Unmarshal },
            { TypeValues.AAAA, dns_resource_record_host_address_ipv6.Unmarshal },
            { TypeValues.SRV, dns_resource_record_server_selection.Unmarshal },
        };

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.NAME);
                macro.write(stream, this.TYPE);
                macro.write(stream, this.CLASS);
                macro.write(stream, this.TTL);
                macro.write(stream, this.RDLENGTH);
                macro.write(stream, ((interface_dns_marshalable)RDATA).Marshal());

                return stream.ToArray();
            }
        }
        public static dns_resource_record Unmarshal(MemoryStream stream)
        {
#if false
            byte[] b = new byte[stream.Length - stream.Position];
            stream.Read(b, 0, b.Length);
            Debug.WriteLine(BitConverter.ToString(b), "RR");
            stream.Position = stream.Length - b.Length;
#endif
            dns_resource_record self = new dns_resource_record();

            macro.read(out self.NAME, stream, stream.Length - stream.Position);
            macro.read(out self.TYPE, stream);
            macro.read(out self.CLASS, stream);
            macro.read(out self.TTL, stream);
            macro.read(out self.RDLENGTH, stream);


            Func<MemoryStream, int, interface_dns_marshalable> unmarshal_func = null;
            if (unmashal_table.ContainsKey((TypeValues)self.TYPE))
                unmarshal_func = unmashal_table[(TypeValues)self.TYPE];
            else
                unmarshal_func = unmashal_table[TypeValues.UNDEFINED];

            self.RDATA = unmarshal_func(stream, self.RDLENGTH);

            return self;
        }


        public static dns_resource_record Create<T>(string[] host_name, T dns_marchal, ClassValues @class = ClassValues.IN, UInt16 ttl = 10)
            where T : interface_dns_marshalable
        {
            dns_resource_record self = new dns_resource_record();

            self.NAME = host_name;
            self.TYPE = (UInt16)def.ConverType(dns_marchal);
            self.CLASS = (UInt16)@class;
            self.TTL = ttl;
            self.RDLENGTH = (UInt16)dns_marchal.Marshal().Length;
            self.RDATA = dns_marchal;

            return self;
        }


        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add(string.Join(".", this.NAME));
            print.Add($"{(TypeValues)this.TYPE}");
            print.Add($"{(ClassValues)this.CLASS}");
            print.Add($"{this.TTL}");
            print.Add($"{this.RDLENGTH}");

            foreach (string s in ((interface_dns_printable)this.RDATA).Print())
            {
                print.Add(s);
            }

            return print.ToArray();
        }
    }
}
