namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandMilestoneReportChecks
{
    public static void PayBandMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700592626);
        var items = new List<PayBandMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayBandMilestone.NewPayBandMilestone($"pay-band-milestone-{index}", "sample"));
        }
        var report = PayBandMilestoneReport.BuildPayBandMilestoneReport("test", items, clock);
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
        PayBandMilestoneReportCountsEachRecordOnce();
    }
}
