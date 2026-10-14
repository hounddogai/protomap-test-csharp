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
/// The review snapshot persisted by the domain repository.
/// </summary>
public sealed record ReviewSnapshot(string TenantId, string Id, byte[] Payload);

public static class ReviewMapper
{
    public static StoredRecord ToRecord(string tenant, ApiPerformance.Review value)
    {
        var snapshot = new ReviewSnapshot(tenant, value.ReviewId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "review", snapshot.Id, snapshot.Payload);
    }
    public static ApiPerformance.Review FromRecord(StoredRecord record) => ApiPerformance.Review.Parser.ParseFrom(record.Payload);
}
