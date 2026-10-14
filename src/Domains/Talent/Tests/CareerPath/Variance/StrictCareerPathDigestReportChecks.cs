namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictCareerPathDigestReportChecks
{
    public static void StrictCareerPathDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703337462);
        var items = new List<StrictCareerPathDigest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictCareerPathDigest.NewStrictCareerPathDigest($"strict-career-path-digest-{index}", "sample"));
        }
        var report = StrictCareerPathDigestReport.BuildStrictCareerPathDigestReport("test", items, clock);
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
        StrictCareerPathDigestReportCountsEachRecordOnce();
    }
}
