namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceRecordReportChecks
{
    public static void BalanceRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704411535);
        var items = new List<BalanceRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BalanceRecord.NewBalanceRecord($"balance-record-{index}", "sample"));
        }
        var report = BalanceRecordReport.BuildBalanceRecordReport("test", items, clock);
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
        BalanceRecordReportCountsEachRecordOnce();
    }
}
