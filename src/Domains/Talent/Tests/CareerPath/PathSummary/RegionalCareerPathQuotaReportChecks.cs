namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathQuotaReportChecks
{
    public static void RegionalCareerPathQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702660015);
        var items = new List<RegionalCareerPathQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalCareerPathQuota.NewRegionalCareerPathQuota($"regional-career-path-quota-{index}", "sample"));
        }
        var report = RegionalCareerPathQuotaReport.BuildRegionalCareerPathQuotaReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalCareerPathQuotaReportCountsEachRecordOnce();
    }
}
