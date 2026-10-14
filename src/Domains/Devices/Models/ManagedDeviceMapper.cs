using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiDevices = Acme.Devices.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The managed device snapshot persisted by the domain repository.
/// </summary>
public sealed record ManagedDeviceSnapshot(string TenantId, string Id, byte[] Payload);

public static class ManagedDeviceMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDevices.ManagedDevice value)
    {
        var snapshot = new ManagedDeviceSnapshot(tenant, value.ManagedDeviceId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "managed_device", snapshot.Id, snapshot.Payload);
    }
    public static ApiDevices.ManagedDevice FromRecord(StoredRecord record) => ApiDevices.ManagedDevice.Parser.ParseFrom(record.Payload);
}
