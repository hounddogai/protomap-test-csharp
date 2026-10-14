using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiEmployeeportal = Acme.Employeeportal.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The announcement snapshot persisted by the domain repository.
/// </summary>
public sealed record AnnouncementSnapshot(string TenantId, string Id, byte[] Payload);

public static class AnnouncementMapper
{
    public static StoredRecord ToRecord(string tenant, ApiEmployeeportal.Announcement value)
    {
        var snapshot = new AnnouncementSnapshot(tenant, value.AnnouncementId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "announcement", snapshot.Id, snapshot.Payload);
    }
    public static ApiEmployeeportal.Announcement FromRecord(StoredRecord record) => ApiEmployeeportal.Announcement.Parser.ParseFrom(record.Payload);
}
