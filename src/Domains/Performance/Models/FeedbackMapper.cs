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
/// The feedback snapshot persisted by the domain repository.
/// </summary>
public sealed record FeedbackSnapshot(string TenantId, string Id, byte[] Payload);

public static class FeedbackMapper
{
    public static StoredRecord ToRecord(string tenant, ApiPerformance.Feedback value)
    {
        var snapshot = new FeedbackSnapshot(tenant, value.FeedbackId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "feedback", snapshot.Id, snapshot.Payload);
    }
    public static ApiPerformance.Feedback FromRecord(StoredRecord record) => ApiPerformance.Feedback.Parser.ParseFrom(record.Payload);
}
