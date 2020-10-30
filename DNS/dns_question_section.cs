using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// 4.1.2. Question section format
    /// </summary>
    public class dns_question_section : interface_dns_marshalable, interface_dns_printable
    {
        public string[] NAME;
        public Int16 TYPE;
        public Int16 CLASS;

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                macro.write(stream, this.NAME);
                macro.write(stream, this.TYPE);
                macro.write(stream, this.CLASS);

                return stream.ToArray();
            }
        }

        public static dns_question_section Unmarshal(MemoryStream stream)
        {
            dns_question_section self = new dns_question_section();
            macro.read(out self.NAME,  stream, stream.Length - stream.Position);
            macro.read(out self.TYPE, stream);
            macro.read(out self.CLASS, stream);

            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add(string.Join(".", this.NAME));
            print.Add($"{(TypeValues)this.TYPE}");
            print.Add($"{(ClassValues)this.CLASS}");

            return print.ToArray();
        }
    }

}
