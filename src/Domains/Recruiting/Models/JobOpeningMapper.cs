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
/// The job opening snapshot persisted by the domain repository.
/// </summary>
public sealed record JobOpeningSnapshot(string TenantId, string Id, byte[] Payload);

public static class JobOpeningMapper
{
    public static StoredRecord ToRecord(string tenant, ApiRecruiting.JobOpening value)
    {
        var snapshot = new JobOpeningSnapshot(tenant, value.JobOpeningId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "job_opening", snapshot.Id, snapshot.Payload);
    }
    public static ApiRecruiting.JobOpening FromRecord(StoredRecord record) => ApiRecruiting.JobOpening.Parser.ParseFrom(record.Payload);
}
