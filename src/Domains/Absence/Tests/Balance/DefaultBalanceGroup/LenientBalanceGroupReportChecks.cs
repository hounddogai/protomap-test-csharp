namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceGroupReportChecks
{
    public static void LenientBalanceGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706429733);
        var items = new List<LenientBalanceGroup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientBalanceGroup.NewLenientBalanceGroup($"lenient-balance-group-{index}", "sample"));
        }
        var report = LenientBalanceGroupReport.BuildLenientBalanceGroupReport("test", items, clock);
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
        LenientBalanceGroupReportCountsEachRecordOnce();
    }
}
