namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanEntryChecks
{
    public static void AdaptiveReturnPlanEntryStartsInPlanned()
    {
        var item = AdaptiveReturnPlanEntry.NewAdaptiveReturnPlanEntry("code-535", "name-366");
        Check.Equal(item.Status, AdaptiveReturnPlanEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveReturnPlanEntryMovesToCompleted()
    {
        var item = AdaptiveReturnPlanEntry.NewAdaptiveReturnPlanEntry("code-535", "name-366");
        try
        {
            item.MoveTo(AdaptiveReturnPlanEntryStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveReturnPlanEntryStatus.Completed, "status");
    }

    public static void AdaptiveReturnPlanEntryRejectsMovesFromCancelled()
    {
        var item = AdaptiveReturnPlanEntry.NewAdaptiveReturnPlanEntry("code-535", "name-366");
        item.Status = AdaptiveReturnPlanEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveReturnPlanEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanEntryStartsInPlanned();
        AdaptiveReturnPlanEntryMovesToCompleted();
        AdaptiveReturnPlanEntryRejectsMovesFromCancelled();
    }
}
