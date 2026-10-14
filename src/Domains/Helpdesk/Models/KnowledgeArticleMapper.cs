using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiHelpdesk = Acme.Helpdesk.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The knowledge article snapshot persisted by the domain repository.
/// </summary>
public sealed record KnowledgeArticleSnapshot(string TenantId, string Id, byte[] Payload);

public static class KnowledgeArticleMapper
{
    public static StoredRecord ToRecord(string tenant, ApiHelpdesk.KnowledgeArticle value)
    {
        var snapshot = new KnowledgeArticleSnapshot(tenant, value.KnowledgeArticleId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "knowledge_article", snapshot.Id, snapshot.Payload);
    }
    public static ApiHelpdesk.KnowledgeArticle FromRecord(StoredRecord record) => ApiHelpdesk.KnowledgeArticle.Parser.ParseFrom(record.Payload);
}
