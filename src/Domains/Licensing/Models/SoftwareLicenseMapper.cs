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
/// The software license snapshot persisted by the domain repository.
/// </summary>
public sealed record SoftwareLicenseSnapshot(string TenantId, string Id, byte[] Payload);

public static class SoftwareLicenseMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLicensing.SoftwareLicense value)
    {
        var snapshot = new SoftwareLicenseSnapshot(tenant, value.SoftwareLicenseId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "software_license", snapshot.Id, snapshot.Payload);
    }
    public static ApiLicensing.SoftwareLicense FromRecord(StoredRecord record) => ApiLicensing.SoftwareLicense.Parser.ParseFrom(record.Payload);
}
