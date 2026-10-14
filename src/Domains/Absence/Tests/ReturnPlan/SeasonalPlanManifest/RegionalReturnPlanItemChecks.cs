namespace Acme.Workforce.Absence.Tests.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Checks of seasonal plan manifest code, run by the project's runner.</summary>
public static class RegionalReturnPlanItemChecks
{
    public static void RegionalReturnPlanItemStartsInDraft()
    {
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
        Check.Equal(item.Status, RegionalReturnPlanItemStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalReturnPlanItemMovesToPending()
    {
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
        try
        {
            item.MoveTo(RegionalReturnPlanItemStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalReturnPlanItemStatus.Pending, "status");
    }

    public static void RegionalReturnPlanItemRejectsMovesFromClosed()
    {
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
        item.Status = RegionalReturnPlanItemStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalReturnPlanItemStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalReturnPlanItemStartsInDraft();
        RegionalReturnPlanItemMovesToPending();
        RegionalReturnPlanItemRejectsMovesFromClosed();
    }
}
