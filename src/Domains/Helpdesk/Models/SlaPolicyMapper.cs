using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiHelpdesk = Acme.Helpdesk.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The sla policy snapshot persisted by the domain repository.
/// </summary>
public sealed record SlaPolicySnapshot(string TenantId, string Id, byte[] Payload);

public static class SlaPolicyMapper
{
    public static StoredRecord ToRecord(string tenant, ApiHelpdesk.SlaPolicy value)
    {
        var snapshot = new SlaPolicySnapshot(tenant, value.SlaPolicyId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "sla_policy", snapshot.Id, snapshot.Payload);
    }
    public static ApiHelpdesk.SlaPolicy FromRecord(StoredRecord record) => ApiHelpdesk.SlaPolicy.Parser.ParseFrom(record.Payload);
}
