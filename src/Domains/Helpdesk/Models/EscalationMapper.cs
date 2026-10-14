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
/// The escalation snapshot persisted by the domain repository.
/// </summary>
public sealed record EscalationSnapshot(string TenantId, string Id, byte[] Payload);

public static class EscalationMapper
{
    public static StoredRecord ToRecord(string tenant, ApiHelpdesk.Escalation value)
    {
        var snapshot = new EscalationSnapshot(tenant, value.EscalationId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "escalation", snapshot.Id, snapshot.Payload);
    }
    public static ApiHelpdesk.Escalation FromRecord(StoredRecord record) => ApiHelpdesk.Escalation.Parser.ParseFrom(record.Payload);
}
