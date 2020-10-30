using DNS;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace mDNS
{
    public class mdns_resolver
    {
        public event DataRecivedEventHandler DataRecived;
        public event UnmarshalCompleatedEventHandler<EndPoint, dns> UnmarshalCompleated;
        public event MatchedEventHandler<dns> Matched;
        public event SocketErrorEventHandler SocketErrorRaised;
        public event EventHandler<System.EventArgs> ServiceTerminated;

        IPAddress multicast_group = IPAddress.Parse("224.0.0.251");
        IPAddress multicast_interface = IPAddress.Any;
        IPAddress listen_ip = IPAddress.Any;
        int listen_port = 5353;
        string[] queries = null;

        public IPAddress MCastGroup { get { return this.multicast_group; } }
        public IPAddress MCastInterface { get { return this.multicast_interface; } }
        public IPAddress ListenAddress { get { return this.listen_ip; } }
        public int ListenPort { get { return this.listen_port; } }
        public string[] Queries { get { return this.queries; } }

        public void Listen()
        {
            System.Net.Sockets.Socket mcastSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

            IPEndPoint endpoint = new IPEndPoint(listen_ip, listen_port);
            mcastSocket.Bind(endpoint);

            MulticastOption multicastOption = new MulticastOption(multicast_group, multicast_interface);

            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);
            
            for (; ; )
            {
                byte[] buf = new byte[1 << 9];

                var readEvent = new AutoResetEvent(false);
                var recieveArgs = new SocketAsyncEventArgs()
                {
                    UserToken = readEvent,
                    RemoteEndPoint = endpoint,
                };
                recieveArgs.SetBuffer(buf, 0, buf.Length);
                recieveArgs.Completed += (s, e) =>
                {
                    var are = (AutoResetEvent)e.UserToken;
                    are.Set();
                };
                mcastSocket.ReceiveFromAsync(recieveArgs); // recive async

                //Wait for recieve
                if (false == readEvent.WaitOne(int.MaxValue))
                {
                    goto EXIT_LOOP;
                }
                if (recieveArgs.BytesTransferred == 0)
                {
                    if (recieveArgs.SocketError != SocketError.Success)
                    {
                        this.SocketErrorRaised?.Invoke(this, recieveArgs.SocketError);
                    }
                    goto EXIT_LOOP;
                }

                this.DataRecived?.Invoke(this, recieveArgs.BytesTransferred, buf);

                using (MemoryStream stream = new MemoryStream(buf, 0, recieveArgs.BytesTransferred))
                {
                    dns dns_response = dns.Unmarshal(stream.ToArray());

                    this.UnmarshalCompleated?.Invoke(this, recieveArgs.RemoteEndPoint, dns_response);


                    if (HeaderFlag.Response == (HeaderFlag)((UInt16)HeaderFlag.Response & dns_response.Header.FLAG))
                    {
                        // raise error
                    }


                    bool found = false;
                    foreach (dns_question_section question in dns_response.Questions)
                    {
                        string resolve_query = string.Join(".", question.NAME);
                        foreach (string query in this.Queries)
                        {
                            if (0 <= query.IndexOf(resolve_query))
                            {
                                found = true;
                            }
                        }
                    }

                    if (true == found)
                    {
                        this.Matched?.Invoke(this, dns_response);

                        System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

                        socket.SendTo(dns_response.Marshal(), recieveArgs.RemoteEndPoint);
                    }
                }
            }
        EXIT_LOOP:

            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, multicastOption);

            this.ServiceTerminated?.Invoke(this, new EventArgs());
        }
        public static mdns_resolver CreateResolver(IPAddress multicast_group, IPAddress multicast_interface, IPAddress listen_ip, int listen_port, params string[] queries)
        {
            mdns_resolver resolver = new mdns_resolver();

            if (null != multicast_group)
                resolver.multicast_group = multicast_group;
            if (null != multicast_interface)
                resolver.multicast_interface = multicast_interface;
            if (null != listen_ip)
                resolver.listen_ip = listen_ip;
            if (0 < listen_port)
                resolver.listen_port = listen_port;

            resolver.queries = queries;

            return resolver;
        }
    }
}
