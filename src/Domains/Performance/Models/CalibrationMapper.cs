using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The calibration snapshot persisted by the domain repository.
/// </summary>
public sealed record CalibrationSnapshot(string TenantId, string Id, byte[] Payload);

public static class CalibrationMapper
{
    public static StoredRecord ToRecord(string tenant, ApiPerformance.Calibration value)
    {
        var snapshot = new CalibrationSnapshot(tenant, value.CalibrationId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "calibration", snapshot.Id, snapshot.Payload);
    }
    public static ApiPerformance.Calibration FromRecord(StoredRecord record) => ApiPerformance.Calibration.Parser.ParseFrom(record.Payload);
}
