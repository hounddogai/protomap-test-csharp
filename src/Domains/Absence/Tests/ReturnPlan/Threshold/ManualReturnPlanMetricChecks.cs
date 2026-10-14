namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualReturnPlanMetricChecks
{
    public static void ManualReturnPlanMetricStartsInOpen()
    {
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("code-40", "name-521");
        Check.Equal(item.Status, ManualReturnPlanMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualReturnPlanMetricMovesToArchived()
    {
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("code-40", "name-521");
        try
        {
            item.MoveTo(ManualReturnPlanMetricStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualReturnPlanMetricStatus.Archived, "status");
    }

    public static void ManualReturnPlanMetricRejectsMovesFromArchived()
    {
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("code-40", "name-521");
        item.Status = ManualReturnPlanMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualReturnPlanMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualReturnPlanMetricStartsInOpen();
        ManualReturnPlanMetricMovesToArchived();
        ManualReturnPlanMetricRejectsMovesFromArchived();
    }
}
