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
/// The employee snapshot persisted by the domain repository.
/// </summary>
public sealed record EmployeeSnapshot(string TenantId, string Id, byte[] Payload);

public static class EmployeeMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployees.Employee value)
    {
        var snapshot = new EmployeeSnapshot(tenant, value.EmployeeId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "employee", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployees.Employee FromRecord(StoredRecord record) => ApiEmployees.Employee.Parser.ParseFrom(record.Payload);
}
