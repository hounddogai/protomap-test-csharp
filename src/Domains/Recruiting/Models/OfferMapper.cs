using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The offer snapshot persisted by the domain repository.
/// </summary>
public sealed record OfferSnapshot(string TenantId, string Id, byte[] Payload);

public static class OfferMapper
{
    public static StoredRecord ToRecord(string tenant, ApiRecruiting.Offer value)
    {
        var snapshot = new OfferSnapshot(tenant, value.OfferId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "offer", snapshot.Id, snapshot.Payload);
    }
    public static ApiRecruiting.Offer FromRecord(StoredRecord record) => ApiRecruiting.Offer.Parser.ParseFrom(record.Payload);
}
