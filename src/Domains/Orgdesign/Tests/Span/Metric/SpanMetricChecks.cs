namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanMetricChecks
{
    public static void SpanMetricStartsInOpen()
    {
        var item = SpanMetric.NewSpanMetric("code-110", "name-362");
        Check.Equal(item.Status, SpanMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanMetricMovesToReopened()
    {
        var item = SpanMetric.NewSpanMetric("code-110", "name-362");
        try
        {
            item.MoveTo(SpanMetricStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanMetricStatus.Reopened, "status");
    }

    public static void SpanMetricRejectsMovesFromArchived()
    {
        var item = SpanMetric.NewSpanMetric("code-110", "name-362");
        item.Status = SpanMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(SpanMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        SpanMetricStartsInOpen();
        SpanMetricMovesToReopened();
        SpanMetricRejectsMovesFromArchived();
    }
}
