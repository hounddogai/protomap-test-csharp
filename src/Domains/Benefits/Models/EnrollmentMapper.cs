using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiBenefits = Acme.Benefits.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The enrollment snapshot persisted by the domain repository.
/// </summary>
public sealed record EnrollmentSnapshot(string TenantId, string Id, byte[] Payload);

public static class EnrollmentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiBenefits.Enrollment value)
    {
        var snapshot = new EnrollmentSnapshot(tenant, value.EnrollmentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "enrollment", snapshot.Id, snapshot.Payload);
    }
    public static ApiBenefits.Enrollment FromRecord(StoredRecord record) => ApiBenefits.Enrollment.Parser.ParseFrom(record.Payload);
}
