namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceChecks
{
    public static void DefaultSuccessionPlanVarianceStartsInPlanned()
    {
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
        Check.Equal(item.Status, DefaultSuccessionPlanVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSuccessionPlanVarianceMovesToScheduled()
    {
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
        try
        {
            item.MoveTo(DefaultSuccessionPlanVarianceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSuccessionPlanVarianceStatus.Scheduled, "status");
    }

    public static void DefaultSuccessionPlanVarianceRejectsMovesFromCancelled()
    {
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
        item.Status = DefaultSuccessionPlanVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultSuccessionPlanVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultSuccessionPlanVarianceStartsInPlanned();
        DefaultSuccessionPlanVarianceMovesToScheduled();
        DefaultSuccessionPlanVarianceRejectsMovesFromCancelled();
    }
}
