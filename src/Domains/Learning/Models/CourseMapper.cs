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
/// The course snapshot persisted by the domain repository.
/// </summary>
public sealed record CourseSnapshot(string TenantId, string Id, byte[] Payload);

public static class CourseMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLearning.Course value)
    {
        var snapshot = new CourseSnapshot(tenant, value.CourseId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "course", snapshot.Id, snapshot.Payload);
    }
    public static ApiLearning.Course FromRecord(StoredRecord record) => ApiLearning.Course.Parser.ParseFrom(record.Payload);
}
