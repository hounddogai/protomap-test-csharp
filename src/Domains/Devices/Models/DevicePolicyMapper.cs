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
/// The device policy snapshot persisted by the domain repository.
/// </summary>
public sealed record DevicePolicySnapshot(string TenantId, string Id, byte[] Payload);

public static class DevicePolicyMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDevices.DevicePolicy value)
    {
        var snapshot = new DevicePolicySnapshot(tenant, value.DevicePolicyId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "device_policy", snapshot.Id, snapshot.Payload);
    }
    public static ApiDevices.DevicePolicy FromRecord(StoredRecord record) => ApiDevices.DevicePolicy.Parser.ParseFrom(record.Payload);
}
