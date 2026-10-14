namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class DefaultVisitorMetricChecks
{
    public static void DefaultVisitorMetricStartsInIdle()
    {
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("code-277", "label-470");
        Check.Equal(item.Status, DefaultVisitorMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVisitorMetricMovesToWarming()
    {
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("code-277", "label-470");
        try
        {
            item.MoveTo(DefaultVisitorMetricStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVisitorMetricStatus.Warming, "status");
    }

    public static void DefaultVisitorMetricRejectsMovesFromStopped()
    {
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("code-277", "label-470");
        item.Status = DefaultVisitorMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultVisitorMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultVisitorMetricStartsInIdle();
        DefaultVisitorMetricMovesToWarming();
        DefaultVisitorMetricRejectsMovesFromStopped();
    }
}
