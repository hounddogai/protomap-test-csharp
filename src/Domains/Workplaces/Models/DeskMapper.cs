using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The desk snapshot persisted by the domain repository.
/// </summary>
public sealed record DeskSnapshot(string TenantId, string Id, byte[] Payload);

public static class DeskMapper
{
    public static StoredRecord ToRecord(string tenant, ApiWorkplaces.Desk value)
    {
        var snapshot = new DeskSnapshot(tenant, value.DeskId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "desk", snapshot.Id, snapshot.Payload);
    }
    public static ApiWorkplaces.Desk FromRecord(StoredRecord record) => ApiWorkplaces.Desk.Parser.ParseFrom(record.Payload);
}
