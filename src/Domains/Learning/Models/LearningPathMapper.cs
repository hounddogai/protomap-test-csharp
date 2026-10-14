using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The learning path snapshot persisted by the domain repository.
/// </summary>
public sealed record LearningPathSnapshot(string TenantId, string Id, byte[] Payload);

public static class LearningPathMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLearning.LearningPath value)
    {
        var snapshot = new LearningPathSnapshot(tenant, value.LearningPathId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "learning_path", snapshot.Id, snapshot.Payload);
    }
    public static ApiLearning.LearningPath FromRecord(StoredRecord record) => ApiLearning.LearningPath.Parser.ParseFrom(record.Payload);
}
