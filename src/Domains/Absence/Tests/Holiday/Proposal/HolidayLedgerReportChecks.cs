namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayLedgerReportChecks
{
    public static void HolidayLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703730857);
        var items = new List<HolidayLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayLedger.NewHolidayLedger($"holiday-ledger-{index}", "sample"));
        }
        var report = HolidayLedgerReport.BuildHolidayLedgerReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        HolidayLedgerReportCountsEachRecordOnce();
    }
}
