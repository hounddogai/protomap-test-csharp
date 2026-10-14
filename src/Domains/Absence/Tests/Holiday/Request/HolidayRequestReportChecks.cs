namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestReportChecks
{
    public static void HolidayRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708755886);
        var items = new List<HolidayRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayRequest.NewHolidayRequest($"holiday-request-{index}", "sample"));
        }
        var report = HolidayRequestReport.BuildHolidayRequestReport("test", items, clock);
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
        HolidayRequestReportCountsEachRecordOnce();
    }
}
