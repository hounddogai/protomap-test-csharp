namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingGarnishmentTierChecks
{
    public static void PendingGarnishmentTierStartsInPlanned()
    {
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("code-822", "title-434");
        Check.Equal(item.Status, PendingGarnishmentTierStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingGarnishmentTierMovesToInProgress()
    {
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("code-822", "title-434");
        try
        {
            item.MoveTo(PendingGarnishmentTierStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingGarnishmentTierStatus.InProgress, "status");
    }

    public static void PendingGarnishmentTierRejectsMovesFromCancelled()
    {
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("code-822", "title-434");
        item.Status = PendingGarnishmentTierStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingGarnishmentTierStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingGarnishmentTierStartsInPlanned();
        PendingGarnishmentTierMovesToInProgress();
        PendingGarnishmentTierRejectsMovesFromCancelled();
    }
}
