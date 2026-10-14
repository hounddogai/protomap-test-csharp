namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssignmentMetricChecks
{
    public static void AssignmentMetricStartsInIdle()
    {
        var item = AssignmentMetric.NewAssignmentMetric("code-562", "label-556");
        Check.Equal(item.Status, AssignmentMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentMetricMovesToDraining()
    {
        var item = AssignmentMetric.NewAssignmentMetric("code-562", "label-556");
        try
        {
            item.MoveTo(AssignmentMetricStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentMetricStatus.Draining, "status");
    }

    public static void AssignmentMetricRejectsMovesFromStopped()
    {
        var item = AssignmentMetric.NewAssignmentMetric("code-562", "label-556");
        item.Status = AssignmentMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssignmentMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssignmentMetricStartsInIdle();
        AssignmentMetricMovesToDraining();
        AssignmentMetricRejectsMovesFromStopped();
    }
}
