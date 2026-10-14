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
/// The employment contract snapshot persisted by the domain repository.
/// </summary>
public sealed record EmploymentContractSnapshot(string TenantId, string Id, byte[] Payload);

public static class EmploymentContractMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployees.EmploymentContract value)
    {
        var snapshot = new EmploymentContractSnapshot(tenant, value.EmploymentContractId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "employment_contract", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployees.EmploymentContract FromRecord(StoredRecord record) => ApiEmployees.EmploymentContract.Parser.ParseFrom(record.Payload);
}
