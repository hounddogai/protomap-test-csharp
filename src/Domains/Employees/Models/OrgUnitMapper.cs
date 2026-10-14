using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiEmployees = Acme.Employees.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The org unit snapshot persisted by the domain repository.
/// </summary>
public sealed record OrgUnitSnapshot(string TenantId, string Id, byte[] Payload);

public static class OrgUnitMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployees.OrgUnit value)
    {
        var snapshot = new OrgUnitSnapshot(tenant, value.OrgUnitId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "org_unit", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployees.OrgUnit FromRecord(StoredRecord record) => ApiEmployees.OrgUnit.Parser.ParseFrom(record.Payload);
}
