namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementProfileReportChecks
{
    public static void EngagementProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700830205);
        var items = new List<EngagementProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementProfile.NewEngagementProfile($"engagement-profile-{index}", "sample"));
        }
        var report = EngagementProfileReport.BuildEngagementProfileReport("test", items, clock);
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
        EngagementProfileReportCountsEachRecordOnce();
    }
}
