using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The equipment snapshot persisted by the domain repository.
/// </summary>
public sealed record EquipmentSnapshot(string TenantId, string Id, byte[] Payload);

public static class EquipmentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiOnboarding.Equipment value)
    {
        var snapshot = new EquipmentSnapshot(tenant, value.EquipmentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "equipment", snapshot.Id, snapshot.Payload);
    }
    public static ApiOnboarding.Equipment FromRecord(StoredRecord record) => ApiOnboarding.Equipment.Parser.ParseFrom(record.Payload);
}
