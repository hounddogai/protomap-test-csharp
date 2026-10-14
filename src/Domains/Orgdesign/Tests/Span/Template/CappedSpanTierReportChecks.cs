namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedSpanTierReportChecks
{
    public static void CappedSpanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702045782);
        var items = new List<CappedSpanTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedSpanTier.NewCappedSpanTier($"capped-span-tier-{index}", "sample"));
        }
        var report = CappedSpanTierReport.BuildCappedSpanTierReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedSpanTierReportCountsEachRecordOnce();
    }
}
