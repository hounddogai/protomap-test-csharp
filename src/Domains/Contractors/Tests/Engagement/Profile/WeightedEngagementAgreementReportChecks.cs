namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedEngagementAgreementReportChecks
{
    public static void WeightedEngagementAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700246536);
        var items = new List<WeightedEngagementAgreement>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedEngagementAgreement.NewWeightedEngagementAgreement($"weighted-engagement-agreement-{index}", "sample"));
        }
        var report = WeightedEngagementAgreementReport.BuildWeightedEngagementAgreementReport("test", items, clock);
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
        WeightedEngagementAgreementReportCountsEachRecordOnce();
    }
}
