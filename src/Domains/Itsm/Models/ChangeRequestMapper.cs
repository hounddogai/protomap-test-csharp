using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The change request snapshot persisted by the domain repository.
/// </summary>
public sealed record ChangeRequestSnapshot(string TenantId, string Id, byte[] Payload);

public static class ChangeRequestMapper
{
    public static StoredRecord ToRecord(string tenant, ApiItsm.ChangeRequest value)
    {
        var snapshot = new ChangeRequestSnapshot(tenant, value.ChangeRequestId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "change_request", snapshot.Id, snapshot.Payload);
    }
    public static ApiItsm.ChangeRequest FromRecord(StoredRecord record) => ApiItsm.ChangeRequest.Parser.ParseFrom(record.Payload);
}
