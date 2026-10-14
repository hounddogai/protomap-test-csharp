namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineChecks
{
    public static void AdaptiveBalanceBaselineStartsInProposed()
    {
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
        Check.Equal(item.Status, AdaptiveBalanceBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBalanceBaselineMovesToRejected()
    {
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
        try
        {
            item.MoveTo(AdaptiveBalanceBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBalanceBaselineStatus.Rejected, "status");
    }

    public static void AdaptiveBalanceBaselineRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
        item.Status = AdaptiveBalanceBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveBalanceBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveBalanceBaselineStartsInProposed();
        AdaptiveBalanceBaselineMovesToRejected();
        AdaptiveBalanceBaselineRejectsMovesFromWithdrawn();
    }
}
