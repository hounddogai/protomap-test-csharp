namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantAdjustmentChecks
{
    public static void EquityGrantAdjustmentStartsInDraft()
    {
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("code-494", "title-345");
        Check.Equal(item.Status, EquityGrantAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantAdjustmentMovesToPending()
    {
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("code-494", "title-345");
        try
        {
            item.MoveTo(EquityGrantAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantAdjustmentStatus.Pending, "status");
    }

    public static void EquityGrantAdjustmentRejectsMovesFromClosed()
    {
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("code-494", "title-345");
        item.Status = EquityGrantAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(EquityGrantAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EquityGrantAdjustmentStartsInDraft();
        EquityGrantAdjustmentMovesToPending();
        EquityGrantAdjustmentRejectsMovesFromClosed();
    }
}
