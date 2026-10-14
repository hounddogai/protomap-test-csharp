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
/// The corporate event snapshot persisted by the domain repository.
/// </summary>
public sealed record CorporateEventSnapshot(string TenantId, string Id, byte[] Payload);

public static class CorporateEventMapper
{
    public static StoredRecord ToRecord(string tenant, ApiCorporateevents.CorporateEvent value)
    {
        var snapshot = new CorporateEventSnapshot(tenant, value.CorporateEventId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "corporate_event", snapshot.Id, snapshot.Payload);
    }
    public static ApiCorporateevents.CorporateEvent FromRecord(StoredRecord record) => ApiCorporateevents.CorporateEvent.Parser.ParseFrom(record.Payload);
}
