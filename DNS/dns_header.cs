using System;
using System.Collections.Generic;
using System.IO;

namespace DNS
{
    /// <summary>
    /// 4.1.1. Header section format
    /// </summary>
    public class dns_header : interface_dns_printable
    {
        public Int16 ID;
        public UInt16 FLAG;
        public UInt16 QDCOUNT, ANCOUNT, NSCOUNT, ARCOUNT;

        public dns_header()
        {
            init();
        }

        void init()
        {
            this.ID = 0;
            this.FLAG = 0;
            this.QDCOUNT = 0;
            this.ANCOUNT = 0;
            this.NSCOUNT = 0;
            this.ARCOUNT = 0;
        }

        public byte[] Marshal()
        {
            MemoryStream stream = new MemoryStream();

            macro.write(stream, this.ID);
            macro.write(stream, this.FLAG);
            macro.write(stream, this.QDCOUNT);
            macro.write(stream, this.ANCOUNT);
            macro.write(stream, this.NSCOUNT);
            macro.write(stream, this.ARCOUNT);

            return stream.ToArray();
        }

        public static dns_header Unmarshal(MemoryStream stream)
        {
            dns_header self = new dns_header();

            macro.read(out self.ID, stream);
            macro.read(out self.FLAG, stream);
            macro.read(out self.QDCOUNT, stream);
            macro.read(out self.ANCOUNT, stream);
            macro.read(out self.NSCOUNT, stream);
            macro.read(out self.ARCOUNT, stream);


            return self;
        }

        public string[] Print()
        {
            List<string> print = new List<string>();

            print.Add($"{this.ID}");
            //print.Add($"Flag={BitConverter.ToString((BitConverter.GetBytes(this.Header.FLAG)))}");
            List<string> flag = new List<string>();
            if (HeaderFlag.Response == (HeaderFlag)((UInt16)HeaderFlag.Response & this.FLAG))
            {
                flag.Add("#Response");
            }
            else
            {
                flag.Add("#Query");
            }
            if (HeaderFlag.AA == (HeaderFlag)((UInt16)HeaderFlag.AA & this.FLAG))
            {
                flag.Add("#Authoritative");
            }
            print.Add($"Flag={string.Join(" ", flag)}");
            print.Add($"Questions={this.QDCOUNT}");
            print.Add($"AnswerRRs={this.ANCOUNT}");
            print.Add($"AuthorityRRs={this.NSCOUNT}");
            print.Add($"AdditionalRRs={this.ARCOUNT}");

            return print.ToArray();
        }
    }
}
