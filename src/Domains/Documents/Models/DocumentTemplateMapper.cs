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
/// The document template snapshot persisted by the domain repository.
/// </summary>
public sealed record DocumentTemplateSnapshot(string TenantId, string Id, byte[] Payload);

public static class DocumentTemplateMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDocuments.DocumentTemplate value)
    {
        var snapshot = new DocumentTemplateSnapshot(tenant, value.DocumentTemplateId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "document_template", snapshot.Id, snapshot.Payload);
    }
    public static ApiDocuments.DocumentTemplate FromRecord(StoredRecord record) => ApiDocuments.DocumentTemplate.Parser.ParseFrom(record.Payload);
}
