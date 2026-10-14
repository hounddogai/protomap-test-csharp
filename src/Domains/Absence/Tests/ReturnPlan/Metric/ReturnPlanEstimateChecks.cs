namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateChecks
{
    public static void ReturnPlanEstimateStartsInPlanned()
    {
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
        Check.Equal(item.Status, ReturnPlanEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanEstimateMovesToScheduled()
    {
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
        try
        {
            item.MoveTo(ReturnPlanEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanEstimateStatus.Scheduled, "status");
    }

    public static void ReturnPlanEstimateRejectsMovesFromCancelled()
    {
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
        item.Status = ReturnPlanEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReturnPlanEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReturnPlanEstimateStartsInPlanned();
        ReturnPlanEstimateMovesToScheduled();
        ReturnPlanEstimateRejectsMovesFromCancelled();
    }
}
