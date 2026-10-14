using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The itinerary snapshot persisted by the domain repository.
/// </summary>
public sealed record ItinerarySnapshot(string TenantId, string Id, byte[] Payload);

public static class ItineraryMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTravel.Itinerary value)
    {
        var snapshot = new ItinerarySnapshot(tenant, value.ItineraryId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "itinerary", snapshot.Id, snapshot.Payload);
    }
    public static ApiTravel.Itinerary FromRecord(StoredRecord record) => ApiTravel.Itinerary.Parser.ParseFrom(record.Payload);
}
