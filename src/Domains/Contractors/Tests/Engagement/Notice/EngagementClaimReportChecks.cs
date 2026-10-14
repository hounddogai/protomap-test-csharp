namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementClaimReportChecks
{
    public static void EngagementClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702450962);
        var items = new List<EngagementClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementClaim.NewEngagementClaim($"engagement-claim-{index}", "sample"));
        }
        var report = EngagementClaimReport.BuildEngagementClaimReport("test", items, clock);
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
        EngagementClaimReportCountsEachRecordOnce();
    }
}
