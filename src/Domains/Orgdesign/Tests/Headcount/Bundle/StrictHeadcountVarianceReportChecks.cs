namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictHeadcountVarianceReportChecks
{
    public static void StrictHeadcountVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705774349);
        var items = new List<StrictHeadcountVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictHeadcountVariance.NewStrictHeadcountVariance($"strict-headcount-variance-{index}", "sample"));
        }
        var report = StrictHeadcountVarianceReport.BuildStrictHeadcountVarianceReport("test", items, clock);
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
        StrictHeadcountVarianceReportCountsEachRecordOnce();
    }
}
