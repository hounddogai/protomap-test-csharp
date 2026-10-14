using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The clock event snapshot persisted by the domain repository.
/// </summary>
public sealed record ClockEventSnapshot(string TenantId, string Id, byte[] Payload);

public static class ClockEventMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTimekeeping.ClockEvent value)
    {
        var snapshot = new ClockEventSnapshot(tenant, value.ClockEventId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "clock_event", snapshot.Id, snapshot.Payload);
    }
    public static ApiTimekeeping.ClockEvent FromRecord(StoredRecord record) => ApiTimekeeping.ClockEvent.Parser.ParseFrom(record.Payload);
}
