using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The license assignment snapshot persisted by the domain repository.
/// </summary>
public sealed record LicenseAssignmentSnapshot(string TenantId, string Id, byte[] Payload);

public static class LicenseAssignmentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLicensing.LicenseAssignment value)
    {
        var snapshot = new LicenseAssignmentSnapshot(tenant, value.LicenseAssignmentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "license_assignment", snapshot.Id, snapshot.Payload);
    }
    public static ApiLicensing.LicenseAssignment FromRecord(StoredRecord record) => ApiLicensing.LicenseAssignment.Parser.ParseFrom(record.Payload);
}
