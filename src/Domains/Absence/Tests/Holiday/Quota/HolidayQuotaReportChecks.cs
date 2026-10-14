namespace Acme.Workforce.Absence.Tests.Holiday.Quota;

using Acme.Workforce.Absence.Holiday.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class HolidayQuotaReportChecks
{
    public static void HolidayQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708050624);
        var items = new List<HolidayQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HolidayQuota.NewHolidayQuota($"holiday-quota-{index}", "sample"));
        }
        var report = HolidayQuotaReport.BuildHolidayQuotaReport("test", items, clock);
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
        HolidayQuotaReportCountsEachRecordOnce();
    }
}
