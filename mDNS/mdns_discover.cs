using DNS;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace mDNS
{
    public class mdns_discover
    {
        public event DataRecivedEventHandler DataRecived;
        public event UnmarshalCompleatedEventHandler<EndPoint, dns> UnmarshalCompleated;
        public event SocketErrorEventHandler SocketErrorRaised;

        static readonly IPAddress default_multicast_ip = IPAddress.Parse("224.0.0.251");
        IPAddress multicast_ip = default_multicast_ip;
        int multicast_port = 5353;

        IPAddress listen_ip = IPAddress.Any;
        int listen_port = 3535;

        int timeout = 3;

        public IPAddress MCastAddress { get { return this.multicast_ip; } }
        public int MCastPort { get { return this.multicast_port; } }
        public IPAddress ListenAddress { get { return this.listen_ip; } }
        public int ListenPort { get { return this.listen_port; } }
        public int Timeout { get { return this.timeout; } }

        bool verbose = false;
        public void Verbose(bool verbose = true)
        {
            this.verbose = verbose;
        }

        public void Discover(string query)
        {
            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

            socket.Bind(new IPEndPoint(this.listen_ip, this.listen_port));
            IPEndPoint endpoint = new IPEndPoint(this.multicast_ip, this.multicast_port);

            dns dns_Reqest = new dns();
            dns_Reqest.AddQuestion(TypeValues.PTR, ClassValues.IN, query.Split(".".ToArray()));

            if (true == this.verbose)
            {
                Trace.WriteLine($"mCAST Group={this.MCastAddress}");
                Trace.WriteLine($"mCAST Interface={this.MCastPort}");
                Trace.WriteLine($"Address={this.ListenAddress}");
                Trace.WriteLine($"Port={this.ListenPort}");
                Trace.WriteLine($"Query={query}");
                Trace.WriteLine($"Timeout={this.Timeout}");
            }

            socket.SendTo(dns_Reqest.Marshal(), endpoint);


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
                socket.ReceiveFromAsync(recieveArgs); // recive async

                //Wait for recieve
                if (false == readEvent.WaitOne(new TimeSpan(0, 0, this.timeout)))
                {
                    return;
                }
                if (recieveArgs.BytesTransferred == 0)
                {
                    if (recieveArgs.SocketError != SocketError.Success)
                    {
                        //Trace.TraceError($"SocketError={recieveArgs.SocketError.ToString()}({recieveArgs.SocketError})");
                        this.SocketErrorRaised?.Invoke(this, recieveArgs.SocketError);
                    }
                    //Trace.TraceInformation($"Terminated");
                    return;
                }

                this.DataRecived?.Invoke(this, recieveArgs.BytesTransferred, buf);

                //Trace.TraceInformation($"RemoteEndPoint={recieveArgs.RemoteEndPoint.ToString()}");

                using (MemoryStream stream = new MemoryStream(buf, 0, recieveArgs.BytesTransferred))
                {
                    //Debug.WriteLine(BitConverter.ToString(stream.ToArray()));
                    dns dns_response = dns.Unmarshal(stream.ToArray());

                    //Trace.TraceInformation($"Response={dns_response.Print()}");

                    this.UnmarshalCompleated?.Invoke(this, recieveArgs.RemoteEndPoint, dns_response);
                }
            }

        }
        public static mdns_discover CreateDiscover(IPAddress multicast_ip = null, int multicast_port = 0, IPAddress listen_ip = null, int listen_port = 0, int timeout = 0)
        {
            mdns_discover discover = new mdns_discover();

            if (null != multicast_ip)
                discover.multicast_ip = multicast_ip;
            if (0 < multicast_port)
                discover.multicast_port = multicast_port;
            if (null != listen_ip)
                discover.listen_ip = listen_ip;
            if (0 < listen_port)
                discover.listen_port = listen_port;
            if (0 < timeout)
                discover.timeout = timeout;

            return discover;
        }
    }
}
