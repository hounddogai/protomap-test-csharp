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
/// The document folder snapshot persisted by the domain repository.
/// </summary>
public sealed record DocumentFolderSnapshot(string TenantId, string Id, byte[] Payload);

public static class DocumentFolderMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDocuments.DocumentFolder value)
    {
        var snapshot = new DocumentFolderSnapshot(tenant, value.DocumentFolderId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "document_folder", snapshot.Id, snapshot.Payload);
    }
    public static ApiDocuments.DocumentFolder FromRecord(StoredRecord record) => ApiDocuments.DocumentFolder.Parser.ParseFrom(record.Payload);
}
