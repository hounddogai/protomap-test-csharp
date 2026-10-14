using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// The document snapshot persisted by the domain repository.
/// </summary>
public sealed record DocumentSnapshot(string TenantId, string Id, byte[] Payload);

public static class DocumentMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDocuments.Document value)
    {
        var snapshot = new DocumentSnapshot(tenant, value.DocumentId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "document", snapshot.Id, snapshot.Payload);
    }
    public static ApiDocuments.Document FromRecord(StoredRecord record) => ApiDocuments.Document.Parser.ParseFrom(record.Payload);
}
