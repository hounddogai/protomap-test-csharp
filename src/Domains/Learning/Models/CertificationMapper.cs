using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The certification snapshot persisted by the domain repository.
/// </summary>
public sealed record CertificationSnapshot(string TenantId, string Id, byte[] Payload);

public static class CertificationMapper
{
    public static StoredRecord ToRecord(string tenant, ApiLearning.Certification value)
    {
        var snapshot = new CertificationSnapshot(tenant, value.CertificationId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "certification", snapshot.Id, snapshot.Payload);
    }
    public static ApiLearning.Certification FromRecord(StoredRecord record) => ApiLearning.Certification.Parser.ParseFrom(record.Payload);
}
