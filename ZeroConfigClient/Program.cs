using DNS;
using mDNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace ZeroConfigClient
{
    class Program
    {
        static string Usage = @"{0} [-h] [-a] [-p] [-A] [-P] [-t] QUERY ... QUERY_N
    -h : help

    -a : Listen Address     default : 0.0.0.0
    -p : Listen Port        default : RAND(50000~59999)
    -A : mCAST Address      default : 224.0.0.251
    -P : mCAST Port         default : 5353
    -t : Wait Time (sec)    default : 3

    QUERY : splited by dot('.') string array 
        ex) _http._tcp.local _audio.tcp.local 
";

        static void Main(string[] args)
        {
            // Trace write console
            Trace.Listeners.Add(new ConsoleTraceListener(true));

            List<string> queries = new List<string>();
            IPAddress listen_ip = null;
            int listen_port = 0;
            IPAddress multicast_ip = null;
            int multicast_port = 0;
            int timeout_sec = 3;

            if (0 == args.Length)
                Trace.TraceError($"Usage={string.Format(Usage, System.AppDomain.CurrentDomain.FriendlyName)}");

            try
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (0 == args[i].IndexOf("-h")|| 0 == args[i].IndexOf("-help") || 0 == args[i].IndexOf("/?"))
                    {
                        Trace.TraceError($"Usage={string.Format(Usage, System.AppDomain.CurrentDomain.FriendlyName)}");
                        return;
                    }
                    else if (0 == args[i].IndexOf("-a"))
                    {
                        ++i;
                        listen_ip = IPAddress.Parse(args[i]);
                        continue;
                    }
                    else if (0 == args[i].IndexOf("-p"))
                    {
                        ++i;
                        listen_port = Int32.Parse(args[i]);
                        continue;
                    }
                    if (0 == args[i].IndexOf("-A"))
                    {
                        ++i;
                        multicast_ip = IPAddress.Parse(args[i]);
                        continue;
                    }
                    else if (0 == args[i].IndexOf("-P"))
                    {
                        ++i;
                        multicast_port = Int32.Parse(args[i]);
                        continue;
                    }
                    else if (0 == args[i].IndexOf("-t"))
                    {
                        ++i;
                        timeout_sec = Int32.Parse(args[i]);
                        continue;
                    }
                    else
                    {
                        queries.Add(args[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message);
                Trace.TraceError($"Usage={string.Format(Usage, System.AppDomain.CurrentDomain.FriendlyName)}");
                return;
            }

            if (0 == queries.Count())
            {
                Trace.TraceError($"Usage={string.Format(Usage, System.AppDomain.CurrentDomain.FriendlyName)}");
                return;
            }


            if (0 == listen_port)
            {
                for (; ; )
                {
                    Random rand = new Random((int)(DateTime.Now - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Ticks);
                    listen_port = rand.Next(50000, 59999);
                    if (true == macro.IsTcpPortAvailable(listen_port))
                        break;
                }
            }

            foreach (string q in queries)
            {
                mdns_discover discover = mdns_discover.CreateDiscover(multicast_ip, multicast_port, listen_ip, listen_port, timeout_sec);

                discover.UnmarshalCompleated += (s, endpoint, response) =>
                {
                    Trace.WriteLine($"endpoint={endpoint.ToString()}");

                    foreach (var rr in get_rr(response))
                    {
                        Trace.WriteLine(string.Join("\t", rr.Print()));
                    }

                };
                discover.Verbose(true);
                discover.Discover(q);
            }
        }

        static Func<dns, dns_resource_record[]> get_rr = (dns) =>
        {
            List<dns_resource_record> RRs = new List<dns_resource_record>();

            foreach (dns_resource_record rr in dns.AnswerRRs)
            {
                    RRs.Add(rr);
            }
            foreach (dns_resource_record rr in dns.AuthorityRRs)
            {
                    RRs.Add(rr);
            }
            foreach (dns_resource_record rr in dns.AdditionalRRs)
            {
                    RRs.Add(rr);
            }

            return RRs.ToArray();
        };

       
    }
}
