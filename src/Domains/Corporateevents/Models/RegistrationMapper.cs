using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The registration snapshot persisted by the domain repository.
/// </summary>
public sealed record RegistrationSnapshot(string TenantId, string Id, byte[] Payload);

public static class RegistrationMapper
{
    public static StoredRecord ToRecord(string tenant, ApiCorporateevents.Registration value)
    {
        var snapshot = new RegistrationSnapshot(tenant, value.RegistrationId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "registration", snapshot.Id, snapshot.Payload);
    }
    public static ApiCorporateevents.Registration FromRecord(StoredRecord record) => ApiCorporateevents.Registration.Parser.ParseFrom(record.Payload);
}
