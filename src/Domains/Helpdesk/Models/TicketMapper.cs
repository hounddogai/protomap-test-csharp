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
/// The ticket snapshot persisted by the domain repository.
/// </summary>
public sealed record TicketSnapshot(string TenantId, string Id, byte[] Payload);

public static class TicketMapper
{
    public static StoredRecord ToRecord(string tenant, ApiHelpdesk.Ticket value)
    {
        var snapshot = new TicketSnapshot(tenant, value.TicketId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "ticket", snapshot.Id, snapshot.Payload);
    }
    public static ApiHelpdesk.Ticket FromRecord(StoredRecord record) => ApiHelpdesk.Ticket.Parser.ParseFrom(record.Payload);
}
