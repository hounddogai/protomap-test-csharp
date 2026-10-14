using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The incident snapshot persisted by the domain repository.
/// </summary>
public sealed record IncidentSnapshot(string TenantId, string Id, byte[] Payload);

public static class IncidentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiItsm.Incident value)
    {
        var snapshot = new IncidentSnapshot(tenant, value.IncidentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "incident", snapshot.Id, snapshot.Payload);
    }
    public static ApiItsm.Incident FromRecord(StoredRecord record) => ApiItsm.Incident.Parser.ParseFrom(record.Payload);
}
