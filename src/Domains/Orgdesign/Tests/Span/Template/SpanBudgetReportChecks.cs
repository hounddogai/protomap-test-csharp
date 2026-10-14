namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetReportChecks
{
    public static void SpanBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705211890);
        var items = new List<SpanBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SpanBudget.NewSpanBudget($"span-budget-{index}", "sample"));
        }
        var report = SpanBudgetReport.BuildSpanBudgetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SpanBudgetReportCountsEachRecordOnce();
    }
}
