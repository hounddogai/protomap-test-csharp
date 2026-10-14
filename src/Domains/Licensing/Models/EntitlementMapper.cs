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
/// The entitlement snapshot persisted by the domain repository.
/// </summary>
public sealed record EntitlementSnapshot(string TenantId, string Id, byte[] Payload);

public static class EntitlementMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLicensing.Entitlement value)
    {
        var snapshot = new EntitlementSnapshot(tenant, value.EntitlementId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "entitlement", snapshot.Id, snapshot.Payload);
    }
    public static ApiLicensing.Entitlement FromRecord(StoredRecord record) => ApiLicensing.Entitlement.Parser.ParseFrom(record.Payload);
}
