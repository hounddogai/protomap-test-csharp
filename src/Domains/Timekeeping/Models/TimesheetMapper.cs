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
/// The timesheet snapshot persisted by the domain repository.
/// </summary>
public sealed record TimesheetSnapshot(string TenantId, string Id, byte[] Payload);

public static class TimesheetMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTimekeeping.Timesheet value)
    {
        var snapshot = new TimesheetSnapshot(tenant, value.TimesheetId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "timesheet", snapshot.Id, snapshot.Payload);
    }
    public static ApiTimekeeping.Timesheet FromRecord(StoredRecord record) => ApiTimekeeping.Timesheet.Parser.ParseFrom(record.Payload);
}
