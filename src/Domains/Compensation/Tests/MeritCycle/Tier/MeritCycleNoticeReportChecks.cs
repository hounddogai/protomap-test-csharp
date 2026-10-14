namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeReportChecks
{
    public static void MeritCycleNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703774759);
        var items = new List<MeritCycleNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MeritCycleNotice.NewMeritCycleNotice($"merit-cycle-notice-{index}", "sample"));
        }
        var report = MeritCycleNoticeReport.BuildMeritCycleNoticeReport("test", items, clock);
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
        MeritCycleNoticeReportCountsEachRecordOnce();
    }
}
