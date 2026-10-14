namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanSummaryChecks
{
    public static void RegionalSuccessionPlanSummaryStartsInDraft()
    {
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("code-426", "label-811");
        Check.Equal(item.Status, RegionalSuccessionPlanSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSuccessionPlanSummaryMovesToPending()
    {
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("code-426", "label-811");
        try
        {
            item.MoveTo(RegionalSuccessionPlanSummaryStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSuccessionPlanSummaryStatus.Pending, "status");
    }

    public static void RegionalSuccessionPlanSummaryRejectsMovesFromClosed()
    {
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("code-426", "label-811");
        item.Status = RegionalSuccessionPlanSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalSuccessionPlanSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanSummaryStartsInDraft();
        RegionalSuccessionPlanSummaryMovesToPending();
        RegionalSuccessionPlanSummaryRejectsMovesFromClosed();
    }
}
