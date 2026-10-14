using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiBenefits = Acme.Benefits.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The life event snapshot persisted by the domain repository.
/// </summary>
public sealed record LifeEventSnapshot(string TenantId, string Id, byte[] Payload);

public static class LifeEventMapper
{
    public static StoredRecord ToRecord(string tenant, ApiBenefits.LifeEvent value)
    {
        var snapshot = new LifeEventSnapshot(tenant, value.LifeEventId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "life_event", snapshot.Id, snapshot.Payload);
    }
    public static ApiBenefits.LifeEvent FromRecord(StoredRecord record) => ApiBenefits.LifeEvent.Parser.ParseFrom(record.Payload);
}
