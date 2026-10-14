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
/// The signature snapshot persisted by the domain repository.
/// </summary>
public sealed record SignatureSnapshot(string TenantId, string Id, byte[] Payload);

public static class SignatureMapper
{
    public static StoredRecord ToRecord(string tenant, ApiDocuments.Signature value)
    {
        var snapshot = new SignatureSnapshot(tenant, value.SignatureId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "signature", snapshot.Id, snapshot.Payload);
    }
    public static ApiDocuments.Signature FromRecord(StoredRecord record) => ApiDocuments.Signature.Parser.ParseFrom(record.Payload);
}
