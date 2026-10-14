namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanMetricChecks
{
    public static void ReturnPlanMetricStartsInOpen()
    {
        var item = ReturnPlanMetric.NewReturnPlanMetric("code-288", "label-988");
        Check.Equal(item.Status, ReturnPlanMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanMetricMovesToReopened()
    {
        var item = ReturnPlanMetric.NewReturnPlanMetric("code-288", "label-988");
        try
        {
            item.MoveTo(ReturnPlanMetricStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanMetricStatus.Reopened, "status");
    }

    public static void ReturnPlanMetricRejectsMovesFromArchived()
    {
        var item = ReturnPlanMetric.NewReturnPlanMetric("code-288", "label-988");
        item.Status = ReturnPlanMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(ReturnPlanMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReturnPlanMetricStartsInOpen();
        ReturnPlanMetricMovesToReopened();
        ReturnPlanMetricRejectsMovesFromArchived();
    }
}
