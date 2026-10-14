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
/// The travel booking snapshot persisted by the domain repository.
/// </summary>
public sealed record TravelBookingSnapshot(string TenantId, string Id, byte[] Payload);

public static class TravelBookingMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTravel.TravelBooking value)
    {
        var snapshot = new TravelBookingSnapshot(tenant, value.TravelBookingId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "travel_booking", snapshot.Id, snapshot.Payload);
    }
    public static ApiTravel.TravelBooking FromRecord(StoredRecord record) => ApiTravel.TravelBooking.Parser.ParseFrom(record.Payload);
}
