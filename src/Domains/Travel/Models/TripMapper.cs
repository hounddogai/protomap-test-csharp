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
/// The trip snapshot persisted by the domain repository.
/// </summary>
public sealed record TripSnapshot(string TenantId, string Id, byte[] Payload);

public static class TripMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTravel.Trip value)
    {
        var snapshot = new TripSnapshot(tenant, value.TripId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "trip", snapshot.Id, snapshot.Payload);
    }
    public static ApiTravel.Trip FromRecord(StoredRecord record) => ApiTravel.Trip.Parser.ParseFrom(record.Payload);
}
