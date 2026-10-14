namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedHeadcountWindowChecks
{
    public static void WeightedHeadcountWindowStartsInProposed()
    {
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("code-729", "label-977");
        Check.Equal(item.Status, WeightedHeadcountWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedHeadcountWindowMovesToApproved()
    {
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("code-729", "label-977");
        try
        {
            item.MoveTo(WeightedHeadcountWindowStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedHeadcountWindowStatus.Approved, "status");
    }

    public static void WeightedHeadcountWindowRejectsMovesFromWithdrawn()
    {
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("code-729", "label-977");
        item.Status = WeightedHeadcountWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedHeadcountWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedHeadcountWindowStartsInProposed();
        WeightedHeadcountWindowMovesToApproved();
        WeightedHeadcountWindowRejectsMovesFromWithdrawn();
    }
}
