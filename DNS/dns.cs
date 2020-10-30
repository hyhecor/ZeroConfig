using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DNS
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc1035 
    /// </summary>
    public class dns
    {
        /// <summary>
        /// Header
        /// </summary>
        public dns_header Header;
        /// <summary>
        /// Question Sections
        /// </summary> 
        public dns_question_section[] Questions;
        /// <summary>
        /// Answer Resource Records
        /// </summary>
        public dns_resource_record[] AnswerRRs;
        /// <summary>
        /// Authority Resource Records
        /// </summary>
        public dns_resource_record[] AuthorityRRs;
        /// <summary>
        /// Additional Resource Records
        /// </summary>
        public dns_resource_record[] AdditionalRRs;

        public dns()
        {
            init();
        }
        void init()
        {
            this.Header = new dns_header();

            this.Questions = new dns_question_section[0];

            this.AnswerRRs = new dns_resource_record[0];
            this.AuthorityRRs = new dns_resource_record[0];
            this.AdditionalRRs = new dns_resource_record[0];
        }

        public byte[] Marshal()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var header_bytes = this.Header.Marshal();
                stream.Write(header_bytes, 0, header_bytes.Length);

                // Questions
                foreach (dns_question_section section in this.Questions)
                {
                    var section_bytes = section.Marshal();

                    stream.Write(section_bytes, 0, section_bytes.Length);
                }

                //if (HeaderFlag.AA == (HeaderFlag)((int)HeaderFlag.AA & this.Header.FLAG))
                //{
                foreach (dns_resource_record answer in this.AnswerRRs)
                {
                    var bytes = answer.Marshal();
                    stream.Write(bytes, 0, bytes.Length);
                }
                foreach (dns_resource_record nameserver in this.AuthorityRRs)
                {
                    var bytes = nameserver.Marshal();
                    stream.Write(bytes, 0, bytes.Length);
                }
                foreach (dns_resource_record record in this.AdditionalRRs)
                {
                    var bytes = record.Marshal();
                    stream.Write(bytes, 0, bytes.Length);
                }
                //}

                return stream.ToArray();
            }
        }

        public static dns Unmarshal(byte[] data)
        {
            dns new_one = new dns();

            using (MemoryStream stream = new MemoryStream(data, 0, data.Length))
            {
                stream.Position = 0;
                //Header
                new_one.Header = dns_header.Unmarshal(stream);
                //Questions
                new_one.Questions = new dns_question_section[new_one.Header.QDCOUNT];
                new_one.Questions[0] = dns_question_section.Unmarshal(stream);

                //if (HeaderFlag.AA == (HeaderFlag)((int)HeaderFlag.AA & new_one.Header.FLAG))
                //{
                    new_one.AnswerRRs = new dns_resource_record[new_one.Header.ANCOUNT];
                    new_one.AuthorityRRs = new dns_resource_record[new_one.Header.NSCOUNT];
                    new_one.AdditionalRRs = new dns_resource_record[new_one.Header.ARCOUNT];

                    for (int idx = 0; idx < new_one.Header.ANCOUNT; ++idx)
                        new_one.AnswerRRs[idx] = dns_resource_record.Unmarshal(stream);
                    for (int idx = 0; idx < new_one.Header.NSCOUNT; ++idx)
                        new_one.AuthorityRRs[idx] = dns_resource_record.Unmarshal(stream);
                    for (int idx = 0; idx < new_one.Header.ARCOUNT; ++idx)
                        new_one.AdditionalRRs[idx] = dns_resource_record.Unmarshal(stream);
                //}

                return new_one;
            }
        }

        public dns AddQuestion(TypeValues type, ClassValues @class, params string[] name)
        {
            dns_question_section question = new dns_question_section()
            {
                NAME = name,
                TYPE = (Int16)type,
                CLASS = (Int16)@class,
            };
            List<dns_question_section> l = new List<dns_question_section>();
            foreach (dns_question_section q in this.Questions)
                l.Add(q);

            l.Add(question);

            this.Header.QDCOUNT = (UInt16)l.Count();
            this.Questions = l.ToArray();
            return this;
        }
        public dns AddAnswerRR(params dns_resource_record[] RRs)
        {
            List<dns_resource_record> l = new List<dns_resource_record>();
            foreach (dns_resource_record q in this.AnswerRRs)
                l.Add(q);

            foreach(dns_resource_record rr in RRs)
                l.Add(rr);

            this.Header.ANCOUNT = (UInt16)l.Count();
            this.AnswerRRs = l.ToArray();
            return this;
        }
        public dns AddAuthorityRR(params dns_resource_record[] RRs)
        {
            List<dns_resource_record> l = new List<dns_resource_record>();
            foreach (dns_resource_record q in this.AuthorityRRs)
                l.Add(q);

            foreach (dns_resource_record rr in RRs)
                l.Add(rr);

            this.Header.NSCOUNT = (UInt16)l.Count();
            this.AuthorityRRs = l.ToArray();
            return this;
        }

        public dns AddAdditionalRR(params dns_resource_record[] RRs)
        {
            List<dns_resource_record> l = new List<dns_resource_record>();
            foreach (dns_resource_record q in this.AdditionalRRs)
                l.Add(q);

            foreach (dns_resource_record rr in RRs)
                l.Add(rr);

            this.Header.ARCOUNT = (UInt16)l.Count();
            this.AdditionalRRs = l.ToArray();
            return this;
        }

        //public string[] Print()
        //{
        //    List<string> print = new List<string>();

        //    print.Add($"{this.Header.ID}");
        //    //print.Add($"Flag={BitConverter.ToString((BitConverter.GetBytes(this.Header.FLAG)))}");
        //    List<string> flag = new List<string>();
        //    if (HeaderFlag.Response == (HeaderFlag)((UInt16)HeaderFlag.Response & this.Header.FLAG))
        //    {
        //        flag.Add( "#Response");
        //    }
        //    else
        //    {
        //        flag.Add("#Query");
        //    }
        //    if (HeaderFlag.AA == (HeaderFlag)((UInt16)HeaderFlag.AA & this.Header.FLAG))
        //    {
        //        flag.Add("#Authoritative");
        //    }
        //    print.Add($"Flag={string.Join(" ", flag)}");
        //    print.Add($"Questions={this.Header.QDCOUNT}");
        //    print.Add($"AnswerRRs={this.Header.ANCOUNT}");
        //    print.Add($"AuthorityRRs={this.Header.NSCOUNT}");
        //    print.Add($"AdditionalRRs={this.Header.ARCOUNT}");

        //    int n = 0;
        //    //foreach (dns_question_section question in this.Questions)
        //    //{
        //    //    print.Add($"Question {++n}/{this.Questions.Length}", question.Print());
        //    //}
        //    //n = 0;
        //    //foreach (dns_resource_record rr in this.AnswerRRs)
        //    //{
        //    //    print.Add($"Answer RR {++n}/{this.Questions.Length}", rr.Print());
        //    //}
        //    //n = 0;
        //    //foreach (dns_resource_record rr in this.AuthorityRRs)
        //    //{
        //    //    print.Add($"Authority RR {++n}/{this.Questions.Length}", rr.Print());
        //    //}
        //    //n = 0;
        //    //foreach (dns_resource_record rr in this.AdditionalRRs)
        //    //{
        //    //    print.Add($"Additional RR {++n}/{this.Questions.Length}", rr.Print());
        //    //}

        //    return print.ToArray();
        //}
    }
}
