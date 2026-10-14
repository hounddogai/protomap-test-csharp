namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class WeightedVisitorBaselineChecks
{
    public static void WeightedVisitorBaselineStartsInOpen()
    {
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("code-890", "label-845");
        Check.Equal(item.Status, WeightedVisitorBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedVisitorBaselineMovesToResolved()
    {
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("code-890", "label-845");
        try
        {
            item.MoveTo(WeightedVisitorBaselineStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedVisitorBaselineStatus.Resolved, "status");
    }

    public static void WeightedVisitorBaselineRejectsMovesFromArchived()
    {
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("code-890", "label-845");
        item.Status = WeightedVisitorBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedVisitorBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedVisitorBaselineStartsInOpen();
        WeightedVisitorBaselineMovesToResolved();
        WeightedVisitorBaselineRejectsMovesFromArchived();
    }
}
