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
/// The interview snapshot persisted by the domain repository.
/// </summary>
public sealed record InterviewSnapshot(string TenantId, string Id, byte[] Payload);

public static class InterviewMapper
{
    public static StoredRecord ToRecord(string tenant, ApiRecruiting.Interview value)
    {
        var snapshot = new InterviewSnapshot(tenant, value.InterviewId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "interview", snapshot.Id, snapshot.Payload);
    }
    public static ApiRecruiting.Interview FromRecord(StoredRecord record) => ApiRecruiting.Interview.Parser.ParseFrom(record.Payload);
}
