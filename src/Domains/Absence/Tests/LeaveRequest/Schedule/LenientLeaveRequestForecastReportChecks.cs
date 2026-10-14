namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientLeaveRequestForecastReportChecks
{
    public static void LenientLeaveRequestForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702020734);
        var items = new List<LenientLeaveRequestForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientLeaveRequestForecast.NewLenientLeaveRequestForecast($"lenient-leave-request-forecast-{index}", "sample"));
        }
        var report = LenientLeaveRequestForecastReport.BuildLenientLeaveRequestForecastReport("test", items, clock);
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
        LenientLeaveRequestForecastReportCountsEachRecordOnce();
    }
}
