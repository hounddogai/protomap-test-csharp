using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The roster snapshot persisted by the domain repository.
/// </summary>
public sealed record RosterSnapshot(string TenantId, string Id, byte[] Payload);

public static class RosterMapper
{
    public static StoredRecord ToRecord(string tenant, ApiScheduling.Roster value)
    {
        var snapshot = new RosterSnapshot(tenant, value.RosterId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "roster", snapshot.Id, snapshot.Payload);
    }
    public static ApiScheduling.Roster FromRecord(StoredRecord record) => ApiScheduling.Roster.Parser.ParseFrom(record.Payload);
}
