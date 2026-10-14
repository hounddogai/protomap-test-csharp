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
/// The candidate snapshot persisted by the domain repository.
/// </summary>
public sealed record CandidateSnapshot(string TenantId, string Id, byte[] Payload);

public static class CandidateMapper
{
    public static StoredRecord ToRecord(string tenant, ApiRecruiting.Candidate value)
    {
        var snapshot = new CandidateSnapshot(tenant, value.CandidateId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "candidate", snapshot.Id, snapshot.Payload);
    }
    public static ApiRecruiting.Candidate FromRecord(StoredRecord record) => ApiRecruiting.Candidate.Parser.ParseFrom(record.Payload);
}
