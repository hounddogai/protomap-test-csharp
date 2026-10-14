using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The venue snapshot persisted by the domain repository.
/// </summary>
public sealed record VenueSnapshot(string TenantId, string Id, byte[] Payload);

public static class VenueMapper
{
    public static StoredRecord ToRecord(string tenant, ApiCorporateevents.Venue value)
    {
        var snapshot = new VenueSnapshot(tenant, value.VenueId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "venue", snapshot.Id, snapshot.Payload);
    }
    public static ApiCorporateevents.Venue FromRecord(StoredRecord record) => ApiCorporateevents.Venue.Parser.ParseFrom(record.Payload);
}
