using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiBenefits = Acme.Benefits.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The dependent snapshot persisted by the domain repository.
/// </summary>
public sealed record DependentSnapshot(string TenantId, string Id, byte[] Payload);

public static class DependentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiBenefits.Dependent value)
    {
        var snapshot = new DependentSnapshot(tenant, value.DependentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "dependent", snapshot.Id, snapshot.Payload);
    }
    public static ApiBenefits.Dependent FromRecord(StoredRecord record) => ApiBenefits.Dependent.Parser.ParseFrom(record.Payload);
}
