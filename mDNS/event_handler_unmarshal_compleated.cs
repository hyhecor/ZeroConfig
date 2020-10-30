using DNS;
using System.Net;

namespace mDNS
{
    public delegate void UnmarshalCompleatedEventHandler<EventArg1, EventArg2>(object sender, EventArg1 endpoint, EventArg2 dns)
        where EventArg1 : EndPoint
        where EventArg2 : dns;

    public delegate void MatchedEventHandler<EventArg1>(object sender, EventArg1 dns)
    where EventArg1 : dns;
}
