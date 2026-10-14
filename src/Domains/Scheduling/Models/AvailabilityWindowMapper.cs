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
/// The availability window snapshot persisted by the domain repository.
/// </summary>
public sealed record AvailabilityWindowSnapshot(string TenantId, string Id, byte[] Payload);

public static class AvailabilityWindowMapper
{
    public static StoredRecord ToRecord(string tenant, ApiScheduling.AvailabilityWindow value)
    {
        var snapshot = new AvailabilityWindowSnapshot(tenant, value.AvailabilityWindowId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "availability_window", snapshot.Id, snapshot.Payload);
    }
    public static ApiScheduling.AvailabilityWindow FromRecord(StoredRecord record) => ApiScheduling.AvailabilityWindow.Parser.ParseFrom(record.Payload);
}
