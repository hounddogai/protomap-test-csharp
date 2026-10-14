namespace Acme.Documents.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Document
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class DocumentTemplate
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
