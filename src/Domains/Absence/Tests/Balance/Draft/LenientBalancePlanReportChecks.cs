namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanReportChecks
{
    public static void LenientBalancePlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702895159);
        var items = new List<LenientBalancePlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientBalancePlan.NewLenientBalancePlan($"lenient-balance-plan-{index}", "sample"));
        }
        var report = LenientBalancePlanReport.BuildLenientBalancePlanReport("test", items, clock);
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
        LenientBalancePlanReportCountsEachRecordOnce();
    }
}
