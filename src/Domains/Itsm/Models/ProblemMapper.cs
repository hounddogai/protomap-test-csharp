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
/// The problem snapshot persisted by the domain repository.
/// </summary>
public sealed record ProblemSnapshot(string TenantId, string Id, byte[] Payload);

public static class ProblemMapper
{
    public static StoredRecord ToRecord(string tenant, ApiItsm.Problem value)
    {
        var snapshot = new ProblemSnapshot(tenant, value.ProblemId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "problem", snapshot.Id, snapshot.Payload);
    }
    public static ApiItsm.Problem FromRecord(StoredRecord record) => ApiItsm.Problem.Parser.ParseFrom(record.Payload);
}
