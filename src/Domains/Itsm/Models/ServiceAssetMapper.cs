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
/// The service asset snapshot persisted by the domain repository.
/// </summary>
public sealed record ServiceAssetSnapshot(string TenantId, string Id, byte[] Payload);

public static class ServiceAssetMapper
{
    public static StoredRecord ToRecord(string tenant, ApiItsm.ServiceAsset value)
    {
        var snapshot = new ServiceAssetSnapshot(tenant, value.ServiceAssetId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "service_asset", snapshot.Id, snapshot.Payload);
    }
    public static ApiItsm.ServiceAsset FromRecord(StoredRecord record) => ApiItsm.ServiceAsset.Parser.ParseFrom(record.Payload);
}
