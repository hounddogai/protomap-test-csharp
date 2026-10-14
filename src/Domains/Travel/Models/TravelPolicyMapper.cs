using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The travel policy snapshot persisted by the domain repository.
/// </summary>
public sealed record TravelPolicySnapshot(string TenantId, string Id, byte[] Payload);

public static class TravelPolicyMapper
{
    public static StoredRecord ToRecord(string tenant, ApiTravel.TravelPolicy value)
    {
        var snapshot = new TravelPolicySnapshot(tenant, value.TravelPolicyId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "travel_policy", snapshot.Id, snapshot.Payload);
    }
    public static ApiTravel.TravelPolicy FromRecord(StoredRecord record) => ApiTravel.TravelPolicy.Parser.ParseFrom(record.Payload);
}
