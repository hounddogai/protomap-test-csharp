namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentEstimateChecks
{
    public static void GarnishmentEstimateStartsInPlanned()
    {
        var item = GarnishmentEstimate.NewGarnishmentEstimate("code-805", "name-566");
        Check.Equal(item.Status, GarnishmentEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentEstimateMovesToScheduled()
    {
        var item = GarnishmentEstimate.NewGarnishmentEstimate("code-805", "name-566");
        try
        {
            item.MoveTo(GarnishmentEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentEstimateStatus.Scheduled, "status");
    }

    public static void GarnishmentEstimateRejectsMovesFromCancelled()
    {
        var item = GarnishmentEstimate.NewGarnishmentEstimate("code-805", "name-566");
        item.Status = GarnishmentEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GarnishmentEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GarnishmentEstimateStartsInPlanned();
        GarnishmentEstimateMovesToScheduled();
        GarnishmentEstimateRejectsMovesFromCancelled();
    }
}
