namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantEntryReportChecks
{
    public static void EquityGrantEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701772822);
        var items = new List<EquityGrantEntry>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EquityGrantEntry.NewEquityGrantEntry($"equity-grant-entry-{index}", "sample"));
        }
        var report = EquityGrantEntryReport.BuildEquityGrantEntryReport("test", items, clock);
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
        EquityGrantEntryReportCountsEachRecordOnce();
    }
}
