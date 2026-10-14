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
/// The swap request snapshot persisted by the domain repository.
/// </summary>
public sealed record SwapRequestSnapshot(string TenantId, string Id, byte[] Payload);

public static class SwapRequestMapper
{
    public static StoredRecord ToRecord(string tenant, ApiScheduling.SwapRequest value)
    {
        var snapshot = new SwapRequestSnapshot(tenant, value.SwapRequestId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "swap_request", snapshot.Id, snapshot.Payload);
    }
    public static ApiScheduling.SwapRequest FromRecord(StoredRecord record) => ApiScheduling.SwapRequest.Parser.ParseFrom(record.Payload);
}
