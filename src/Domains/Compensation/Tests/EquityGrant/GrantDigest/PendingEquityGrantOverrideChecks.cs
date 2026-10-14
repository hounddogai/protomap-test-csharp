namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideChecks
{
    public static void PendingEquityGrantOverrideStartsInPlanned()
    {
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
        Check.Equal(item.Status, PendingEquityGrantOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingEquityGrantOverrideMovesToInProgress()
    {
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
        try
        {
            item.MoveTo(PendingEquityGrantOverrideStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingEquityGrantOverrideStatus.InProgress, "status");
    }

    public static void PendingEquityGrantOverrideRejectsMovesFromCancelled()
    {
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
        item.Status = PendingEquityGrantOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingEquityGrantOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingEquityGrantOverrideStartsInPlanned();
        PendingEquityGrantOverrideMovesToInProgress();
        PendingEquityGrantOverrideRejectsMovesFromCancelled();
    }
}
