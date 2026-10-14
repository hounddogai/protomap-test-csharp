namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualExceptionReportChecks
{
    public static void AccrualExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709398464);
        var items = new List<AccrualException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccrualException.NewAccrualException($"accrual-exception-{index}", "sample"));
        }
        var report = AccrualExceptionReport.BuildAccrualExceptionReport("test", items, clock);
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
        AccrualExceptionReportCountsEachRecordOnce();
    }
}
