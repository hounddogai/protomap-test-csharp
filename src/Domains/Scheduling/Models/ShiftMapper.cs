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
/// The shift snapshot persisted by the domain repository.
/// </summary>
public sealed record ShiftSnapshot(string TenantId, string Id, byte[] Payload);

public static class ShiftMapper
{
    public static StoredRecord ToRecord(string tenant, ApiScheduling.Shift value)
    {
        var snapshot = new ShiftSnapshot(tenant, value.ShiftId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "shift", snapshot.Id, snapshot.Payload);
    }
    public static ApiScheduling.Shift FromRecord(StoredRecord record) => ApiScheduling.Shift.Parser.ParseFrom(record.Payload);
}
