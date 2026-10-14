namespace Acme.Helpdesk.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class KnowledgeArticle
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
