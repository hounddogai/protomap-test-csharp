namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingBalanceRecordReportChecks
{
    public static void RollingBalanceRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708110465);
        var items = new List<RollingBalanceRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RollingBalanceRecord.NewRollingBalanceRecord($"rolling-balance-record-{index}", "sample"));
        }
        var report = RollingBalanceRecordReport.BuildRollingBalanceRecordReport("test", items, clock);
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
        RollingBalanceRecordReportCountsEachRecordOnce();
    }
}
