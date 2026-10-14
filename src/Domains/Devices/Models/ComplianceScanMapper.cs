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
/// The compliance scan snapshot persisted by the domain repository.
/// </summary>
public sealed record ComplianceScanSnapshot(string TenantId, string Id, byte[] Payload);

public static class ComplianceScanMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDevices.ComplianceScan value)
    {
        var snapshot = new ComplianceScanSnapshot(tenant, value.ComplianceScanId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "compliance_scan", snapshot.Id, snapshot.Payload);
    }
    public static ApiDevices.ComplianceScan FromRecord(StoredRecord record) => ApiDevices.ComplianceScan.Parser.ParseFrom(record.Payload);
}
