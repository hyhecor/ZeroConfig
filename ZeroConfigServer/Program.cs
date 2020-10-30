using DNS;
using mDNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace ZeroConfigServer
{
    class Program
    {
        static string Usage = @"{0} [-h] [-a] [-p] [-G] [-I]  QUERY ... QUERY_N
    -h : help

    -a : Listen Address     default : 0.0.0.0
    -p : Listen Port        default : RAND(50000~59999)
    -G : mCAST Group        default : 224.0.0.251
    -I : mCAST Interface    default : 0.0.0.0

    QUERY : splited by dot('.') string array 
        ex) _http._tcp.local _audio.tcp.local 
";

        static void Main(string[] args)
        {
            // Trace write console
            Trace.Listeners.Add(new ConsoleTraceListener(true));

            List<string> queries = new List<string>();

            IPAddress multicast_group = null;
            IPAddress multicast_interface = null;
            //int multicast_port = 0;

            IPAddress listen_ip = null;
            int listen_port = 0;
            //int timeout_sec = 0;

            if (0 == args.Length)
                Trace.TraceError($"Usage={string.Format(Usage, System.AppDomain.CurrentDomain.FriendlyName)}");

            try
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (0 == args[i].IndexOf("-h") || 0 == args[i].IndexOf("-help") || 0 == args[i].IndexOf("/?"))
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
                    if (0 == args[i].IndexOf("-G"))
                    {
                        ++i;
                        multicast_group = IPAddress.Parse(args[i]);
                        continue;
                    }
                    else if (0 == args[i].IndexOf("-I"))
                    {
                        ++i;
                        multicast_interface = IPAddress.Parse(args[i]);
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

            mdns_resolver resolver = mdns_resolver.CreateResolver(multicast_group, multicast_interface, listen_ip, listen_port, queries.ToArray());

            Trace.WriteLine($"mCAST Group={resolver.MCastGroup}");
            Trace.WriteLine($"mCAST Interface={resolver.MCastInterface}");
            Trace.WriteLine($"Address={resolver.ListenAddress}");
            Trace.WriteLine($"Port={resolver.ListenPort}");
            foreach (string q in resolver.Queries)
                Trace.WriteLine($"Query={q}");

            resolver.UnmarshalCompleated += (s, endpoint, response) =>
            {
                Trace.WriteLine($"incoming={endpoint.ToString()}\t{string.Join("\t", response.Questions[0].Print())}");
            };

            string processor_name = GetProcessorName();
            ulong total_physical_memory = GetTotalPhysicalMemory();
            OperatingSystem os_version = GetOsVersion();
            string computer_name = GetComputerName();

            resolver.Matched += (s, response) =>
            {
                string ip_address = GetIPAddress();
                string ipv6_address = GetIPAddressIPv6();
                string mac_address = GetMacAddress();

                string[] local_name = new string[] { computer_name, "local" };
                string[] domain_name = new string[] { computer_name, "_tomatopos", "_tcp", "local", };

                dns_resource_record_host_address rr_host_address = new dns_resource_record_host_address(IPAddress.Parse(ip_address));
                dns_resource_record_server_selection rr_domain_name_selection = new dns_resource_record_server_selection()
                {
                    Port = 8080,
                    Target = domain_name,
                };
                dns_resource_record_txt rr_txt = new dns_resource_record_txt()
                {
                    TextStrings = new string[]
                    {
                        $"computer_name={computer_name}",
                        $"os_version={os_version.ToString()}",
                        $"processor_name={processor_name}",
                        $"total_physical_memory={total_physical_memory}",
                        $"mac_address={mac_address}",
                        $"ip_address={ip_address}",
                        $"ipv6_address={ipv6_address}",
                    }
                };

                response.Header.FLAG = (UInt16)(response.Header.FLAG | (UInt16)HeaderFlag.Response);
                response.AddAnswerRR
                (
                    dns_resource_record.Create(local_name, rr_host_address)
                );
                response.AddAdditionalRR
                (
                    dns_resource_record.Create(domain_name, rr_domain_name_selection),
                    dns_resource_record.Create(domain_name, rr_txt)
                );
            };

            resolver.Listen();
        }

        static Func<dns, TypeValues, dns_resource_record[]> get_rr_type = (dns, type) =>
        {
            List<dns_resource_record> RRs = new List<dns_resource_record>();
            foreach (dns_resource_record rr in dns.AnswerRRs)
            {
                if (type == (TypeValues)rr.TYPE)
                    RRs.Add(rr);
            }
            foreach (dns_resource_record rr in dns.AuthorityRRs)
            {
                if (type == (TypeValues)rr.TYPE)
                    RRs.Add(rr);
            }
            foreach (dns_resource_record rr in dns.AdditionalRRs)
            {
                if (type == (TypeValues)rr.TYPE)
                    RRs.Add(rr);
            }

            return RRs.ToArray();
        };


        #region GetIPAddress [IP주소가져오기]
        /// <summary>
        /// IP주소가져오기
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_NetworkAdapterConfiguration");

            string[] IpAddresses = (string[])(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                where (true == (bool)x.GetPropertyValue("IPEnabled"))
                select x.GetPropertyValue("IPAddress")
                ).FirstOrDefault();

            string IpAddress = (
                from x in IpAddresses
                where (0 <= x.IndexOf("."))
                select x
            ).FirstOrDefault();

            return IpAddress;
        }
        public static string GetIPAddressIPv6()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_NetworkAdapterConfiguration");

            string[] IpAddresses = (string[])(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                where (true == (bool)x.GetPropertyValue("IPEnabled"))
                select x.GetPropertyValue("IPAddress")
                ).FirstOrDefault();

            string IpAddress = (
                from x in IpAddresses
                where (0 <= x.IndexOf(":"))
                select x
            ).FirstOrDefault();

            return IpAddress;
        }
        #endregion

        #region GetMacAddress [MAC주소가져오기]
        /// <summary>
        /// MAC주소가져오기
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_NetworkAdapterConfiguration");

            string MACAddress = (string)(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                where (true == (bool)x.GetPropertyValue("IPEnabled"))
                select x.GetPropertyValue("MACAddress")
                ).FirstOrDefault();

            return MACAddress;
        }
        #endregion
        #region GetOsVersion
        public static OperatingSystem GetOsVersion()
        {
            return Environment.OSVersion;
        }
        #endregion
        #region GetProcessorName [CPU이름가져오기]
        /// <summary>
        /// CPU이름가져오기
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorName()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_Processor");

            string name = (string)(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                select x.GetPropertyValue("Name")
                ).FirstOrDefault();

            return name;
        }
        #endregion
        #region GetTotalPhysicalMemory [물리메모리가져오기(byte)]
        /// <summary>
        /// 물리메모리가져오기(byte)
        /// </summary>
        /// <returns></returns>
        public static UInt64 GetTotalPhysicalMemory()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_ComputerSystem");

            UInt64 capacity = (UInt64)(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                select x.GetPropertyValue("TotalPhysicalMemory")
                ).FirstOrDefault();

            return capacity;
        }
        #endregion
        #region GetComputerName [COMPUTER이름가져오기]
        /// <summary>
        /// COMPUTER이름가져오기
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                "select * from Win32_ComputerSystem");

            string name = (string)(
                from x in searcher.Get().Cast<System.Management.ManagementObject>()
                select x.GetPropertyValue("Name")
                ).FirstOrDefault();

            return name;
        }
        #endregion
    }
}
