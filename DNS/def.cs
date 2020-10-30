namespace DNS
{
    /// <summary>
    /// Resource Record (RR) TYPEs
    /// </summary>
    public enum TypeValues
    {
        UNDEFINED = 0,
        /// <summary>
        /// a host address
        /// </summary>
        A = 1,
        NS = 2,// an authoritative name server
        MD = 3,// a mail destination (Obsolete - use MX)
        MF = 4, // a mail forwarder (Obsolete - use MX)
        CNAME = 5, // the canonical name for an alias
        SOA = 6, // marks the start of a zone of authority
        MB = 7, // a mailbox domain name (EXPERIMENTAL)
        MG = 8, // a mail group member (EXPERIMENTAL)
        MR = 9, // a mail rename domain name (EXPERIMENTAL)
        NULL = 10, // a null RR (EXPERIMENTAL)
        /// <summary>
        /// a well known service description
        /// </summary>
        WKS = 11, 
        /// <summary>
        ///  a domain name pointer
        /// </summary>
        PTR = 12,
        /// <summary>
        /// host information
        /// </summary>
        HINFO = 13,
        /// <summary>
        /// mailbox or mail list information
        /// </summary>
        MINFO = 14,
        /// <summary>
        /// mail exchange
        /// </summary>
        MX = 15,
        /// <summary>
        /// text strings 
        /// </summary>
        TXT = 16,
        /// <summary>
        ///  a host address (IPv6)
        /// </summary>
        AAAA = 28,
        /// <summary>
        /// Server Selection
        /// </summary>
        SRV = 33,
    }

    public enum QTypeValues
    {
        AXFR = 252, // A request for a transfer of an entire zone
        MAILB = 253, // A request for mailbox-related records (MB, MG or MR)
        MAILA = 254, // A request for mail agent RRs(Obsolete - see MX)
        STAR = 255, // A request for all records (*)
    }
    /// <summary>
    /// Resource Record (RR) CLASSs
    /// </summary>
    public enum ClassValues
    {
        IN = 1,// the Internet
        CS = 2,// the CSNET class (Obsolete - used only for examples in some obsolete RFCs)
        CH = 3,// the CHAOS class
        HS = 4,// Hesiod[Dyer 87]
    }
    public enum QClassValues
    {
        STAR = 255, // any class
    }

    public enum HeaderFlag
    {
        /// <summary>
        /// Response
        /// bit 1
        /// </summary>
        Response = 1 << 15, // unset:query && set:response
        /// <summary>
        /// bit 2
        /// </summary>
        Reserved_2 = 1 << 13,
        /// <summary>
        /// Inverse
        /// bit 3
        /// </summary>
        Inverse = 1 << 14, // 0000:standard query 0100:inverse 0010/0001:not used
        /// <summary>
        /// bit 4
        /// </summary>
        Reserved_4 = 1 << 12,
        /// <summary>
        /// bit 5
        /// </summary>
        Reserved_5 = 1 << 1,
        /// <summary>
        /// Authoritative Answer
        /// bit 6
        /// </summary>
        AA = 1 << 10,
        /// <summary>
        /// Truncated Response
        /// bit 7
        /// </summary>
        TC = 1 << 9,
        /// <summary>
        ///  Recursion Desired
        /// bit 8
        /// </summary>
        RD = 1 << 8,
        /// <summary>
        /// Recursion Available
        /// bit 9
        /// </summary>
        RA = 1 << 7,
        /// <summary>
        /// bit 10
        /// </summary>
        Reserved_10 = 1 << 6,
        /// <summary>
        /// Authentic Data
        /// bit 11
        /// </summary>
        AD = 1 << 5,
        /// <summary>
        /// Checking Disabled
        /// bit 12
        /// </summary>
        CD = 1 << 4,

    }
    public class def
    {
        public static TypeValues ConverType(object obj)
        {
            TypeValues type = TypeValues.UNDEFINED;

            if (obj is dns_resource_record_domain_name_pointer)
                type = TypeValues.PTR;
            else if (obj is dns_resource_record_host_address)
                type = TypeValues.A;
            else if (obj is dns_resource_record_host_address_ipv6)
                type = TypeValues.AAAA;
            else if (obj is dns_resource_record_server_selection)
                type = TypeValues.SRV;
            else if (obj is dns_resource_record_txt)
                type = TypeValues.TXT;
            else
                type = TypeValues.UNDEFINED;
            return type;
        }
    }
}
