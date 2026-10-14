using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The overtime snapshot persisted by the domain repository.
/// </summary>
public sealed record OvertimeSnapshot(string TenantId, string Id, byte[] Payload);

public static class OvertimeMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTimekeeping.Overtime value)
    {
        var snapshot = new OvertimeSnapshot(tenant, value.OvertimeId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "overtime", snapshot.Id, snapshot.Payload);
    }
    public static ApiTimekeeping.Overtime FromRecord(StoredRecord record) => ApiTimekeeping.Overtime.Parser.ParseFrom(record.Payload);
}
