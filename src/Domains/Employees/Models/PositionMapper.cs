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
/// The position snapshot persisted by the domain repository.
/// </summary>
public sealed record PositionSnapshot(string TenantId, string Id, byte[] Payload);

public static class PositionMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployees.Position value)
    {
        var snapshot = new PositionSnapshot(tenant, value.PositionId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "position", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployees.Position FromRecord(StoredRecord record) => ApiEmployees.Position.Parser.ParseFrom(record.Payload);
}
