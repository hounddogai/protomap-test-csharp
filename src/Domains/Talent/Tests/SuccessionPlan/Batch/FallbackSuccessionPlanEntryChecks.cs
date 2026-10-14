namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanEntryChecks
{
    public static void FallbackSuccessionPlanEntryStartsInNew()
    {
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("code-841", "label-594");
        Check.Equal(item.Status, FallbackSuccessionPlanEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSuccessionPlanEntryMovesToFailed()
    {
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("code-841", "label-594");
        try
        {
            item.MoveTo(FallbackSuccessionPlanEntryStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSuccessionPlanEntryStatus.Failed, "status");
    }

    public static void FallbackSuccessionPlanEntryRejectsMovesFromFailed()
    {
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("code-841", "label-594");
        item.Status = FallbackSuccessionPlanEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackSuccessionPlanEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanEntryStartsInNew();
        FallbackSuccessionPlanEntryMovesToFailed();
        FallbackSuccessionPlanEntryRejectsMovesFromFailed();
    }
}
