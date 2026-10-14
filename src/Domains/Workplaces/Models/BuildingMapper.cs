using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The building snapshot persisted by the domain repository.
/// </summary>
public sealed record BuildingSnapshot(string TenantId, string Id, byte[] Payload);

public static class BuildingMapper
{
    public static StoredRecord ToRecord(string tenant, ApiWorkplaces.Building value)
    {
        var snapshot = new BuildingSnapshot(tenant, value.BuildingId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "building", snapshot.Id, snapshot.Payload);
    }
    public static ApiWorkplaces.Building FromRecord(StoredRecord record) => ApiWorkplaces.Building.Parser.ParseFrom(record.Payload);
}
