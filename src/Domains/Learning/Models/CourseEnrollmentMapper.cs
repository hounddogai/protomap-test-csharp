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
/// The course enrollment snapshot persisted by the domain repository.
/// </summary>
public sealed record CourseEnrollmentSnapshot(string TenantId, string Id, byte[] Payload);

public static class CourseEnrollmentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLearning.CourseEnrollment value)
    {
        var snapshot = new CourseEnrollmentSnapshot(tenant, value.CourseEnrollmentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "course_enrollment", snapshot.Id, snapshot.Payload);
    }
    public static ApiLearning.CourseEnrollment FromRecord(StoredRecord record) => ApiLearning.CourseEnrollment.Parser.ParseFrom(record.Payload);
}
