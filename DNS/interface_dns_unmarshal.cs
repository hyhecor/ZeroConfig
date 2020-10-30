namespace DNS
{
    public interface interface_dns_unmarshal
    {
        interface_dns_marshalable Unmarshal(System.IO.MemoryStream stream);
    }
    public interface interface_dns_unmarshal_with_length
    {
        interface_dns_marshalable Unmarshal(System.IO.MemoryStream stream, int length);
    }
}
