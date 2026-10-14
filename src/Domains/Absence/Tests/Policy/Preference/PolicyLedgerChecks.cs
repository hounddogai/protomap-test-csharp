namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyLedgerChecks
{
    public static void PolicyLedgerStartsInPlanned()
    {
        var item = PolicyLedger.NewPolicyLedger("code-998", "title-965");
        Check.Equal(item.Status, PolicyLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyLedgerMovesToInProgress()
    {
        var item = PolicyLedger.NewPolicyLedger("code-998", "title-965");
        try
        {
            item.MoveTo(PolicyLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyLedgerStatus.InProgress, "status");
    }

    public static void PolicyLedgerRejectsMovesFromCancelled()
    {
        var item = PolicyLedger.NewPolicyLedger("code-998", "title-965");
        item.Status = PolicyLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyLedgerStartsInPlanned();
        PolicyLedgerMovesToInProgress();
        PolicyLedgerRejectsMovesFromCancelled();
    }
}
