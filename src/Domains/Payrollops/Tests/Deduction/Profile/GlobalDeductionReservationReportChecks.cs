namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationReportChecks
{
    public static void GlobalDeductionReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704823066);
        var items = new List<GlobalDeductionReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalDeductionReservation.NewGlobalDeductionReservation($"global-deduction-reservation-{index}", "sample"));
        }
        var report = GlobalDeductionReservationReport.BuildGlobalDeductionReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalDeductionReservationReportCountsEachRecordOnce();
    }
}
