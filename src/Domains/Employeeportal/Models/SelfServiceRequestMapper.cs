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
/// The self service request snapshot persisted by the domain repository.
/// </summary>
public sealed record SelfServiceRequestSnapshot(string TenantId, string Id, byte[] Payload);

public static class SelfServiceRequestMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployeeportal.SelfServiceRequest value)
    {
        var snapshot = new SelfServiceRequestSnapshot(tenant, value.SelfServiceRequestId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "self_service_request", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployeeportal.SelfServiceRequest FromRecord(StoredRecord record) => ApiEmployeeportal.SelfServiceRequest.Parser.ParseFrom(record.Payload);
}
