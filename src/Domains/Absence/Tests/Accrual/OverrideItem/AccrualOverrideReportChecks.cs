namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AccrualOverrideReportChecks
{
    public static void AccrualOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709114247);
        var items = new List<AccrualOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccrualOverride.NewAccrualOverride($"accrual-override-{index}", "sample"));
        }
        var report = AccrualOverrideReport.BuildAccrualOverrideReport("test", items, clock);
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
        AccrualOverrideReportCountsEachRecordOnce();
    }
}
