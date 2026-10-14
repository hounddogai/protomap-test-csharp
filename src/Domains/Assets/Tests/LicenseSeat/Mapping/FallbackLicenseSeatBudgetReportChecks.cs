namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackLicenseSeatBudgetReportChecks
{
    public static void FallbackLicenseSeatBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701067728);
        var items = new List<FallbackLicenseSeatBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget($"fallback-license-seat-budget-{index}", "sample"));
        }
        var report = FallbackLicenseSeatBudgetReport.BuildFallbackLicenseSeatBudgetReport("test", items, clock);
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
        FallbackLicenseSeatBudgetReportCountsEachRecordOnce();
    }
}
