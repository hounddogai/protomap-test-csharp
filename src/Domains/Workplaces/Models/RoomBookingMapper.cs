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
/// The room booking snapshot persisted by the domain repository.
/// </summary>
public sealed record RoomBookingSnapshot(string TenantId, string Id, byte[] Payload);

public static class RoomBookingMapper
{
    public static StoredRecord ToRecord(string tenant, ApiWorkplaces.RoomBooking value)
    {
        var snapshot = new RoomBookingSnapshot(tenant, value.RoomBookingId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "room_booking", snapshot.Id, snapshot.Payload);
    }
    public static ApiWorkplaces.RoomBooking FromRecord(StoredRecord record) => ApiWorkplaces.RoomBooking.Parser.ParseFrom(record.Payload);
}
