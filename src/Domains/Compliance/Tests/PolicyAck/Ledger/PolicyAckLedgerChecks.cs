namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PolicyAckLedgerChecks
{
    public static void PolicyAckLedgerStartsInRequested()
    {
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
        Check.Equal(item.Status, PolicyAckLedgerStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckLedgerMovesToRevoked()
    {
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
        try
        {
            item.MoveTo(PolicyAckLedgerStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckLedgerStatus.Revoked, "status");
    }

    public static void PolicyAckLedgerRejectsMovesFromExpired()
    {
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
        item.Status = PolicyAckLedgerStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyAckLedgerStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyAckLedgerStartsInRequested();
        PolicyAckLedgerMovesToRevoked();
        PolicyAckLedgerRejectsMovesFromExpired();
    }
}
