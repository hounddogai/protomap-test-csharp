using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The goal snapshot persisted by the domain repository.
/// </summary>
public sealed record GoalSnapshot(string TenantId, string Id, byte[] Payload);

public static class GoalMapper
{
    public static StoredRecord ToRecord(string tenant, ApiPerformance.Goal value)
    {
        var snapshot = new GoalSnapshot(tenant, value.GoalId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "goal", snapshot.Id, snapshot.Payload);
    }
    public static ApiPerformance.Goal FromRecord(StoredRecord record) => ApiPerformance.Goal.Parser.ParseFrom(record.Payload);
}
