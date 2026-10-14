namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedEngagementAgreementReportChecks
{
    public static void CappedEngagementAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704228140);
        var items = new List<CappedEngagementAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedEngagementAgreement.NewCappedEngagementAgreement($"capped-engagement-agreement-{index}", "sample"));
        }
        var report = CappedEngagementAgreementReport.BuildCappedEngagementAgreementReport("test", items, clock);
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
        CappedEngagementAgreementReportCountsEachRecordOnce();
    }
}
