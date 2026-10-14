namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestReportChecks
{
    public static void RefreshDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700917065);
        var items = new List<RefreshDigest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RefreshDigest.NewRefreshDigest($"refresh-digest-{index}", "sample"));
        }
        var report = RefreshDigestReport.BuildRefreshDigestReport("test", items, clock);
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
        RefreshDigestReportCountsEachRecordOnce();
    }
}
