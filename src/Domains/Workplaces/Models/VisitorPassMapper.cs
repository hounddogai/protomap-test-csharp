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
/// The visitor pass snapshot persisted by the domain repository.
/// </summary>
public sealed record VisitorPassSnapshot(string TenantId, string Id, byte[] Payload);

public static class VisitorPassMapper
{
    public static StoredRecord ToRecord(string tenant, ApiWorkplaces.VisitorPass value)
    {
        var snapshot = new VisitorPassSnapshot(tenant, value.VisitorPassId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "visitor_pass", snapshot.Id, snapshot.Payload);
    }
    public static ApiWorkplaces.VisitorPass FromRecord(StoredRecord record) => ApiWorkplaces.VisitorPass.Parser.ParseFrom(record.Payload);
}
