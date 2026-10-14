namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestReportChecks
{
    public static void ScheduledMobilityRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701701770);
        var items = new List<ScheduledMobilityRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledMobilityRequest.NewScheduledMobilityRequest($"scheduled-mobility-request-{index}", "sample"));
        }
        var report = ScheduledMobilityRequestReport.BuildScheduledMobilityRequestReport("test", items, clock);
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
        ScheduledMobilityRequestReportCountsEachRecordOnce();
    }
}
