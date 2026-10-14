namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanTemplateReportChecks
{
    public static void SpanTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705875431);
        var items = new List<SpanTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanTemplate.NewSpanTemplate($"span-template-{index}", "sample"));
        }
        var report = SpanTemplateReport.BuildSpanTemplateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SpanTemplateReportCountsEachRecordOnce();
    }
}
