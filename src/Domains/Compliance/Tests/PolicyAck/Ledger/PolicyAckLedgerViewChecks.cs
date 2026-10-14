namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PolicyAckLedgerViewChecks
{
    public static void PolicyAckLedgerViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
        var view = PolicyAckLedgerView.ToPolicyAckLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckLedgerView.ToPolicyAckLedgerViews(new List<PolicyAckLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckLedgerViewShowsTheStatusInCapitals();
    }
}
