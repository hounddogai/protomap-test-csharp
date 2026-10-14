namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class PendingHolidayQuotaReportChecks
{
    public static void PendingHolidayQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705230190);
        var items = new List<PendingHolidayQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingHolidayQuota.NewPendingHolidayQuota($"pending-holiday-quota-{index}", "sample"));
        }
        var report = PendingHolidayQuotaReport.BuildPendingHolidayQuotaReport("test", items, clock);
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
        PendingHolidayQuotaReportCountsEachRecordOnce();
    }
}
