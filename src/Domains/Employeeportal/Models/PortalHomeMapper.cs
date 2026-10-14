using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiEmployeeportal = Acme.Employeeportal.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The portal home snapshot persisted by the domain repository.
/// </summary>
public sealed record PortalHomeSnapshot(string TenantId, string Id, byte[] Payload);

public static class PortalHomeMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployeeportal.PortalHome value)
    {
        var snapshot = new PortalHomeSnapshot(tenant, value.PortalHomeId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "portal_home", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployeeportal.PortalHome FromRecord(StoredRecord record) => ApiEmployeeportal.PortalHome.Parser.ParseFrom(record.Payload);
}
